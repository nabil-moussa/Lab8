using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Lab8.Enrollment.Infrastructure.RabbitMQ
{
    public class RabbitMQPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;

        public RabbitMQPublisher(string hostName, string exchangeName)
        {
            var factory = new ConnectionFactory() { HostName = hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _exchangeName = exchangeName;

            // Declare exchange
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
        }

        private void PublishMessage(string routingKey, object message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body);
        }

        public void PublishEnrollmentCreated(object enrollmentCreatedEvent)
        {
            PublishMessage("enrollment.created", enrollmentCreatedEvent);
        }

        public void PublishEnrollmentDeleted(object enrollmentDeletedEvent)
        {
            PublishMessage("enrollment.deleted", enrollmentDeletedEvent);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}