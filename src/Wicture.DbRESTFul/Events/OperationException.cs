using System;

namespace Wicture.DbRESTFul
{
    public class OperationException : Exception
    {
        public OperationException()
        {
        }

        public OperationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
