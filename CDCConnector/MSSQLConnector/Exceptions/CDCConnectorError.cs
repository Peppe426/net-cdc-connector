using System.Runtime.Serialization;

namespace MSSQLConnector.Exceptions
{
    [Serializable]
    internal class CDCConnectorError : Exception
    {
        public CDCConnectorError()
        {
        }

        public CDCConnectorError(string? message) : base(message)
        {
        }

        public CDCConnectorError(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected CDCConnectorError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}