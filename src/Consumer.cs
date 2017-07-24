using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace RabbitHole
{
    public class Consumer<T> : IConsumer<T>
        where T : IMessage
    {
        public string Queue { get; set; }
        public bool AutoKnowledge { get; private set; }
        private Func<EventingBasicConsumer, BasicDeliverEventArgs, T, bool> _action;
        private IModel _channel;

        public IConsumer<T> WhenReceive(Func<EventingBasicConsumer, BasicDeliverEventArgs, T, bool> action)
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

            try
            {
                _channel = connection.RabbitConnection.CreateModel();
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
                consumer.Received += (model, ea) =>
                {
                    var success = false;
                    var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(ea.Body));
                    try
                    {
                        success = _action(model as EventingBasicConsumer, ea, message);
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

        public void CloseChannel()
        {
            _channel.Close();
            _channel.Dispose();
        }
    }
}
