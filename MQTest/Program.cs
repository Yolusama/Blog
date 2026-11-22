// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public struct TestA
{
    public int A { get; set; }
    public string B { get; set; }
    public bool C { get; set; }
}
class Program
{
    private static async Task Main(string[] args)
    {
        var connectionFactory = new ConnectionFactory();
        connectionFactory.HostName = "localhost";
        connectionFactory.UserName = "guest";
        connectionFactory.Password = "guest";
        connectionFactory.Port = 5672;
        connectionFactory.VirtualHost = "/";
        connectionFactory.ClientProvidedName = "Blog";
        var connection = await connectionFactory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(
            exchange: "Blog",
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        await channel.QueueDeclareAsync(
            queue: "Test",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {message = "hello,world!"}));
        string routingKey = "Test.Test";
        TestA test = new TestA();
        test.A = Random.Shared.Next();
        test.B = Guid.NewGuid().ToString();
        test.C = true;
        body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {model = test},new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
        await channel.QueueBindAsync("Test", "Blog", routingKey);
        //await channel.BasicPublishAsync("Blog", routingKey, false, body);
        await channel.BasicPublishAsync("Blog", routingKey, false, body);
    }
}

    