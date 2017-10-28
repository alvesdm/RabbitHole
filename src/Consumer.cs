using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Polly;
using RabbitHole.Exceptions;
using System.Threading.Tasks;

namespace RabbitHole
{
    public class Consumer<T> : IConsumer<T>
        where T : IMessage
    {
        public string Queue { get; set; }
        public bool AutoKnowledge { get; private set; }
        private Func<EventingBasicConsumer, BasicDeliverEventArgs, T, string, Task<bool>> _action;
        private Func<BasicDeliverEventArgs, T> _deserializer;
        private IModel _channel;
        private int _tryConnectAttempts = 15;

        public Consumer()
        {
            this._deserializer = (ea) => {
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(ea.Body));
            };
        }

        public IConsumer<T> WhenReceive(Func<EventingBasicConsumer, BasicDeliverEventArgs, T, string, Task<bool>> action)
        {
            _action = action;
            return this;
        }

        public IConsumer<T> WithQueue(string queueName)
        {
            this.Queue = queueName;
            return this;
        }

        public IConsumer<T> BeingAutoKnowledge(bool isAutoKnowledged)
        {
            this.AutoKnowledge = isAutoKnowledged;
            return this;
        }

        public void Go(IConnection connection, IExchange exchange, IQueue queue)
        {
            void KnoledgeIt(ulong deliveryTag)
            {
                _channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
            }

            void RequeueIt(ulong deliveryTag)
            {
                Task.Run(()=> {
                    System.Threading.Thread.Sleep(5000);
                    _channel.BasicNack(deliveryTag: deliveryTag, multiple: false, requeue: true);
                });
            }

            try
            {
                TryToStablishAChannel(connection);

                _channel.ExchangeDeclare(exchange: exchange.Name,
                                        type: exchange.Type.ToString().ToLower(),
                                        durable: exchange.Durable,
                                        autoDelete: exchange.AutoDelete);
                var queueName = _channel.QueueDeclare(queue.Name, queue.Durable, queue.Exclusive, queue.AutoDelete).QueueName;
                _channel.BasicQos(prefetchSize: queue.Qos.PrefetchSize, prefetchCount: queue.Qos.PrefetchCount, global: queue.Qos.Global);

                foreach (var binding in queue.Bindings)
                {
                    _channel.QueueBind(queue: queueName,
                                      exchange: exchange.Name,
                                      routingKey: binding.RoutingKey);
                }

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    Console.WriteLine($"RabbitHole: Received Message. Exchange: {exchange.Name}, Queue: {queueName}, CorrelationId:{ea.BasicProperties.CorrelationId}");
                    var success = false;
                    var message = _deserializer(ea);
                    try
                    {
                        success = await _action(model as EventingBasicConsumer, ea, message, ea.BasicProperties.CorrelationId);
                    }
                    catch (Exception ex) {
                        //https://www.rabbitmq.com/nack.html
                        //http://www.rabbitmq.com/dlx.html
                        //RequeueIt(ea.DeliveryTag);
                    }
                    finally {
                        if (this.AutoKnowledge || success)
                            KnoledgeIt(ea.DeliveryTag);
                    }
                };

                _channel.BasicConsume(queue: queueName,
                                     noAck: false,
                                     consumer: consumer);
            }
            catch (Exception ex)
            {
                ///TODO logging
                throw;
            }
        }

        private void TryToStablishAChannel(IConnection connection)
        {
            Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    _tryConnectAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                    (exception, timespan) =>
                    {
                        Console.WriteLine($"Unable to stablish a channel. Trying again in {timespan.TotalSeconds} seconds.", exception);
                        //throw new UnableToCreateChannelException($"Unable to stablish a channel. Trying again in {timespan.TotalSeconds} seconds.", exception);
                    })
                .Execute(() =>
                {
                    _channel = connection.RabbitConnection.CreateModel();
                });

            if (_channel == null)
                throw new UnableToCreateChannelException($"We were unable to stablish a channel. We tried {_tryConnectAttempts} times.");
        }

        public void CloseChannel()
        {
            _channel.Close();
            _channel.Dispose();
        }

        public IConsumer<T> WithDeserializer(Func<BasicDeliverEventArgs, T> action)
        {
            this._deserializer = action;
            return this;
        }
    }
}
