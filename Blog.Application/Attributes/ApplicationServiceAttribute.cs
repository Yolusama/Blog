using Microsoft.AspNetCore.Mvc;

namespace Blog.Application.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ApplicationServiceAttribute(string route = "api/[controller]/[action]") : RouteAttribute(route)
{
    
}