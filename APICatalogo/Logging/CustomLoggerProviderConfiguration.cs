namespace APICatalogo.Logging
{
    public class CustomLoggerProviderConfiguration
    {
        //Define o nível mínimo de log a ser registrado, com o padrão LogLevel.Warning
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        //Dedfine o ID do evento de log, com o padrão sendo zero
        public int EventId { get; set; } = 0;
    }
}
