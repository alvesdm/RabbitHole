using System;
namespace RabbitHole
{
    public interface IClient : IDisposable
    {
        IClient WithConnection(Func<IConnection, IConnection> connection);
        IClient WithConnection(IConnection connection);
        IClient DeclareExchange(Func<IExchange, IExchange> exchange);
        IClient DeclareQueue(Func<IQueue, IQueue> queue);
        IClient WithRequeueTime(int requeueWaitingTime);
        void Publish<T>(Func<IPublisher<T>, IPublisher<T>> publisher) where T : IMessage;
        void Consume<T>(Func<IConsumer<T>, IConsumer<T>> consumer) where T : IMessage;
        IClient ConfiguringMessage<T>(Func<IMessageConfiguration<T>, IMessageConfiguration<T>> configuration) where T : IMessage;
        IConnection GetConnection();
        void Shutdown();
    }
}