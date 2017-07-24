using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace RabbitHole
{
    public class Publisher : IPublisher
    {
        public IMessage Message { get; private set; }
        public string RoutingKey { get; private set; }
        public IBasicProperties Properties { get; private set; }

        public void Go<T>(IConnection connection, IExchange exchange, IDictionary<Type, IMessageConfigurator> messagesConfiguration)
            where T : IMessage
        {
            try
            {
                //string queueName;
                Guid correlationId;
                var properties = this.Properties;
                var routingKey = this.RoutingKey;

                using (var channel = connection.RabbitConnection.CreateModel())
                {
                    var messageType = this.Message.GetType();
                    if (messagesConfiguration.ContainsKey(messageType))
                    {
                        var messageConfiguration = messagesConfiguration[messageType] as IMessageConfiguration<T>;
                        properties = messageConfiguration.Properties != null ?
                            messageConfiguration.Properties :
                            channel.CreateBasicProperties();

                        routingKey = messageConfiguration.RoutingKey;
                        //queueName = messageConfiguration.QueueName;
                        correlationId = messageConfiguration.CorrelationField((T)this.Message);
                        properties.CorrelationId = (correlationId != Guid.Empty ? correlationId : Guid.NewGuid()).ToString();
                        properties.Persistent = messageConfiguration.Persistent;
                    }

                    channel.ExchangeDeclare(exchange: exchange.Name,
                                            type: exchange.Type.ToString().ToLower(),
                                            durable: exchange.Durable,
                                            autoDelete: exchange.AutoDelete);

                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.Message));
                    channel.BasicPublish(exchange: exchange.Name,
                                         routingKey: routingKey,
                                         basicProperties: properties,
                                         body: body);
                }
            }
            catch (Exception ex)
            {
                ///TODO logging
                throw;
            }
        }

        public IPublisher WithMessage(IMessage message)
        {
            this.Message = message;
            return this;
        }

        public IPublisher WithProperties(IBasicProperties properties)
        {
            this.Properties = properties;
            return this;
        }

        public IPublisher WithRoutingKey(string routingKey)
        {
            this.RoutingKey = routingKey;
            return this;
        }
    }
}
