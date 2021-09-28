using System;
using RabbitMQ.Client;

namespace RabbitLib
{
    public interface IRabbitMqPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();
        IModel CreateModel();

        int GetRetryCount();
    }
}