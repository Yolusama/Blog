using Blog.Shared;
using Blog.Shared.Exceptions;
using Blog.Shared.Functional;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Blog.Services.Filters;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        context.ExceptionHandled = true;
        logger.LogError("{e}", context.Exception);
        context.HttpContext.Response.ContentType = "application/json";
        var failResult = ResponseResult.Fail(context.Exception.Message);
        if(context.Exception is ServiceException serviceException)
            failResult.Code = serviceException.Code;
        await context.HttpContext.Response
            .WriteAsync(failResult.ToJson(Constants.SmallCamelcase));
    }
}