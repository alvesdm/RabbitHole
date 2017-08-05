# RabbitHole
A RabbitMQ wrapper for .net core


Nuget
```
Install-Package alvesdm.RabbitHole
```

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

...but of course you can go in a simpler way.
i.e a simple fanout would go like this:

Publisher
```csharp
            using (var client = ClientFactory.Create())
            {
                client
                    .WithExchange(c => c.WithName("PublisherService"))
                    .ConfiguringMessage<CustomerUpdated>(c => c.WithCorrelationId(i => i.Id));

                while (true)
                {
                    client
                        .Publish<CustomerUpdated>(p => p
                                                        .WithMessage(new CustomerUpdated
                                                        {
                                                            Id = Guid.NewGuid(),
                                                            Name = "RabbitHole is my name"
                                                        }));
                    System.Threading.Thread.Sleep(1500);
                }
            }
```

Consumer
```csharp
            using (var client = ClientFactory.Create())
            {
                client
                    .WithExchange(c => c.WithName("PublisherService"))
                    .WithQueue(q => q.WithName("ConsumerService.CustomerUpdated"))
                    .Consume<CustomerUpdated>(c => c
                                                    .WhenReceive((ch, ea, message, cId) =>
                                                    {
                                                        Console.WriteLine($"Received --> Message: {message.Name}, CorrelationId: {cId}");
                                                        return Task.FromResult(true);
                                                    }));

                HoldOn();
            }
}
```

# License

Code released under the MIT license.
