﻿namespace RabbitLib.Utility
{
    public class RabbitMqSettings
    {
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public int RetryCount { get; set; }
    }
}