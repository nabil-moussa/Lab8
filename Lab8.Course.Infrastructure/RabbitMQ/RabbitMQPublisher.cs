using Lab8.Course.Common.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
namespace Lab8.Course.Infrastructure.RabbitMQ;

public class RabbitMQPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "course_exchange";

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

    public void PublishCourseCreated(CourseCreatedEvent courseEvent)
    {
        var message = JsonSerializer.Serialize(courseEvent);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: "course.created",
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
