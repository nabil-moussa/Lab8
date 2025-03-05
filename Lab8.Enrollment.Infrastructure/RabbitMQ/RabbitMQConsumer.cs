// Lab8.Enrollment.Infrastructure/RabbitMQ/RabbitMQConsumer.cs
using System.Text;
using System.Text.Json;
using Lab8.Enrollment.Common.Dtos;
using Lab8.Enrollment.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lab8.Enrollment.Infrastructure.RabbitMQ
{
    public class RabbitMQConsumer
    {
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQConsumer(
            IServiceProvider serviceProvider, 
            string hostname = "localhost",
            string queueName = "student_enrolled")
        {
            _hostname = hostname;
            _queueName = queueName;
            _serviceProvider = serviceProvider;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = _hostname };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var enrollmentEvent = JsonSerializer.Deserialize<StudentEnrolledEvent>(message);

                    Console.WriteLine($"Received message: {message}");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
                        await enrollmentService.ProcessStudentEnrolledEvent(enrollmentEvent);
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing RabbitMQ message: {ex.Message}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }

        public void Close()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}