namespace RabbitWebApiProducer
{
    
    public class Customer : Document
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public bool Valid { get; set; }
    }
}