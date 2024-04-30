using System.Collections.Concurrent;

namespace APICatalogo.Logging
{
    public class CustomLoggerProvider : ILoggerProvider//Interface usada para criar logs personalizados
    {
        //CONFIGURAÇÃO
        readonly CustomLoggerProviderConfiguration loggerConfig;
        //DICIONÁRIO DE LOGGER DO TIPO <CHAVE, VALOR>
        readonly ConcurrentDictionary<string, CustomerLogger> loggers = new ConcurrentDictionary<string, CustomerLogger>();

        //CONSTRUTOR -> define a configuração de todos os loggers criados para este provedor
        public CustomLoggerProvider(CustomLoggerProviderConfiguration config)
        {
            this.loggerConfig = config;
        }

        //SERÁ USADO PARA CRIAR UM LOGGER PARA CADA CATEGORIA ESPECÍFICA
        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, name => new CustomerLogger(name, loggerConfig));
        }

        //lIBERA OS RECURSOS QUANDO O PROVEDOR FOR DESCARTADO
        public void Dispose()
        {
            loggers.Clear();
        }
    }
}
