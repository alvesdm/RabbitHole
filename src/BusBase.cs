namespace RabbitHole
{
    public abstract class BusBase : IBus
    {
        public IClient Client { get; }

        protected BusBase(IClient client)
        {
            Client = client;
        }
    }
}