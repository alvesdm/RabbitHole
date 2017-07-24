using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace RabbitHole
{
    public class Connection : IConnection
    {
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string HostName { get; private set; }
        public string VirtualHost { get; private set; }
        public RabbitMQ.Client.IConnection RabbitConnection { get; private set; }

        public RabbitMQ.Client.IConnection CreateRabbitConnection()
        {
            return CreateRabbitConnection(null);
        }

        public RabbitMQ.Client.IConnection CreateRabbitConnection(string name)
        {
            var factory = new ConnectionFactory { HostName = this.HostName, UserName = this.UserName, Password = this.Password, VirtualHost = this.VirtualHost };
            this.RabbitConnection = name != null 
                ? factory.CreateConnection(name) 
                : factory.CreateConnection();
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
