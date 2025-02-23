using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthService.Infrastructure.Exceptions;

public class HighTimeExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not HighTimeException highTimeException) return;
        context.Result = new BadRequestObjectResult(highTimeException.Message)
        {
            StatusCode = 400
        };
        context.ExceptionHandled = true;
    }
}