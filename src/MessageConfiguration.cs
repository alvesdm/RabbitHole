using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace RabbitHole
{
    public class MessageConfiguration<T> : IMessageConfiguration<T>
        where T : IMessage
    {
        public Func<T, Guid> CorrelationField { get; private set; }
        public string QueueName { get; private set; }
        public string RoutingKey { get; private set; }
        public bool Persistent { get; private set; }
        public IBasicProperties Properties { get; private set; }

        public MessageConfiguration()
        {
            this.Persistent = true;
        }

        public IMessageConfiguration<T> BeingPersistent(bool isPersistent)
        {
            this.Persistent = isPersistent;
            return this;
        }

        public IMessageConfiguration<T> WithCorrelationId(Func<T, Guid> correlationField)
        {
            this.CorrelationField = correlationField;
            return this;
        }

        public IMessageConfiguration<T> WithProperties(IBasicProperties properties)
        {
            this.Properties = properties;
            return this;
        }

        public IMessageConfiguration<T> WithQueue(string queueName)
        {
            this.QueueName = queueName;
            return this;
        }

        public IMessageConfiguration<T> WithQueue(IQueue queue)
        {
            this.QueueName = queue.Name;
            return this;
        }

        public IMessageConfiguration<T> WithRoutingKey(string routingKey)
        {
            this.RoutingKey = routingKey;
            return this;
        }
    }
}
