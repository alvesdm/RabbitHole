using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole.Exceptions
{
    public class UnableToCreateChannelException : Exception
    {
        public UnableToCreateChannelException(string message) : this(message, null)
        {
        }

        public UnableToCreateChannelException(string message, Exception innerException): base(message, innerException)
        {
        }
    }
}
