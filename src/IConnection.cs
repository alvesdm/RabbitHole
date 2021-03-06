﻿using System;

namespace RabbitHole
{
    public interface IConnection : IDisposable
    {
        string UserName { get; }
        string Password { get; }
        string HostName { get; }
        string VirtualHost { get; }
        RabbitMQ.Client.IConnection RabbitConnection { get; }

        IConnection WithUserName(string userName);
        IConnection WithPassword(string password);
        IConnection WithHostName(string hostName);
        IConnection WithVirtualHost(string virtualHost);
        IConnection WithAutomaticRecovery(bool automaticRecoveryEnabled);
        IConnection WithTopologyRecovery(bool topologyRecoveryEnabled);
        IConnection WithRecoveryRetryInterval(int recoveryRetryInterval);
        RabbitMQ.Client.IConnection CreateRabbitConnection();
        RabbitMQ.Client.IConnection CreateRabbitConnection(string name);
        void Close();
    }
}