using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public class Binding : IBinding
    {
        public string QueueName { get; set; }
        public string ExchangeName { get; private set; }
        public string RoutingKey { get; private set; }

        public IBinding WithExchange(string exchangeName)
        {
            this.ExchangeName = exchangeName;
            return this;
        }

        public IBinding WithExchange(IExchange exchange)
        {
            this.ExchangeName = exchange.Name;
            return this;
        }

        public IBinding WithQueue(string queueName)
        {
            this.QueueName = queueName;
            return this;
        }

        public IBinding WithQueue(IQueue queue)
        {
            this.QueueName = queue.Name;
            return this;
        }

        public IBinding WithRoutingKey(string routingKey)
        {
            this.RoutingKey = routingKey;
            return this;
        }
    }
}
