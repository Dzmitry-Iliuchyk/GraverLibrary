using System.Net;

namespace GraverLibrary.Models.Common
{
    /// <summary>
    /// Конфигурация подключения, необходим ip-адрес и порт
    /// </summary>
    public class ConnectionConfig
    {
        public IPAddress IpAddress { get; set; }
        public int Port { get; set; }
    }
}
