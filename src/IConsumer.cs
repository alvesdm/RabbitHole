using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace RabbitHole
{
    public interface IConsumer<T> : IConsumerBroker
        where T : IMessage
    {
        string Queue { get; }
        bool AutoKnowledge { get; }
        IConsumer<T> WithQueue(string queueName);
        IConsumer<T> WhenReceive(Func<EventingBasicConsumer, BasicDeliverEventArgs, T, bool> action);
        IConsumer<T> BeingAutoKnowledge(bool isAutoKnowledge);

    }
}
