using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public interface IBinding
    {
        string QueueName { get; }
        string ExchangeName { get; }
        string RoutingKey { get; }

        IBinding WithQueue(string queueName);
        IBinding WithQueue(IQueue queue);
        IBinding WithExchange(string exchangeName);
        IBinding WithExchange(IExchange exchange);
        IBinding WithRoutingKey(string routingKey);
    }
}
