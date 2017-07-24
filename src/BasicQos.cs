using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public class BasicQos : IBasicQos
    {
        public bool Global { get; private set; }
        public ushort PrefetchCount { get; private set; }
        public uint PrefetchSize { get; private set; }

        public IBasicQos BeingGlobal(bool isGlobal)
        {
            this.Global = isGlobal;
            return this;
        }

        public IBasicQos WithPrefetchCount(ushort prefetchCount)
        {
            this.PrefetchCount = prefetchCount;
            return this;
        }

        public IBasicQos WithPrefetchSize(uint prefetchSize)
        {
            this.PrefetchSize = prefetchSize;
            return this;
        }
    }
}
