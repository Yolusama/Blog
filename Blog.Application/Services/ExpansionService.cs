using System.Reflection;
using System.Text;
using Blog.Application.Attributes;
using Blog.Application.Services.UserDomain;
using Blog.Application.Services.UserDomain.Configuration;
using Blog.Domain.Services.UserDomain.Interfaces;
using Blog.Domain.UserDomain.Services.Interfaces;
using Blog.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

namespace Blog.Application.Services;

public static class ExpansionService
{
    /// <summary>
    /// 开始自动检查注入配置类
    /// </summary>
    /// <param name="app"></param>
    public static void EnableAutoConfig(this IServiceCollection services,IConfiguration config)
    {
        var configTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(e=>e.GetCustomAttribute<ConfigurationAttribute>() != null)
            .ToList();
        
        foreach (var configType in configTypes)
        {
            var attr = configType.GetCustomAttribute<ConfigurationAttribute>();
            var key = string.IsNullOrEmpty(attr.Key) ? configType.Name : attr.Key;
            var configInstance = Activator.CreateInstance(configType);
            var properties = configType.GetProperties();
            var root = config.GetSection(key);
            foreach (var property in properties) 
            {
                var value = root.GetSection(property.Name).Get(property.PropertyType);
                property.SetValue(configInstance, value);
            }
            services.AddSingleton(configType, configInstance);
        }
    }
    
    public static void AddSerilogService(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        try
        {
            Log.Logger.Information("初始化Serilog日志记录器...");  
            services.AddSerilog((provider, lc) =>
            {
                lc.MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("Logs/info-.log", LogEventLevel.Information, rollingInterval: RollingInterval.Hour)
                    .WriteTo.File("Logs/error-.log", LogEventLevel.Error, rollingInterval: RollingInterval.Hour)
                    .WriteTo.File("Logs/debug-.log", LogEventLevel.Debug, rollingInterval: RollingInterval.Hour);
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// 使用自动注入
    /// </summary>
    /// <param name="app"></param>
    public static void DoAutoServiceRegister(this WebApplicationBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes()
            .Where(t=>t.GetCustomAttribute<ServiceInjectionAttribute>()!=null)
            .ToList();
        var interfaces = types.Where(t => t.IsInterface);
        var implementations = types.Where(t => t.IsClass && !t.IsAbstract);
        foreach (var type in implementations)
        {
            var interfaceType = interfaces.FirstOrDefault(t => t.IsAssignableFrom(type));
            var implAttr = type.GetCustomAttribute<ServiceInjectionAttribute>();
            builder.Services.Add(interfaceType != null
                ? new ServiceDescriptor(interfaceType, type, implAttr.ServiceLifetime)
                : new ServiceDescriptor(type, implAttr.ServiceLifetime));
        }
    }
    
    public static void AddJwt(this IServiceCollection services)
    {
        services.AddSingleton<IJwtService, JwtService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtConfig = services.BuildServiceProvider().GetRequiredService<JwtConfig>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
                };
            });
        services.AddAuthorization();
    }

    public static void UseStaticFileRequest(this WebApplication app)
    {
        var directory = new DirectoryInfo(app.Configuration["StaticFiles:BasePath"]);
        if(!directory.Exists)
            directory.Create();
        app.UseStaticFiles(new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
            FileProvider = new PhysicalFileProvider(directory.FullName),
            RequestPath = app.Configuration["StaticFiles:RequestPath"],
            DefaultContentType = Constants.FileResponseHeader
        });
    }

    public static void AddEmailSender(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, EmailSender>();
    }
}