using System;
using System.Collections.Generic;
using System.Linq;
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
        private string _queueName;
        private string _exchangeName;
        public bool AutoKnowledge { get; private set; }
        private Func<EventingBasicConsumer, BasicDeliverEventArgs, T, string, Task<bool>> _action;
        private Func<BasicDeliverEventArgs, T> _deserializer;
        private IModel _channel;
        private int _tryConnectAttempts = 15;
        private int _requeueWaitingTime = 500;
        

        public Consumer()
        {
            _deserializer = (ea) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(ea.Body));
        }

        public IConsumer<T> WhenReceive(Func<EventingBasicConsumer, BasicDeliverEventArgs, T, string, Task<bool>> action)
        {
            _action = action;
            return this;
        }

        public IConsumer<T> WithQueue(string queueName)
        {
            _queueName = queueName;
            return this;
        }

        public IConsumer<T> WithExchange(string exchangeName)
        {
            _exchangeName = exchangeName;
            return this;
        }

        public IConsumer<T> BeingAutoKnowledge(bool isAutoKnowledged)
        {
            this.AutoKnowledge = isAutoKnowledged;
            return this;
        }

        public void Go(IConnection connection, IEnumerable<IExchange> exchanges, IEnumerable<IQueue> queues)
        {
            var exchange = exchanges.FirstOrDefault(e => e.Name.Equals(_exchangeName));
            if(exchange == null) throw new Exception($"No exchange with name '{_exchangeName}' was found.");
            var queue = queues.FirstOrDefault(q => q.Name.Equals(_queueName));
            if (queue == null) throw new Exception($"No queue with name '{_queueName}' was found.");

            void KnoledgeIt(ulong deliveryTag)
            {
                _channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
            }

            void RequeueIt(ulong deliveryTag)
            {
                Task.Run(()=> {
                    System.Threading.Thread.Sleep(_requeueWaitingTime);
                    _channel.BasicNack(deliveryTag: deliveryTag, multiple: false, requeue: true);
                });
            }

            TryToStablishAChannel(connection);

            _channel.ExchangeDeclare(exchange: exchange.Name,
                type: exchange.Type.ToString().ToLower(),
                durable: exchange.Durable,
                autoDelete: exchange.AutoDelete);
            var queueName = _channel.QueueDeclare(queue.Name, queue.Durable, queue.Exclusive, queue.AutoDelete).QueueName;
            _channel.BasicQos(prefetchSize: queue.Qos.PrefetchSize, prefetchCount: queue.Qos.PrefetchCount, global: queue.Qos.Global);

            //Perform autobindind if none
            if (!queue.Bindings.Any() && exchange.Type.Equals(Enums.ExchangeType.Fanout))
            {
                queue.WithBinding(b => b.WithExchange(exchange).WithQueue(queue));
            }

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
                    RequeueIt(ea.DeliveryTag);
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

        //https://derickbailey.com/2014/03/26/2-lessons-learned-and-3-resources-for-for-learning-rabbitmq-on-nodejs/
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
            _deserializer = action;
            return this;
        }

        public IConsumer<T> WithRequeueTime(int requeueWaitingTime)
        {
            _requeueWaitingTime = requeueWaitingTime;
            return this;
        }
    }
}
