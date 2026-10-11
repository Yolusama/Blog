using Microsoft.AspNetCore.Authorization;

namespace Blog.Application.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
[Authorize]
public class AuthorizedServiceAttribute(string route="api/[controller]/[action]") : ApplicationServiceAttribute(route)
{
    
}