using Blog.Domain.Services.UserDomain.Interfaces;
using Blog.Domain.UserDomain.ValueObjects;
using Blog.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers;

public class ProjectControllerBase : ControllerBase
{
    private const string AuthorizationHeaderName = "Authorization";
    [NonAction]
    public UserJwtInfo GetCurrentUser()
    {
        var jwtService = Request.HttpContext.RequestServices.GetRequiredService<IJwtService>();
        var token = Request.Headers[AuthorizationHeaderName].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
            throw new ServiceException("没有授权令牌",401);
        return jwtService.Parse<UserJwtInfo>(token);
    }
}