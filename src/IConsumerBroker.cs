using System.Collections.Generic;

namespace RabbitHole
{
    public interface IConsumerBroker
    {
        void CloseChannel();
        void Go(IConnection connection, IEnumerable<IExchange> exchanges, IEnumerable<IQueue> queues);
    }
}