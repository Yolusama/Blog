using Blog.MQ.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilogService();

await builder.Services.AddRabbitMQ(builder.Configuration);

var app = builder.Build();

await app.RunAsync();