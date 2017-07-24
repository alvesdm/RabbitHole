using System;
using System.Collections.Generic;
using System.Text;
using RabbitHole.Enums;

namespace RabbitHole
{
    public interface IExchange
    {
        string Name { get; }
        ExchangeType Type { get; }
        bool Durable { get; }
        bool AutoDelete { get; }

        IExchange WithName(string name);
        IExchange WithType(ExchangeType type);
        IExchange BeingDurable(bool isDurable);
        IExchange BeingAutoDeleted(bool isAutoDelete);
    }
}
