# RabbitHole
A RabbitMQ wrapper for .net core


Nuget
```
Install-Package alvesdm.RabbitHole
```

How to use it?

Check our Samples

...but find below how a simple fanout would go:

Publisher
```csharp
            using (var client = ClientFactory.Create())
            {
                client
                    .DeclareExchange(c => c.WithName("PublisherService"));

				client
					.Publish<CustomerUpdated>(p => p
													.WithExchange("PublisherService")
													.WithCorrelationId(r=>r.Id)
													.WithMessage(new CustomerUpdated
													{
														Id = Guid.NewGuid(),
														Name = "RabbitHole is my name"
													}));
            }
```

Consumer
```csharp
            using (var client = ClientFactory.Create())
            {
                client
                    .DeclareExchange(c => c.WithName("PublisherService"))
                    .DeclareQueue(q => q.WithName("ConsumerService.CustomerUpdated"))
                    .Consume<CustomerUpdated>(c => c
                                                    .WithExchange("PublisherService")
                                                    .WithQueue("ConsumerService.CustomerUpdated")
                                                    .WhenReceive((ch, ea, message, cId) =>
                                                    {
                                                        Console.WriteLine($"Received by '{name}' --> Message: {message.Name}, CorrelationId: {cId}");
                                                        return Task.FromResult(true);
                                                    }));
            }
}
```

# License

Code released under the MIT license.
