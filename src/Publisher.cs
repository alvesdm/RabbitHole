using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitHole.Exceptions;

namespace RabbitHole
{
    public class Publisher : IPublisher
    {
        public IMessage Message { get; private set; }
        public string RoutingKey { get; private set; }
        public IBasicProperties Properties { get; private set; }
        private int _tryConnectAttempts = 15;

        private RetryPolicy _policy;

        public Publisher()
        {
            this.RoutingKey = string.Empty;

            _policy = Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    _tryConnectAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                    (exception, timespan) =>
                    {
                        Console.WriteLine($"Unable to stablish a channel and/or publish the message. Trying again in {timespan.TotalSeconds} seconds.", exception);
                        //throw new UnableToCreateChannelException($"Unable to stablish a channel and/or publish the message. Trying again in {timespan.TotalSeconds} seconds.", exception);
                    });
        }

        public void Go<T>(IConnection connection, IExchange exchange, IDictionary<Type, IMessageConfigurator> messagesConfiguration)
            where T : IMessage
        {
            try
            {
                var routingKey = this.RoutingKey;

                _policy.Execute(() =>
                {
                    using (var channel = connection.RabbitConnection.CreateModel())
                    {
                        var properties = this.Properties ?? channel.CreateBasicProperties();
                        var messageType = this.Message.GetType();
                        if (messagesConfiguration.ContainsKey(messageType))
                        {
                            var messageConfiguration = messagesConfiguration[messageType] as IMessageConfiguration<T>;
                            if (messageConfiguration.Properties != null)
                                properties = messageConfiguration.Properties;

                            routingKey = messageConfiguration.RoutingKey;
                            //queueName = messageConfiguration.QueueName;
                            var correlationId = messageConfiguration.CorrelationField((T)this.Message);
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
                });
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
