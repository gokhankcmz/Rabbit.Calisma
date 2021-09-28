using Microsoft.Extensions.Logging;

namespace RabbitLib.Producer
{
    public class EventProducerManager : IEventProducerManager
    {
        
        private readonly IRabbitMqPersistentConnection _persistentConnection;
        private readonly ILogger<EventProducerManager> _logger;
        private readonly int _retryCount;
        
        
        private IDirectEventProducer _direct;
        private IFanoutEventProducer _fanout;
        private ITopicEventProducer _topic;

        
        
        public EventProducerManager(IRabbitMqPersistentConnection persistentConnection, ILogger<EventProducerManager> logger)
        {
            _persistentConnection = persistentConnection;
            _logger = logger;
            _retryCount = _persistentConnection.GetRetryCount();
        }
        
        public IDirectEventProducer DirectEventProducer => _direct ?? new DirectEventProducer(_logger, _persistentConnection, _retryCount);
        public IFanoutEventProducer FanoutEventProducer => _fanout ?? new FanoutEventProducer(_logger, _persistentConnection, _retryCount);
        public ITopicEventProducer TopicEventProducer => _topic ?? new TopicEventProducer(_logger, _persistentConnection, _retryCount);
    }
}