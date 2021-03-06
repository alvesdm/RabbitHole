﻿using System.Threading.Tasks;
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
        bool AutoKnowledge { get; }
        IConsumer<T> WithQueue(string queueName);
        IConsumer<T> WithExchange(string exchangeName);
        IConsumer<T> WithDeserializer(Func<BasicDeliverEventArgs, T> action);
        IConsumer<T> WhenReceive(Func<EventingBasicConsumer, BasicDeliverEventArgs, T, string, Task<bool>> action);
        IConsumer<T> WithRequeueTime(int requeueWaitingTime);
        IConsumer<T> BeingAutoKnowledge(bool isAutoKnowledge);
    }
}
