
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lab8.Course.Common.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Lab8.Course.Infrastructure.RabbitMQ;

public class RabbitMQConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "course_exchange";
    private readonly string _queueName = "course_queue";

    public RabbitMQConsumer(IConfiguration configuration)
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
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_queueName, _exchangeName, "course.created");
    }

    public void StartConsuming(Action<CourseCreatedEvent> processMessage)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var courseEvent = JsonSerializer.Deserialize<CourseCreatedEvent>(message);
            
            if (courseEvent != null)
            {
                processMessage(courseEvent);
            }
            
            _channel.BasicAck(args.DeliveryTag, false);
        };
        
        _channel.BasicConsume(_queueName, false, consumer);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}