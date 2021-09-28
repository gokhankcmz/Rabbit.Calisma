namespace RabbitLib.Events
{
    public class OrderCreateEvent : EventBase
    {
        public int orderId { get; set; }
    }
}