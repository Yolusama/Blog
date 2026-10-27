using System.Diagnostics;
using System.Text.Json;
using Blog.Infrastructure;
using Blog.Shared;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Middlewares;

/// <summary>
/// Api 审计日志中间件。
/// 只记录 /api 下、且命中 Controller Action 的请求：
/// 根目录、Swagger、静态资源、404、CORS 预检等一律跳过。
/// </summary>
public class ProjectAuditLogMiddleware(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<ProjectAuditLogMiddleware> logger,
    RequestDelegate next)
{
    private const string ApiPathPrefix = "/api";
    private const string SwaggerPathPrefix = "/swagger";
    
    
    private static readonly JsonSerializerOptions ParameterJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!ShouldAudit(httpContext, out var action))
        {
            await next(httpContext);
            return;
        }

        var userAgent = httpContext.Request.Headers.UserAgent;
        var log = new ProjectAuditLog
        {
            ExecutionTime = DateTime.Now,
            ActionType = httpContext.Request.Method,
            ActionName = action.ActionName,
            ServiceName = action.ControllerTypeInfo.FullName,
            Route = httpContext.Request.Path.Value ?? string.Empty,
            RemoteIpAddr = GetIpAddress(httpContext) ?? string.Empty,
            ActionDesc = action.DisplayName,
            ParameterJson = await ReadParameterJsonAsync(httpContext,action) ?? string.Empty,
            ClientInfo = userAgent.ToString()
        };

        var stopwatch = Stopwatch.StartNew();
        string? exception =null;

        try
        {
            await next(httpContext);
        }
        catch (Exception e)
        {
            exception = e.ToString();
            throw;
        }
        finally
        {
            stopwatch.Stop();
            log.ExecutionDuration = (int)stopwatch.ElapsedMilliseconds;
            log.Exception = exception;
            await WriteLogAsync(log);
        }
    }

    /// <summary>
    /// 判断当前请求是否需要记录审计日志
    /// </summary>
    private bool ShouldAudit(HttpContext context, out ControllerActionDescriptor? action)
    {
        // 只有命中 Controller Action 的请求才算 Api，
        // 根目录、Swagger 端点、静态文件中间件、404 等取不到这个 Metadata
        if (context.Request.Method == HttpMethods.Options)
        {
            action = null;
            return false;
        }
        action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (action is null)
            return false;

        // CORS 预检请求不记录
        if (HttpMethods.IsOptions(context.Request.Method))
            return false;

        var path = context.Request.Path;

        // 只记录 /api 下的请求
        if (!path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        // 兜底排除 Swagger
        if (path.StartsWithSegments(SwaggerPathPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        // 兜底排除静态资源（配置项 StaticFiles:RequestPath）
        var staticRequestPath = configuration["StaticFiles:RequestPath"];
        if (!string.IsNullOrWhiteSpace(staticRequestPath)
            && path.StartsWithSegments(staticRequestPath, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    /// <summary>
    /// 组合路由参数、Query、请求体并序列化
    /// </summary>
    private static async Task<string?> ReadParameterJsonAsync(HttpContext context,ActionDescriptor? actionDescriptor)
    {
        var parameters = new Dictionary<string,object?>();
        foreach (var key in context.Request.RouteValues.Keys.Where(k=>k!="action" && k!="controller"))
            parameters.Add(key, context.Request.RouteValues[key]);
        foreach (var key in context.Request.Query.Keys)
        {
            var value = context.Request.Query[key];
            parameters.Add(key, value.Count == 1 ? value.First() : value);
        }
        if(context.Request.Method != HttpMethods.Get || context.Request.Method != HttpMethods.Head
           || context.Request.Method != HttpMethods.Delete)
        {
            if (context.Request.HasFormContentType)
            {
                var form = context.Request.Form;
                foreach (var formItem in form)
                    parameters.Add(formItem.Key, formItem.Value);
                foreach (var fileGroup in form.Files.GroupBy(f => f.Name))
                {
                    var key = fileGroup.Key;
                    var files = fileGroup.Select(f=>new
                    {
                        f.FileName,
                        f.Length,
                        f.ContentType,
                        f.ContentDisposition,
                        f.Headers
                    }).ToList();
                    if(files.Count == 1)
                        parameters.Add(key,files.First());
                    else
                        parameters.Add(key,files);
                    
                }
            }
            else
            {
                context.Request.EnableBuffering();
                var bodyParameters = actionDescriptor.Parameters;
                if (bodyParameters.Count > 0)
                {
                    var bodyParameter = bodyParameters.FirstOrDefault(e =>
                        e.BindingInfo.BindingSource == BindingSource.Body);
                    if (bodyParameter != null)
                    {
                        var obj = await JsonSerializer
                            .DeserializeAsync(context.Request.Body, bodyParameter.ParameterType, 
                                ParameterJsonOptions);
                        parameters.Add(bodyParameter.Name, obj);
                    }
                }
            }

            context.Request.Body.Position = 0;
        }
        
        
        return parameters.Count == 0
            ? "{}"
            : JsonSerializer.Serialize(parameters, ParameterJsonOptions);
    }

    private static string? GetIpAddress(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        var ip = context.Connection.RemoteIpAddress;
        if (ip is null)
            return null;

        return ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString();
    }

    private async Task WriteLogAsync(ProjectAuditLog log)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SysDbContext>();
            var logs = dbContext.Set<ProjectAuditLog>();
            await logs.AddAsync(log);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            // 审计失败不能影响业务请求
            logger.LogError(e, "审计日志写入失败，路由：{Route}", log.Route);
        }
    }
}
