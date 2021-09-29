using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitLib;
using RabbitLib.Constants;
using RabbitLib.Events;
using RabbitLib.Producer;
using RabbitLib.Utility;

namespace RabbitWebApiProducer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private IEventProducerManager _producer;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IEventProducerManager producer)
        {
            _logger = logger;
            _producer = producer;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();

            var queue = new QueueOptions
            {
                NewQueue = true,
                RoutingKey = "Queue",
                Arguments = null,
                Durable = false,
                Exclusive = false,
                AutoDelete = false,
                QueueName = "Queue"
            };
            
            var Queue2 = new QueueOptions
            {
                NewQueue = true,
                RoutingKey = "Queue.*",
                Arguments = null,
                Durable = false,
                Exclusive = false,
                AutoDelete = false,
                QueueName = "Queue2"
            };
            
            var Queue3 = new QueueOptions
            {
                NewQueue = true,
                RoutingKey = "Queue.3",
                Arguments = null,
                Durable = false,
                Exclusive = false,
                AutoDelete = false,
                QueueName = "Queue3"
            };
            
            var DirectExchange = new ExchangeOptions()
            {
                Arguments = null,
                Durable = false,
                AutoDelete = false,
                ExchangeName = "CustomDirectExchange"
            };
            
            var TopicExchange = new ExchangeOptions()
            {
                Arguments = null,
                Durable = false,
                AutoDelete = false,
                ExchangeName = "CustomTopicExchange"
            };
            
            var FanoutExchange = new ExchangeOptions()
            {
                Arguments = null,
                Durable = false,
                AutoDelete = false,
                ExchangeName = "CustomFanoutExchange"
            };
            
            _producer.DirectEventProducer.Publish(queue, new OrderCreateEvent(){orderId = 1}, DirectExchange);
            _producer.TopicEventProducer.Publish(new OrderCreateEvent{orderId = 3}, "Queue.*", Queue2, TopicExchange);
            _producer.TopicEventProducer.Publish(new OrderCreateEvent{orderId = 3}, "Queue.3", Queue3, TopicExchange);


            _producer.FanoutEventProducer.Publish(new OrderCreateEvent(){orderId = 2}, Queue2, FanoutExchange);
            _producer.FanoutEventProducer.Publish(new OrderCreateEvent(){orderId = 2}, Queue3,FanoutExchange);
            
            
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();

        }
    }
}