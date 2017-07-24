namespace RabbitHole
{
    public interface IConsumerBroker
    {
        void CloseChannel();
        void Go(IConnection connection, IExchange exchange, IQueue queue);
    }
}