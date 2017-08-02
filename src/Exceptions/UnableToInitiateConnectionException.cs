using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole.Exceptions
{
    public class UnableToInitiateConnectionException : Exception
    {
        public UnableToInitiateConnectionException(string message) : this(message, null)
        {
        }

        public UnableToInitiateConnectionException(string message, Exception innerException): base(message, innerException)
        {
        }
    }
}
