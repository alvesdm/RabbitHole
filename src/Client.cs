using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitHole.Enums;

namespace RabbitHole
{
    public class Client : IClient
    {
        private IConnection _connection = new Connection();
        private readonly List<IExchange> _exchanges = new List<IExchange>();
        private List<IQueue> _queues = new List<IQueue>();
        private IConsumerBroker _consumer;
        private IPublisherBroker _publisher;
        private readonly IDictionary<Type, IMessageConfigurator> _messagesConfiguration = new Dictionary<Type, IMessageConfigurator>();
        private int _requeueWaitingTime = 500;
        //--------

        public IClient ConfiguringMessage<T>(Func<IMessageConfiguration<T>, IMessageConfiguration<T>> configuration) 
            where T : IMessage
        {
            _messagesConfiguration.Add(typeof(T), configuration(new MessageConfiguration<T>()));
            return this;
        }

        public IConnection GetConnection()
        {
            return _connection;
        }

        public void Consume<T>(Func<IConsumer<T>, IConsumer<T>> consumer)
            where T : IMessage
        {
            _consumer = consumer(new Consumer<T>().WithRequeueTime(_requeueWaitingTime));
            _consumer.Go(_connection, _exchanges, _queues);
        }

        public void Dispose()
        {
            this.Shutdown();
        }

        public IClient WithRequeueTime(int requeueWaitingTime)
        {
            _requeueWaitingTime = requeueWaitingTime;
            return this;
        }

        public void Publish<T>(Func<IPublisher<T>, IPublisher<T>> publisher)
            where T : IMessage
        {
            _publisher = publisher(new Publisher<T>());
            _publisher.Go(_connection, _exchanges, _messagesConfiguration);
        }

        public void Shutdown()
        {
            _consumer?.CloseChannel();
            _connection.Close();
            _connection.Dispose();
        }

        public IClient WithConnection(Func<IConnection, IConnection> connection)
        {
            _connection = connection(_connection);
            return this;
        }

        public IClient WithConnection(IConnection connection)
        {
            _connection = connection;
            return this;
        }

        public IClient DeclareExchange(Func<IExchange, IExchange> exchange)
        {
            _exchanges.Add(exchange(new Exchange()));
            return this;
        }

        public IClient DeclareQueue(Func<IQueue, IQueue> queue)
        {
            _queues.Add(queue(new Queue()));
            return this;
        }
    }
}
