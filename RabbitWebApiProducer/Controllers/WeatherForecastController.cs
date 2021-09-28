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

            var queueOptions = new QueueOptions
            {
                NewQueue = true,
                Arguments = null,
                Durable = false,
                Exclusive = false,
                AutoDelete = false,
                QueueName = "BindedQueue"
            };
            
            var exchangeOptions = new ExchangeOptions()
            {
                Arguments = null,
                Durable = false,
                AutoDelete = false,
                ExchangeName = "TestExchange"
            };
            
            
            _producer.DirectEventProducer.Publish(queueOptions, new OrderCreateEvent(){orderId = 1}, exchangeOptions);

            queueOptions.QueueName = "NonBindedQueue";
            queueOptions.NewQueue = true;
            //_producer.FanoutEventProducer.Publish(new OrderCreateEvent(){orderId = 2}, queueOptions, exchangeOptions);
            // This will result in error: RabbitMQ.Client.Exceptions.OperationInterruptedException: The AMQP operation was interrupted: AMQP close-reason, initiated by Peer, code=406, text='PRECONDITION_FAILED - inequivalent arg 'type' for exchange 'TestExchange' in vhost '/': received 'fanout' but current is 'direct'', classId=40, methodId=10
           
            // we need to change the exchange option.
            exchangeOptions.ExchangeName = "NewExchange";
            _producer.FanoutEventProducer.Publish(new OrderCreateEvent(){orderId = 2}, queueOptions, exchangeOptions);
            
            // or not supply a exchangeoption to use defult.
            _producer.TopicEventProducer.Publish(new OrderCreateEvent{orderId = 3}, "Test*", queueOptions);
            
            
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