using Blog.MQ.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilogService();
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddServices();

await builder.Services.AddRabbitMQ(builder.Configuration);


var app = builder.Build();

await app.RunAsync();