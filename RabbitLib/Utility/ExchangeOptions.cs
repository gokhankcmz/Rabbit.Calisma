using System.Collections.Generic;

namespace RabbitLib.Utility
{
    public class ExchangeOptions
    {
        
        public string ExchangeName { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }
        public Dictionary<string,object> Arguments { get; set; }
    }
}