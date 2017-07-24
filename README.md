# RabbitHole
A C# Promise package for .net core.

How to use it?

```csharp
using (var client = RabbitHole.Factories.ClientFactory.Create())
{
    IBasicProperties properties = null;
    var message = new TestMessage();

    client
        .WithConnection(c => c.WithUserName("guest").WithPassword("guest").WithHostName("localhost:5672").WithVirtualHost("/"))
        .WithExchange(c => c.WithName("ServiceX").WithType(RabbitHole.Enums.ExchangeType.Direct))
        .WithQueue(q => q
                        .WithName("").BeingDurable(true).BeingAutoDeleted(false).BeingExclusive(false)
                        .WithBinding(b => b.WithQueue("").WithExchange("").WithRoutingKey(""))
                        .WithBasicQos(b => b.WithPrefetchSize(0).WithPrefetchCount(0).BeingGlobal(true)))
        .ConfiguringMessage<TestMessage>(c => c.WithCorrelationId(i => i.Id).WithQueue("").WithRoutingKey("").WithProperties(properties));

    client
        .Publish(p => p.WithMessage(message).WithRoutingKey("").WithProperties(properties));
    client
        .Consume<TestMessageReceived>(c => c.WithQueue("").WhenReceive((ch, ea, m) => { }));

    client.Shutdown();
}

public class TestMessage : RabbitHole.IMessage
{
	public Guid Id { get; set; }
}

public class TestMessageReceived : RabbitHole.IMessage
{
	public Guid Id { get; set; }
}
```

# License

Code released under the MIT license.
