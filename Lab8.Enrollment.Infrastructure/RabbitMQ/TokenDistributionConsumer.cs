using System.Text;
using System.Text.Json;
using Lab8.Enrollment.Common.RabbitMQ;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class TokenDistributionConsumer
{
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly ILogger<TokenDistributionConsumer> _logger;
    private readonly IMemoryCache _cache;

    public TokenDistributionConsumer(
        ILogger<TokenDistributionConsumer> logger,
        IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
        _queueName = "token_distribution_queue";
        
        try 
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.QueueDeclare(
                queue: _queueName, 
                durable: true, 
                exclusive: false
            );

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
            throw;
        }
    }

    public void StartConsuming()
    {
        try 
        {
            _channel.QueueBind(
                queue: _queueName,
                exchange: "auth_exchange", 
                routingKey: "token.distributed"
            );

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                try 
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    _logger.LogInformation($"Raw received message: {message}");

                    var tokenEvent = JsonSerializer.Deserialize<UserAuthenticatedEvent>(message);

                    _logger.LogInformation($"Deserialized token event - Username: {tokenEvent.Username}");
                    _logger.LogInformation($"Token: {tokenEvent.JwtToken}");

                    // Cache the token with a sliding expiration
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(1))
                        .SetAbsoluteExpiration(TimeSpan.FromHours(2));

                    _cache.Set("AuthToken", tokenEvent.JwtToken, cacheEntryOptions);
                    _cache.Set("Username", tokenEvent.Username, cacheEntryOptions);

                    _logger.LogInformation($"Received and cached token for user: {tokenEvent.Username}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing token distribution: {ex.Message}");
                    _logger.LogError($"Exception details: {ex}");
                }
            };

            _channel.BasicConsume(
                queue: _queueName, 
                autoAck: true, 
                consumer: consumer
            );

            _logger.LogInformation("Started consuming tokens from queue");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming tokens");
            throw;
        }
    }
}