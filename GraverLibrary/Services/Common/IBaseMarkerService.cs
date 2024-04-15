using GraverLibrary.Models;
using GraverLibrary.Models.Common;
using GraverLibrary.Models.Enums;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GraverLibrary.Services.Common
{
    /// <summary>
    /// Для того чтобы добавить новый сервис для работы
    /// с другим гравером необходимо создать класс сервиса 
    /// и наследоваться от данного интерфейса
    /// </summary>
    public interface IBaseMarkerService 
    {
        Action<int> ProgressSendFile { get; set; }
        Action Connected { get; set; }
        Action Disconnected { get; set; }
        Action markingStarted { get; set; }
        Action markingStopped { get; set; }
        Action markingFinished { get; set; }
        Action HeightIsSet { get; set; }
        Action PowerModIsSet { get; set; }
        bool IsGrafAvailableForMarking { get; set; }
        void SendCommand(string command);
        Task SendCommandAsync(string command);
        string GetMarkingTime();
        string GetMarkingTimeWhileMarking();
        Task<bool> TryConnectAsync(IPAddress iPAddress, int port);
        void Disconnect();
        Task<bool> SendFile(FileStream fileStreamToLeFile);
        void CheckCurrentFile();
        string GetMainGroup();
        string GetFileName();
        string GetValue(string pathToProp);
        string SetValue(string pathToProp, string value);
        string MoveToCoordinateByX(float value);
        string MoveToCoordinateByY(float value);
        string GetCoordinateX();
        string GetCoordinateY();
        string StartMarkOnce();
        void StopMarking();
        string ShowRectangularJoystick();
        void SendCommandWithoutPrefix(string command);
        string SetHeightToObject(int height);
        string SetPowerMode(Material material);
        string SetObjectHeight(float height);
        string SetValueBeforeMarking(Order order);
    }
}
