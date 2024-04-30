using Microsoft.AspNetCore.Mvc.Filters;

namespace APICatalogo.Filters;

public class ApiLoggingFilter : IActionFilter
{
    private readonly ILogger<ApiLoggingFilter> _logger;

    public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
    {
        _logger = logger;
    }

    //Executa antes da Action
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("### Executando -> OnActionExecuting");
        _logger.LogInformation("##########################################################");
        _logger.LogInformation($"{DateTime.Now.ToLongTimeString()}");
        _logger.LogInformation($"Request Method: {context.HttpContext.Request.Method}");
        _logger.LogInformation($"Path: {context.HttpContext.Request.Path}");
        _logger.LogInformation($"Hash Code: {context.HttpContext.Request.GetHashCode()}");
        _logger.LogInformation($"ModedlState: {context.ModelState.IsValid}");
        _logger.LogInformation("##########################################################");
    }

    //Executa depois da Action
    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("### Executando -> OnActionExecuted");
        _logger.LogInformation("##########################################################");
        _logger.LogInformation($"{DateTime.Now.ToLongTimeString()}");
        _logger.LogInformation($"Status Code: {context.HttpContext.Response.StatusCode}");
        _logger.LogInformation($"Hash Code: {context.HttpContext.Response.GetHashCode()}");
        _logger.LogInformation($"ModedlState: {context.ModelState.IsValid}");
        _logger.LogInformation("##########################################################");
    }

}
