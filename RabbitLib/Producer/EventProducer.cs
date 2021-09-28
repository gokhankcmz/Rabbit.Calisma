/*using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitLib.Events;
using RabbitLib.Utility;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ExchangeType = RabbitMQ.Client.ExchangeType;

namespace RabbitLib.Producer
{
    public class EventProducer 
    {
        private readonly ILogger<EventProducer> _logger;
        private readonly IRabbitMqPersistentConnection _persistentConnection;
        private readonly int _retryCount;

        public EventProducer(IRabbitMqPersistentConnection persistentConnection, ILogger<EventProducer> logger)
        {
            _persistentConnection = persistentConnection;
            _logger = logger;
            _retryCount = _persistentConnection.GetRetryCount();
        }
        
        
        public void Publish(QueueOptions queueOptions,EventBase eventBase, ExchangeOptions exchangeOptions = null, string routingKey=null)
        {
            if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex,
                            "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})",
                            $"{time.TotalSeconds:n1}", ex);
                    });
            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueDeclare(queueOptions.QueueName, queueOptions.Durable, queueOptions.Exclusive, queueOptions.AutoDelete, queueOptions.Arguments);
                string exchangeName = "";
                if (exchangeOptions != null)
                {
                    channel.ExchangeDeclare(
                        exchangeOptions.ExchangeName,
                        Enum.GetName(typeof(ExchangeTypeEnum), exchangeOptions.Type)?.ToLower(),
                        exchangeOptions.Durable, exchangeOptions.AutoDelete,
                        exchangeOptions.Arguments);
                    exchangeName = exchangeOptions.ExchangeName;
                }


                var message = JsonSerializer.Serialize(eventBase);
                var body = Encoding.UTF8.GetBytes(message);
                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;
                    channel.ConfirmSelect();
                    channel.BasicPublish(
                        
                        exchange: exchangeOptions.ExchangeName,
                        routingKey:queueOptions.QueueName, 
                        mandatory:true,
                        basicProperties:properties,
                        body);
                    channel.WaitForConfirmsOrDie();
                    channel.BasicAcks += (sender, eventArgs) =>
                    {
                        Console.WriteLine("Sent. RabbitMQ");
                        //Ack implementation.
                    };
                });
            }
        }
    }
}*/