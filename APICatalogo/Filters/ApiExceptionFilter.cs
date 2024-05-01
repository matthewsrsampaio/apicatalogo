using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace APICatalogo.Filters
{
    //Cria uma mensagem de exceção GLOBAL evitando repetição de código
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            //A linha de baixo vai escrever no nosso LOG a mensagem abaixo em caso de uma exceção
            _logger.LogError(context.Exception, "Ocorreu uma exceção não tratada: Status Code 500");
            //Essa mensagem vai aparecer no body da response
            context.Result = new ObjectResult("Ocorreu um problema ao tratar a sua solicitação: Status Code 500")
            {
                //Esse é o código que vai ser retornado na response
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
    }
}
