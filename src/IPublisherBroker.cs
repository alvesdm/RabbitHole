using System;
using System.Collections.Generic;

namespace RabbitHole
{
    public interface IPublisherBroker
    {
        void Go(IConnection connection, IEnumerable<IExchange> exchange, IDictionary<Type, IMessageConfigurator> messagesConfiguration);
    }
}