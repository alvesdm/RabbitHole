using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public interface IMessageConfiguration<T> : IMessageConfigurator
        where T : IMessage
    {
        Func<T, Guid> CorrelationField { get; }
        string QueueName { get; }
        string RoutingKey { get; }
        bool Persistent { get; }
        IBasicProperties Properties { get; }

        IMessageConfiguration<T> WithCorrelationId(Func<T, Guid> correlationField);
        IMessageConfiguration<T> WithQueue(string queueName);
        IMessageConfiguration<T> WithQueue(IQueue queue);
        IMessageConfiguration<T> WithRoutingKey(string routingKey);
        IMessageConfiguration<T> WithProperties(IBasicProperties properties);
        IMessageConfiguration<T> BeingPersistent(bool isPersistent);
    }

    public interface IMessageConfigurator {
    }
}
