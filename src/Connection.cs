using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using Polly;
using RabbitHole.Exceptions;

namespace RabbitHole
{
    public class Connection : IConnection
    {
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string HostName { get; private set; }
        public string VirtualHost { get; private set; }
        public bool AutomaticRecovery { get; private set; }
        public bool TopologyRecovery { get; private set; }
        public int RecoveryRetryInterval { get; private set; }
        public RabbitMQ.Client.IConnection RabbitConnection { get; private set; }

        public Connection()
        {
            UserName = "guest";
            Password = "guest";
            HostName = "localhost";
            VirtualHost = "/";
        }

        public RabbitMQ.Client.IConnection CreateRabbitConnection()
        {
            return CreateRabbitConnection(null);
        }

        public RabbitMQ.Client.IConnection CreateRabbitConnection(string name)
        {
            var tryConnectAttempts = 10;

            var factory = new ConnectionFactory {
                HostName = this.HostName,
                UserName = this.UserName,
                Password = this.Password,
                VirtualHost = this.VirtualHost,
                AutomaticRecoveryEnabled = this.AutomaticRecovery,
                TopologyRecoveryEnabled = this.TopologyRecovery,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(this.RecoveryRetryInterval)
            };

            Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    tryConnectAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timespan) =>
                    {
                        Console.WriteLine($"Unable to connect to RabbitMQ. Trying again in {timespan.TotalSeconds} seconds.", exception);
                        //throw new UnableToInitiateConnectionException($"Unable to connect to RabbitMQ. Trying again in {timespan.TotalSeconds} seconds.", exception);
                    })
                .Execute(() =>
                {
                    this.RabbitConnection = name != null
                        ? factory.CreateConnection(name)
                        : factory.CreateConnection();
                });


            if(this.RabbitConnection == null)
                throw new UnableToInitiateConnectionException($"We were unable to connect to RabbitMQ. We tried {tryConnectAttempts} times.");

            //factory.AutomaticRecoveryEnabled = this.AutomaticRecovery;
            //factory.TopologyRecoveryEnabled = this.TopologyRecovery;
            //factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(this.RecoveryRetryInterval);

            return this.RabbitConnection;
        }

        public IConnection WithHostName(string hostName)
        {
            this.HostName = hostName;
            return this;
        }

        public IConnection WithPassword(string password)
        {
            this.Password = password;
            return this;
        }

        public IConnection WithUserName(string userName)
        {
            this.UserName = userName;
            return this;
        }

        public IConnection WithVirtualHost(string virtualHost)
        {
            this.VirtualHost = virtualHost;
            return this;
        }

        public IConnection WithAutomaticRecovery(bool automaticRecoveryEnabled)
        {
            this.AutomaticRecovery = automaticRecoveryEnabled;
            return this;
        }

        public IConnection WithTopologyRecovery(bool topologyRecoveryEnabled)
        {
            this.TopologyRecovery = topologyRecoveryEnabled;
            return this;
        }

        public IConnection WithRecoveryRetryInterval(int recoveryRetryInterval)
        {
            this.RecoveryRetryInterval = recoveryRetryInterval;
            return this;
        }

        public void Dispose()
        {
            this.RabbitConnection.Dispose();
        }

        public void Close()
        {
            this.RabbitConnection.Close();
        }
    }
}
