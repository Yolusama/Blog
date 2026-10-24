using Blog.Application.Services;
using Blog.HostedServices;
using Blog.Infrastructure;
using Blog.Infrastructure.Expansions;
using Blog.Middlewares;
using Blog.Services.Filters;
using Blog.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.EnableAutoConfig(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionHandler>();
    options.Filters.Add<UnitOfWorkFilter>();
    options.Filters.Add<ResponseResultFilter>();
});
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddRepository();
builder.Services.AddHttpClient();
builder.Services.AddJwt();
builder.Services.AddSerilogService();
builder.Services.AddDomainEventChannel();
builder.Services.AddEmailSender();

builder.Services.AddRedis(builder.Configuration
    .GetConnectionString(Constants.Redis)
);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<UnitOfWorkFilter>();
builder.Services.AddScoped<GlobalExceptionHandler>();
builder.Services.AddScoped<ResponseResultFilter>();

builder.Services.AddDbContext<SysDbContext>(options =>
{
    var dbConnectionStr =  builder.Configuration.GetConnectionString(Constants.SySDb);
    options.UseMySql(dbConnectionStr,
       new MySqlServerVersion(new Version(9,4,0)));
});
builder.Services.AddHostedService<DomainEventWatcher>();

builder.Services.AutoServiceRegister();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseStaticFileRequest();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ProjectAuditLogMiddleware>();

app.MapControllers();

app.Run();
