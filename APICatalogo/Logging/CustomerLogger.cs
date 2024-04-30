
namespace APICatalogo.Logging;

public class CustomerLogger : ILogger
{
    readonly string loggerName;
    readonly CustomLoggerProviderConfiguration loggerConfig;

    public CustomerLogger(string name,
                          CustomLoggerProviderConfiguration config)
    {
        this.loggerName = name;
        this.loggerConfig = config;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    //VAI VERIFICAR SE O NIVEL DE LOG DESEJADO ESTÁ HABILITADO COM BASE NA CONFIGURAÇÃO
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == loggerConfig.LogLevel;
    }

    //Esse método é chamado pra registrar uma mensagem de LOG
    //vai verificar se o nível de log é permitido e se for o caso, vai formatar a mensagem e escrevê-la no arquivo de LOG
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                            Exception exception, Func<TState, Exception, string> formatter)
    {
        string mensagem = $"{logLevel.ToString()}: {eventId} - {formatter(state, exception)}";
        EscreverTextoNoArquivo(mensagem);
    }

    private void EscreverTextoNoArquivo(string mensagem)
    {
        //Esse é o caminho onde a mensagem vai ser registrada
        string caminhoArquivoLog = @"C:\Users\USER\OneDrive\Área de Trabalho\Curso_C#\LOG_API_CATALOGO.txt";

        using (StreamWriter streamWriter = new StreamWriter(caminhoArquivoLog, true))
        {
            try
            {
                streamWriter.WriteLine(mensagem);
                streamWriter.Close();
            } 
            catch(Exception)
            {
                throw;
            }
        }
    }
}
