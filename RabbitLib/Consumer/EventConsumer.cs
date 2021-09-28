using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitLib.Constants;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitLib.Consumer
{
    public class EventConsumer
    {
        private readonly IRabbitMqPersistentConnection _persistentConnection;

        public EventConsumer(IRabbitMqPersistentConnection persistentConnection)
        {
            _persistentConnection = persistentConnection;
        }

        public void Consume(string queue)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();
            channel.QueueDeclare(queue, false, false, false, null);
            var consumer = new EventingBasicConsumer(channel);
            
            consumer.Received += RecievedEvent;

            channel.BasicConsume(queue, autoAck:true, consumer: consumer);
        }

        private void RecievedEvent(object sender, BasicDeliverEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Body.Span);
            Console.WriteLine(message);
        }

        public void Disconnect()
        {
            _persistentConnection.Dispose();
        }
    }
}