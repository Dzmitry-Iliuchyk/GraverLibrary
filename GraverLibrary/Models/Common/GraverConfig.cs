namespace GraverLibrary.Models.Common
{
    /// <summary>
    /// Конфигурация для подключения к граверу
    /// </summary>
    public interface IGraverConfig
    {
        string Name { get; set; }
        string ApiKey { get; set; }
        string ConnectionId { get; set; }
        float FocusHeight {  get; set; }
        string GetPrefix();
    }
}
