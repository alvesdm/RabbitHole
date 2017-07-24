using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public interface IQueue
    {
        string Name { get; }
        bool Durable { get; }
        bool AutoDelete { get; }
        bool Exclusive { get; }
        
        IList<IBinding> Bindings { get; }
        IBasicQos Qos { get; }

        IQueue WithName(string name);
        IQueue BeingDurable(bool isDurable);
        IQueue BeingAutoDeleted(bool isAutoDelete);
        IQueue BeingExclusive(bool isExclusive);
        
        IQueue WithBinding(Func<IBinding, IBinding> binding);
        IQueue WithBasicQos(Func<IBasicQos, IBasicQos> basicQos);
        
    }
}
