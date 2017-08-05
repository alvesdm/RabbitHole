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
        private IExchange _exchange = new Exchange();
        private IQueue _queue = new Queue();
        private IConsumerBroker _consumer;
        private IPublisherBroker _publisher;
        private IDictionary<Type, IMessageConfigurator> _messagesConfiguration = new Dictionary<Type, IMessageConfigurator>();
        //--------

        public IClient ConfiguringMessage<T>(Func<IMessageConfiguration<T>, IMessageConfiguration<T>> configuration) 
            where T : IMessage
        {
            _messagesConfiguration.Add(typeof(T), configuration(new MessageConfiguration<T>()));
            return this;
        }

        public void Consume<T>(Func<IConsumer<T>, IConsumer<T>> consumer)
            where T : IMessage
        {
            //Perform autobindind if none
            if (!_queue.Bindings.Any() && _exchange.Type.Equals(ExchangeType.Fanout))
            {
                _queue.WithBinding(b => b.WithExchange(_exchange).WithQueue(_queue));
            }
            _consumer = consumer(new Consumer<T>());
            _consumer.Go(_connection, _exchange, _queue);
        }

        public void Dispose()
        {
            this.Shutdown();
        }

        public void Publish<T>(Func<IPublisher<T>, IPublisher<T>> publisher)
            where T : IMessage
        {
            _publisher = publisher(new Publisher<T>());
            _publisher.Go(_connection, _exchange, _messagesConfiguration);
        }

        public void Shutdown()
        {
            _consumer?.CloseChannel();
            _connection.Close();
            _connection.Dispose();
        }

        public IClient WithConnection(Func<IConnection, IConnection> connection)
        {
            connection(_connection);
            return this;
        }

        public IClient WithConnection(IConnection connection)
        {
            _connection = connection;
            return this;
        }

        public IClient WithExchange(Func<IExchange, IExchange> exchange)
        {
            exchange(_exchange);
            return this;
        }

        public IClient WithQueue(Func<IQueue, IQueue> queue)
        {
            queue(_queue);
            return this;
        }
    }
}
