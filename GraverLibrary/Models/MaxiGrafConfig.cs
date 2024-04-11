using GraverLibrary.Models.Common;

namespace GraverLibrary.Models
{
    /// <summary>
    /// Конфигурация для подключения к MaxiGraf, для работы необходимо указать apiKey.
    /// </summary>
    public class MaxiGrafConfig : IGraverConfig
    {
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string ConnectionId { get; set; }
        public float FocusHeight { get; set; }

        public MaxiGrafConfig(string apiKey, float focusHeight)
        {
            Name = "MaxiGraf";
            ApiKey = apiKey;
            FocusHeight = focusHeight;
        }

        public string GetPrefix()
        {
            return $"{ApiKey}|{ConnectionId}|";
            
        }
    }
}
