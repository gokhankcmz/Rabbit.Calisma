using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitLib.Producer;
using RabbitMQ.Client;

namespace RabbitLib.Utility
{
    public static class ServiceExtension
    {
        public static void AddRabbitMq(this IServiceCollection services, RabbitMqSettings rabbitSettings)
        {
            services.AddSingleton<IRabbitMqPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMqPersistentConnection>>();
                var factory = new ConnectionFactory
                {
                    HostName = rabbitSettings.Hostname,
                    Password = rabbitSettings.Password,
                    UserName = rabbitSettings.Username,
                    Port = rabbitSettings.Port
                };
                return new DefaultRabbitMqPersistentConnection(factory, rabbitSettings.RetryCount, logger);
            });

            services.AddSingleton<IEventProducerManager, EventProducerManager>();
        }
    }
}