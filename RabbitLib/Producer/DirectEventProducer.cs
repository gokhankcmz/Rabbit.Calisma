using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitLib.Events;
using RabbitLib.Utility;
using RabbitMQ.Client.Exceptions;

namespace RabbitLib.Producer
{
    public class DirectEventProducer : IDirectEventProducer
    {
        
        private readonly ILogger<EventProducerManager> _logger;
        private readonly IRabbitMqPersistentConnection _persistentConnection;
        private readonly int _retryCount;

        public DirectEventProducer(ILogger<EventProducerManager> logger, IRabbitMqPersistentConnection persistentConnection, int retryCount)
        {
            _logger = logger;
            _persistentConnection = persistentConnection;
            _retryCount = retryCount;
        }

        public void Publish(QueueOptions queueOptions, EventBase eventBase, ExchangeOptions exchangeOptions = null)
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
                string exchangeName = "";
                if (exchangeOptions != null)
                {
                    channel.ExchangeDeclare(
                        exchangeOptions.ExchangeName,
                        type:"direct",
                        exchangeOptions.Durable, exchangeOptions.AutoDelete,
                        exchangeOptions.Arguments);
                    exchangeName = exchangeOptions.ExchangeName;
                }

                if (queueOptions.NewQueue)
                {
                    channel.QueueDeclare(queueOptions.QueueName, queueOptions.Durable, queueOptions.Exclusive, queueOptions.AutoDelete, queueOptions.Arguments);
                    channel.QueueBind(queueOptions.QueueName,exchangeName,arguments:null, routingKey:queueOptions.QueueName);
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
                        
                        exchange: exchangeName,
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
}