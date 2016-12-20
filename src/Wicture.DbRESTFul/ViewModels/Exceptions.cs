using System;

namespace Wicture.DbRESTFul
{
    public class LogicalException : Exception
    {
        public string ErrorCode { get; set; }

        public LogicalException(string errorMessage)
            : base(errorMessage)
        {

        }

        public LogicalException(string errorMessage, string errorCode)
            : base(errorMessage)
        {
            ErrorCode = errorCode;
        }
    }

    public class CSIInvocationException : Exception
    {
        public CSIInvocationException(string errorMessage)
            : base(errorMessage)
        {

        }

        public CSIInvocationException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {

        }
    }
}
