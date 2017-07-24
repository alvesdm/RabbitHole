using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole
{
    public class Queue : IQueue
    {
        public string Name { get; private set; }
        public bool Durable { get; private set; }
        public bool AutoDelete { get; private set; }
        public bool Exclusive { get; private set; }
        
        public IList<IBinding> Bindings { get; private set; }
        public IBasicQos Qos { get; private set; }

        

        public Queue()
        {
            this.Bindings = new List<IBinding>();
            this.Durable = true;
            this.Qos = new BasicQos()
                        .WithPrefetchCount(1)
                        .WithPrefetchSize(0);
        }

        public IQueue BeingAutoDeleted(bool isAutoDelete)
        {
            this.AutoDelete = isAutoDelete;
            return this;
        }

        public IQueue BeingDurable(bool isDurable)
        {
            this.Durable = isDurable;
            return this;
        }

        public IQueue BeingExclusive(bool isExclusive)
        {
            this.Exclusive = isExclusive;
            return this;
        }

        public IQueue WithBasicQos(Func<IBasicQos, IBasicQos> basicQos)
        {
            basicQos(this.Qos);
            return this;
        }

        public IQueue WithBinding(Func<IBinding, IBinding> binding)
        {
            var b = binding(new Binding());
            b.WithQueue(this);
            this.Bindings.Add(b);

            return this;
        }

        public IQueue WithName(string name)
        {
            this.Name = name;
            return this;
        }
    }
}
