using Blog.MQ.Services;
using Blog.MQ.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilogService();
builder.Services.AddFreeSql(builder.Configuration);

await builder.Services.AddRabbitMQ(builder.Configuration);

builder.Services.AddScoped<IArticleService, ArticleService>();

var app = builder.Build();

await app.RunAsync();