// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

var connectionFactory = new ConnectionFactory();
connectionFactory.HostName = "localhost";
connectionFactory.UserName = "guest";
connectionFactory.Password = "guest";
connectionFactory.Port = 5672;
connectionFactory.VirtualHost = "/";
connectionFactory.ClientProvidedName = "Blog";
var connection = await connectionFactory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {message = "hello,world!"}));
string routingKey = "Test.Index";
await channel.BasicPublishAsync("Blog", routingKey, false, body);
    