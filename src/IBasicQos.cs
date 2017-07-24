using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public interface IBasicQos
    {
        bool Global { get; }
        ushort PrefetchCount { get; }
        uint PrefetchSize { get; }

        IBasicQos WithPrefetchSize(uint prefetchSize);
        IBasicQos WithPrefetchCount(ushort prefetchCount);
        IBasicQos BeingGlobal(bool isGlobal);
    }
}
