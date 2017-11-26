using Newtonsoft.Json;
using RabbitHole.Factories;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitHole.Samples.PubSub
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => {
                StartConsumer("Consumer A");
            });

            Task.Run(() => {
                StartConsumer("Consumer B");
            });

            Task.Run(()=> {
                StartPublisher();
            });

            Console.ReadKey();
        }

        private static void StartPublisher()
        {
            var connection = new Connection().WithUserName("guest2").WithPassword("guest2");
            //ClientFactory.WithDefaultConnection(c=>c.WithUserName("guest2").WithPassword("guest2"));
            using (var client = ClientFactory.Create())
            {
                client
                    .WithExchange(c => c.WithName("PublisherService"));

                while (true)
                {
                    var id = Guid.NewGuid();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Sending message: Id:{id}");
                    Console.ResetColor();
                    client
                        .Publish<CustomerUpdated>(p => p
                                                        .WithCorrelationId(r=>r.Id)
                                                        .WithMessage(new CustomerUpdated
                        {
                            Id = id,
                            Name = "RabbitHole is my name"
                        }));
                    System.Threading.Thread.Sleep(10);
                    //break;
                }
            }

            HoldOn();
        }

        private static void StartConsumer(string name)
        {
            var consumedCounter = 0;
            using (var client = ClientFactory.Create())
            {
                client
                    .WithExchange(c => c.WithName("PublisherService"))
                    .WithQueue(q => q.WithName("ConsumerService.CustomerUpdated"))
                    .Consume<CustomerUpdated>(c => c
                                                    .WithRequeueTime(100)
                                                    .WithDeserializer(ea =>
                                                    {
                                                        return JsonConvert.DeserializeObject<CustomerUpdated>(Encoding.UTF8.GetString(ea.Body));
                                                    })
                                                    .WhenReceive((ch, ea, message, cId) =>
                                                    {
                                                        if(DateTime.Now.Second % 2 == 0)
                                                            throw new Exception("sdsdfsdf");
                                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                                        Console.WriteLine($"Received by '{name}' --> Message: {message.Name}, CorrelationId: {cId}");
                                                        Console.ResetColor();
                                                        Interlocked.Increment(ref consumedCounter);
                                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                                        Console.WriteLine($"{consumedCounter} received by '{name}'.");
                                                        return Task.FromResult(true);
                                                    }));

                HoldOn();
            }
        }

        private static void HoldOn() {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }


        public class CustomerUpdated : IMessage
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }
    }

}