using GraverLibrary.Models;
using GraverLibrary.Models.Common;
using GraverLibrary.Services.Common;
using GraverLibrary.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GraverLibrary.Services
{
    /// <summary>
    /// Реализация класса BaseMarkerService для управления MaxiGraf-ом
    /// </summary>
    public class MaxiGrafService : IBaseMarkerService
    {
        private const string RESULT_SUCCESS = "0";
        private readonly float focusHeight_mm;
        public bool IsGrafAvailableForMarking { get; set; }= false;
        private float oldHeight = 0;

        private NetworkStream networkStream;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private TcpClient client;

        private readonly IGraverConfig _graverConfig;
        private readonly MarkInfo _markInfo;
        private readonly ILogger<MaxiGrafService> _logger;

        public Action markingFinished { get; set; }
        public Action Connected { get; set; }
        public Action Disconnected { get; set; }
        public Action<int> ProgressSendFile { get; set; }
        public Action HeightIsSet { get; set; }
        public Action PowerModIsSet { get; set; }
        public Action markingStarted { get; set; }
        public Action markingStopped { get; set; }

        public MaxiGrafService(MaxiGrafConfig maxiGrafConfig, MarkInfo markInfo, ILogger<MaxiGrafService> logger = null)
        {
            _graverConfig = maxiGrafConfig;
            _markInfo = markInfo;
            focusHeight_mm = _graverConfig.FocusHeight;
            _logger = logger;
            markingFinished += OnMarkingFinished;
        }

        public async Task<bool> TryConnectAsync(IPAddress iPAddress, int port)
        {
            
                client = new TcpClient();
                while (!client.Connected)
                {
                    try
                    {
                        _logger?.LogInformation(nameof(TryConnectAsync) + " connecting");
                        client.Connect(iPAddress, port);
                        OnConnectToMaxiGraph();
                    }
                    catch (SocketException ex)
                    {
                        _logger?.LogError(nameof(TryConnectAsync) + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(nameof(TryConnectAsync) + ex.Message);
                    }
                }
                _logger?.LogInformation(nameof(TryConnectAsync) + " connected");
                return client.Connected;
            
        }
        private void OnConnectToMaxiGraph()
        {
            try
            {
                networkStream = client.GetStream(); // получаем поток
            }
            catch (InvalidOperationException ex)
            {
                _logger?.LogError(nameof(OnConnectToMaxiGraph) + "\r\n" + ex.Message);
                throw;
            }

            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);

            //Отпрaвляем ApiKey

            streamWriter.WriteLine(_graverConfig.ApiKey);
            streamWriter.Flush();
            _graverConfig.ConnectionId = streamReader.ReadConnectionId();

            SendCommandWithoutPrefix(API_String_Keys.Api_Tcp_Prefix + _graverConfig.GetPrefix().Length);
            _logger?.LogInformation(nameof(OnConnectToMaxiGraph) + "\r\nPrefix: " + _graverConfig.GetPrefix());

            _markInfo.MarkingTime = GetMarkingTime();
            _markInfo.MainGroup = GetMainGroup();
            Connected?.Invoke();
        }

        public void SendCommandWithoutPrefix(string command)
        {

            _logger?.LogInformation(nameof(SendCommandWithoutPrefix) + " \r\n Sent command:" + command);
            streamWriter.WriteLine(command);
            streamWriter.Flush();
        }

        public void Disconnect()
        {
            _logger?.LogInformation(nameof(SendCommandWithoutPrefix) + " \r\n Disconnect:");
            Disconnected?.Invoke();
            SendCommand(API_String_Keys.Disconnect_And_Wait_For_New);
        }

        public string GetFileName()
        {
            _logger?.LogInformation(nameof(GetFileName));
            return SendCommandAndReceiveAnswer(API_String_Keys.Get_File_Name);
        }

        public string GetMarkingTime()
        {
            _logger?.LogInformation(nameof(GetMarkingTime));
            return SendCommandAndReceiveAnswer(API_String_Keys.Get_Marking_Time);

        }
        public string GetMarkingTimeWhileMarking()
        {
            _logger?.LogInformation(nameof(GetMarkingTimeWhileMarking));
            return SendCommandAndReceiveAnswer(API_String_Keys.Get_Marking_Time_While_Marking);
        }

        public string GetMainGroup()
        {
            _logger?.LogInformation(nameof(GetMainGroup));
            var response = SendCommandAndReceiveAnswer(API_String_Keys.GetObjects);
            return response.Split('|')[0];
        }
        /// <summary>
        /// Получить значение свойства обьекта.
        /// </summary>
        /// <param name="pathToProp">Путь к обьекту и свойству из дерева объектов MaxiGraf </param>
        /// <returns> Значение свойства или код ошибки</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetValue(string pathToProp)
        {
            _logger?.LogInformation(nameof(GetValue));
            var status = SendCommandAndReceiveAnswer(((MaxiGrafConfig)_graverConfig).GetPrefix()
                + $"{API_String_Keys.Get_Value_TCP}{pathToProp}");
            if (status == RESULT_SUCCESS)
            {
                var result = streamReader.ReadLineCustom();
                return result;
            }
            return "error";
        }

        public void SendCommand(string command)
        {
            _logger?.LogInformation(nameof(SendCommand) + " \r\n Sent command:" + command);
            streamWriter.WriteLine(((MaxiGrafConfig)_graverConfig).GetPrefix() + command);
            streamWriter.Flush();
        }
        public async Task SendCommandAsync(string command)
        {
            _logger?.LogInformation(nameof(SendCommandAsync) + " \r\n Sent command:" + command);
            await streamWriter.WriteLineAsync(((MaxiGrafConfig)_graverConfig).GetPrefix() + command);
            await streamWriter.FlushAsync();
        }
        public void CheckCurrentFile()
        {
            _markInfo.MarkingTime = GetMarkingTime();
            _markInfo.MainGroup = GetMainGroup();
        }

        public async Task<bool> SendFile(FileStream fileStreamToLeFile)
        {
            _logger?.LogInformation(nameof(SendFile));
            const int packageDelay = 500;
            double percent = 0;
            string command = "This is a LE file";
            await SendCommandAsync(((MaxiGrafConfig)_graverConfig).GetPrefix() + command);

            BinaryReader reader = new BinaryReader(fileStreamToLeFile);

            byte[] dataPrefix = Encoding.UTF8.GetBytes(_graverConfig.GetPrefix());
            byte[] UB = dataPrefix;
            int i = 0;

            while (reader.BaseStream.Position < reader.BaseStream.Length - 256)
            {
                i++;
                var inputBuff = reader.ReadBytes(256);
                UB = dataPrefix;
                Array.Resize(ref UB, UB.Length + inputBuff.Length);
                Array.Copy(inputBuff, 0, UB, UB.Length - inputBuff.Length, inputBuff.Length);
                await networkStream.WriteAsync(UB, 0, UB.Length);
                await Task.Delay(packageDelay);
                percent = 256.0 / reader.BaseStream.Length * 100 * i;
                ProgressSendFile?.Invoke((int)percent);
            }

            int len = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
            if (len > 0)
            {
                var inputBuff = reader.ReadBytes(len);
                UB = dataPrefix;
                Array.Resize(ref UB, UB.Length + inputBuff.Length);
                Array.Copy(inputBuff, 0, UB, UB.Length - inputBuff.Length, inputBuff.Length);
                await networkStream.WriteAsync(UB, 0, UB.Length);
                await Task.Delay(packageDelay);

            }

            command = "This is the end of file";
            ProgressSendFile?.Invoke(100);
            await streamWriter.WriteLineAsync(_graverConfig.GetPrefix() + command);
            await streamWriter.FlushAsync();
            reader.Close();
            await Task.Delay(packageDelay);

            //Считываем ответ
            var response = streamReader.ReadLineCustom();
            if (!response.Contains("LE success"))
            {
                await Task.Delay(50);
                return false;
            }
            _markInfo.MarkingTime = GetMarkingTime();
            await Task.Delay(50);

            _markInfo.MainGroup = GetMainGroup();
            return true;
        }

        public string SetValue(string pathToProp, string value)
        {
            _logger?.LogInformation(nameof(SetValue));
            return SendCommandAndReceiveAnswer(_graverConfig.GetPrefix()
                + $"{API_String_Keys.Set_New_Value_TCP}{pathToProp}={value}");
        }

        public string MoveToCoordinateByX(float value)
        {
            _logger?.LogInformation(nameof(MoveToCoordinateByX));
            return SendCommandAndReceiveAnswer(
                $"{API_String_Keys.Set_New_Value_TCP}{_markInfo.MainGroup}.PosOfAnchorX={value}");
        }

        public string MoveToCoordinateByY(float value)
        {
            _logger?.LogInformation(nameof(MoveToCoordinateByY));
            return SendCommandAndReceiveAnswer(
                $"{API_String_Keys.Set_New_Value_TCP}{_markInfo.MainGroup}.PosOfAnchorY={value}");
        }

        public string GetCoordinateX()
        {
            _logger?.LogInformation(nameof(GetCoordinateX));
            string result = GetValue($"{_markInfo.MainGroup}.PosOfAnchorX=");
            return result;
        }

        public string GetCoordinateY()
        {
            _logger?.LogInformation(nameof(GetCoordinateY));
            string result = GetValue($"{_markInfo.MainGroup}.PosOfAnchorY=");
            return result;
        }

        public string SetHeightToObject(int height)
        {
            _logger?.LogInformation(nameof(SetHeightToObject) + "HeightToObject: " + height);
            //oldHeight = focusHeight_mm - height;
            HeightIsSet?.Invoke();
            SendCommand("MoveAxis=Z" + (focusHeight_mm - height));
            return "Z";
        }
        [Obsolete]
        public string SetObjectHeight(float height)
        {
            _logger?.LogInformation(nameof(SetObjectHeight) + "Height: " + height + "OldHeight: " + oldHeight);
            (height, oldHeight) = (height - oldHeight, height);
            SendCommand("MoveAxis=Z" + height);
            return "Z";
        }

        public string ShowRectangularJoystick()
        {
            _logger?.LogInformation(nameof(ShowRectangularJoystick));
            return SendCommandAndReceiveAnswer(API_String_Keys.Show_Rectangular_Joystick);
        }

        public void StopMarking()
        {
            _logger?.LogInformation(nameof(StopMarking));
            SendCommand(API_String_Keys.Stop);
            IsGrafAvailableForMarking = true;
            markingStopped?.Invoke();
        }
        public string SendCommandAndReceiveAnswer(string command)
        {
            _logger?.LogInformation(nameof(SendCommandAndReceiveAnswer) + " \r\n Sent command:" + command);
            streamWriter.Write(_graverConfig.GetPrefix() + command);
            streamWriter.Flush();
            var result = streamReader.ReadLineCustom();
            _logger?.LogInformation(nameof(SendCommandAndReceiveAnswer) + " \r\nResult:" + result);
            return result;
        }


        public string StartMarkOnce()
        {
            IsGrafAvailableForMarking = false;
            markingStarted?.Invoke();
            _logger?.LogInformation(nameof(StartMarkOnce));
            if (!IsGrafAvailableForMarking)
            {
                return "Minimarker is not ready";
            }
            string command = "Start mark";

            var MarkingResult = SendCommandAndReceiveAnswer(command);

            markingFinished?.Invoke();

            return MarkingResult;
        }

        public string SetPowerMode(Material material)
        {
            _logger?.LogInformation(nameof(SetPowerMode));
            var power = SetValue($"{_markInfo.MainGroup}.Power", material.Power.ToString());
            var speed = SetValue($"{_markInfo.MainGroup}.Speed", material.Speed.ToString());
            var frequency = SetValue($"{_markInfo.MainGroup}.Freq", material.Frequency.ToString());
            PowerModIsSet?.Invoke();
            if (power == RESULT_SUCCESS && speed == RESULT_SUCCESS && frequency == RESULT_SUCCESS)
            {
                return $"\t\tName: {material.Name}\r\n" +
                    $" Parameters: power: {material.Power}%" +
                    $", speed: {material.Speed} mm/s" +
                    $", frequency: {material.Frequency} kHz";
            }
            return "Error";
        }

        private void OnMarkingFinished()
        {
            _logger?.LogInformation(nameof(OnMarkingFinished));
            IsGrafAvailableForMarking = true;
        }

        public string SetValueBeforeMarking(Order order)
        {
            _logger?.LogInformation(nameof(SetPowerMode));
            var barCode = SetValue($"{_markInfo.MainGroup}\\barcode.Power", order.BarCodeValue);
           
            if (barCode == RESULT_SUCCESS)
            {
                order.IsMarked = true;
                return $"{barCode}";
                    
            }
            return "Error";
        }
    }
}
