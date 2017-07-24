﻿using System;
namespace RabbitHole
{
    public interface IClient : IDisposable
    {
        IClient WithConnection(Func<IConnection, IConnection> connection);
        IClient WithExchange(Func<IExchange, IExchange> exchange);
        IClient WithQueue(Func<IQueue, IQueue> queue);
        void Publish<T>(Func<IPublisher, IPublisher> publisher) where T : IMessage;
        void Consume<T>(Func<IConsumer<T>, IConsumer<T>> consumer) where T : IMessage;
        IClient ConfiguringMessage<T>(Func<IMessageConfiguration<T>, IMessageConfiguration<T>> configuration) where T : IMessage;
        void Shutdown();
    }
}