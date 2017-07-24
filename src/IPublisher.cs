using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public interface IPublisher
    {
        IMessage Message { get; }
        string RoutingKey { get; }
        IBasicProperties Properties { get; }

        IPublisher WithMessage(IMessage message);
        IPublisher WithRoutingKey(string routingKey);
        IPublisher WithProperties(IBasicProperties properties);
        void Go<T>(IConnection connection, IExchange exchange, IDictionary<Type, IMessageConfigurator> messagesConfiguration) where T : IMessage;
    }
}
