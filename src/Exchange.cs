using System;
using System.Collections.Generic;
using System.Text;
using RabbitHole.Enums;

namespace RabbitHole
{
    public class Exchange : IExchange
    {
        public string Name { get; private set; }
        public ExchangeType Type { get; private set; }
        public bool Durable { get; private set; }
        public bool AutoDelete { get; private set; }

        public Exchange()
        {
            this.AutoDelete = true;
            this.Type = ExchangeType.Fanout;
        }

        public IExchange BeingAutoDeleted(bool isAutoDelete)
        {
            this.AutoDelete = isAutoDelete;
            return this;
        }

        public IExchange BeingDurable(bool isDurable)
        {
            this.Durable = isDurable;
            return this;
        }

        public IExchange WithName(string name)
        {
            this.Name = name;
            return this;
        }

        public IExchange WithType(ExchangeType type)
        {
            this.Type = type;
            return this;
        }
    }
}
