using GraverLibrary.Models;
using GraverLibrary.Models.Enums;
using GraverLibrary.Services.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GraverLibrary.Services
{
    public class MockGraverService : IBaseMarkerService
    {
        private readonly ILogger<MockGraverService> _logger;

        public MockGraverService(ILogger<MockGraverService> logger = null)
        {
            _logger = logger;
        }

        public Action<int> ProgressSendFile { get; set; }
        public Action Connected { get; set; }
        public Action Disconnected { get; set; }

        public Action markingFinished { get; set; }
        public Action HeightIsSet { get; set; }
        public Action PowerModIsSet { get; set; }
        public Action markingStarted { get ; set; }
        public Action markingStopped { get ; set ; }
        public bool IsGrafAvailableForMarking { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Disconnect()
        {
            Disconnected?.Invoke();
            _logger?.LogInformation(nameof(Disconnect)  + " Mock");
        }

        public string GetCoordinateX()
        {
            _logger?.LogInformation(nameof(GetCoordinateX)  + " Mock");
            return "444";
        }

        public string GetCoordinateY()
        {
            _logger?.LogInformation(nameof(GetCoordinateY)  + " Mock");
            return "444";
        }

        public string GetFileName()
        {
            _logger?.LogInformation(nameof(GetFileName)  + " Mock");
            return "FileName";
        }

        public string GetMainGroup()
        {
            _logger?.LogInformation(nameof(GetMainGroup)  + " Mock");
            return "MainGroup";
        }

        public string GetMarkingTime()
        {
            _logger?.LogInformation(nameof(GetMarkingTime)  + " Mock");
            return "1.43";
        }

        public string GetMarkingTimeWhileMarking()
        {
            _logger?.LogInformation(nameof(GetMarkingTimeWhileMarking)  + " Mock");
            return "1.33";
        }

        public string GetValue(string pathToProp)
        {
            _logger?.LogInformation(nameof(GetValue)  + " Mock");
            return "444";
        }

        public string MoveToCoordinateByX(float value)
        {
            _logger?.LogInformation(nameof(MoveToCoordinateByX)  + " Mock");
            return "444";
        }

        public string MoveToCoordinateByY(float value)
        {
            _logger?.LogInformation(nameof(MoveToCoordinateByY)  + " Mock");
            return "444";
        }

        public void SendCommand(string command)
        {
            _logger?.LogInformation(nameof(SendCommand)  +  " Mock");
        }

        public Task SendCommandAsync(string command)
        {
            _logger?.LogInformation(nameof(SendCommandAsync)  + " Mock");
            return Task.CompletedTask;
        }

        public void SendCommandWithoutPrefix(string command)
        {
            _logger?.LogInformation(nameof(SendCommandWithoutPrefix)  + " Mock");
        }
        public void CheckCurrentFile()
        {
            _logger?.LogInformation(nameof(CheckCurrentFile) + " Mock");
        }
        public async Task<bool> SendFile(FileStream fileStreamToLeFile)
        {
            _logger?.LogInformation(nameof(SendFile)  + " Mock");
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                ProgressSendFile?.Invoke(i);
            }
            return await Task.FromResult(true);
        }

        public string SetHeightToObject(int height)
        {
            HeightIsSet?.Invoke();
            _logger?.LogInformation(nameof(SetHeightToObject)  + " Mock");
            return "444";
        }

        public string SetObjectHeight(float height)
        {
            _logger?.LogInformation(nameof(SetObjectHeight)  + " Mock");
            return "444";
        }

        public string SetPowerMode(Material material)
        {
            _logger?.LogInformation(nameof(SetPowerMode)  + " Mock");
            PowerModIsSet?.Invoke();
            return "444";
        }

        public string SetValue(string pathToProp, string value)
        {
            _logger?.LogInformation(nameof(SetValue)  + " Mock");
            return "444";
        }

        public string ShiftCoordinate(Axes axis, float value)
        {
            _logger?.LogInformation(nameof(ShiftCoordinate)  + " Mock");
            return "444";
        }

        public string ShowRectangularJoystick()
        {
            _logger?.LogInformation(nameof(ShowRectangularJoystick)  + " Mock");
            return "444";
        }

        public string StartMarkOnce()
        {
            markingStarted?.Invoke();
            _logger?.LogInformation(nameof(StartMarkOnce)  + " Mock");
            markingFinished?.Invoke();
            return "MarkingCompletedSuccessfully";
        }

        public void StopMarking()
        {
            _logger?.LogInformation(nameof(StopMarking)  + " Mock");
            markingStopped?.Invoke();
        }

        public Task<bool> TryConnectAsync(IPAddress iPAddress, int port)
        {
            _logger?.LogInformation(nameof(TryConnectAsync)  + " Mock");
            Connected?.Invoke();
            return Task.FromResult(true);
        }

        public string SetValueBeforeMarking(Order order)
        {
            _logger?.LogInformation(nameof(SetValueBeforeMarking) + " Mock");
            return "ok";
        }
    }
}
