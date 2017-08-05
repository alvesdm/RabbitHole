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
    public class Publisher<T> : IPublisher<T>
        where T : IMessage
    {
        public IMessage Message { get; private set; }
        public string RoutingKey { get; private set; }
        public IBasicProperties Properties { get; private set; }
        public Func<T, Guid> CorrelationField { get; private set; }
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

        public void Go(IConnection connection, IExchange exchange, IDictionary<Type, IMessageConfigurator> messagesConfiguration)
        {
            try
            {
                var routingKey = this.RoutingKey;

                _policy.Execute(() =>
                {
                    using (var channel = connection.RabbitConnection.CreateModel())
                    {
                        Guid correlationId = this.CorrelationField != null ? this.CorrelationField((T)this.Message) : Guid.Empty;
                        var properties = this.Properties ?? channel.CreateBasicProperties();
                        var messageType = this.Message.GetType();

                        properties.CorrelationId = (correlationId != Guid.Empty ? correlationId : Guid.NewGuid()).ToString();
                        if (messagesConfiguration.ContainsKey(messageType))
                        {
                            var messageConfiguration = messagesConfiguration[messageType] as IMessageConfiguration<T>;
                            if (messageConfiguration.Properties != null)
                                properties = messageConfiguration.Properties;

                            routingKey = messageConfiguration.RoutingKey;
                            //queueName = messageConfiguration.QueueName;
                            correlationId = messageConfiguration.CorrelationField((T)this.Message);
                            if(correlationId != Guid.Empty)
                                properties.CorrelationId = correlationId.ToString();
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
                        Console.WriteLine($"RabbitHole: Sent Message. Exchange: {exchange.Name}, CorrelationId:{properties.CorrelationId}");
                    }
                });
            }
            catch (Exception ex)
            {
                ///TODO logging
                throw;
            }
        }

        public IPublisher<T> WithMessage(IMessage message)
        {
            this.Message = message;
            return this;
        }

        public IPublisher<T> WithProperties(IBasicProperties properties)
        {
            this.Properties = properties;
            return this;
        }

        public IPublisher<T> WithRoutingKey(string routingKey)
        {
            this.RoutingKey = routingKey;
            return this;
        }

        public IPublisher<T> WithCorrelationId(Func<T, Guid> correlationField)
        {
            this.CorrelationField = correlationField;
            return this;
        }
    }
}
