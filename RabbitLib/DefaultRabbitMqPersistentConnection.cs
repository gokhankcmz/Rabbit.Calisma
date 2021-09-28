using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace RabbitLib
{
    public class DefaultRabbitMqPersistentConnection : IRabbitMqPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;
        private IConnection _connection;
        private bool _disposed;
        private readonly ILogger<DefaultRabbitMqPersistentConnection> _logger;

        public DefaultRabbitMqPersistentConnection(IConnectionFactory connectionFactory, int retryCount,
            ILogger<DefaultRabbitMqPersistentConnection> logger)
        {
            _connectionFactory = connectionFactory;
            _retryCount = retryCount;
            _logger = logger;
        }


        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed; 

        public bool TryConnect()
        {
            _logger.LogInformation("RabbitMQ Client is trying to connect.");
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex,
                            "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})",
                            $"{time.TotalSeconds:n1}", ex);
                    });
            policy.Execute(() => { _connection = _connectionFactory.CreateConnection(); });

            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutDown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;
                _logger.LogInformation(
                    "RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events",
                    _connection);
                return true;
            }

            _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened.");
            return false;
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action.");
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                _connection.Dispose();
                _disposed = true;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
            }
        }

        public int GetRetryCount()
        {
            return _retryCount;
        }

        private void OnConnectionShutDown(object sender, ShutdownEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ Connection is shutdown. Trying to re-connect...");
            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ Connection throws exception. Trying to re-connect...");
            TryConnect();
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ Connection is blocked. Trying to re-connect...");
            TryConnect();
        }
    }
}