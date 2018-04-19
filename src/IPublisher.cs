using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public interface IPublisher<T> : IPublisherBroker
        where T : IMessage
    {
        IMessage Message { get; }
        string RoutingKey { get; }
        IBasicProperties Properties { get; }

        IPublisher<T> WithMessage(IMessage message);
        IPublisher<T> WithRoutingKey(string routingKey);
        IPublisher<T> WithProperties(IBasicProperties properties);
        IPublisher<T> WithCorrelationId(Func<T, Guid> correlationField);
        IPublisher<T> WithExchange(string exchangeName);
    }
}
