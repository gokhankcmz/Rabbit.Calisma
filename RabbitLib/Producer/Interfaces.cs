using RabbitLib.Events;
using RabbitLib.Utility;

namespace RabbitLib.Producer
{
    public interface IEventProducerManager
    {
        IDirectEventProducer DirectEventProducer { get; }
        IFanoutEventProducer FanoutEventProducer { get; }
        ITopicEventProducer TopicEventProducer { get; }

    }
    
    public interface IDirectEventProducer
    {
        void Publish(QueueOptions queueOptions, EventBase eventBase,
            ExchangeOptions exchangeOptions = null);
    }
    public interface IFanoutEventProducer
    {
        void Publish(EventBase eventBase,QueueOptions queueOptions=null,
            ExchangeOptions exchangeOptions = null);
    }
    public interface ITopicEventProducer
    {
        void Publish(EventBase eventBase, string messageRoutingPattern,QueueOptions queueOptions=null,
            ExchangeOptions exchangeOptions = null);
    }
}