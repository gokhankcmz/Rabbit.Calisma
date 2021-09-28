using System;

namespace RabbitLib.Events
{
    public abstract class EventBase
    {       
        public Guid eventId { get; }
        public DateTime CreationDate { get; }
        
        protected EventBase()
        {
            eventId = Guid.NewGuid();
            CreationDate = DateTime.Now;
        }


    }
}