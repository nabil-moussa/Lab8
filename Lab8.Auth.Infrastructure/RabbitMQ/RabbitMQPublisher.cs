using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Lab8.Auth.Common.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lab8.Auth.Infrastructure.RabbitMQ;

public class RabbitMQPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "auth_exchange";

    public RabbitMQPublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, durable: true);
    }

    public void PublishUserEvent<T>(T message, string routingKey)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<RabbitMQPublisher>();
        return services;
    }
}