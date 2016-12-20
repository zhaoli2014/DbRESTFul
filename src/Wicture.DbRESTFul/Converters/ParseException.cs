using System;

namespace Wicture.DbRESTFul.Converters
{
    public class ParameterParseException : Exception
    {
        public ParameterParseException(string message)
            : base(message)
        {

        }
    }

    public class SqlParseException : Exception
    {
        public SqlParseException(string message)
            : base(message)
        {

        }
    }

    public class CSINameExistsException : Exception
    {
        public CSINameExistsException(string message)
            : base(message)
        {

        }
    }

    public class CRINameExistsException : Exception
    {
        public CRINameExistsException(string message)
            : base(message)
        {

        }
    }

    public class CacheNotExistException : Exception
    {
        public CacheNotExistException(string message)
            : base(message)
        {

        }
    }

    public class CSINotFoundException : Exception
    {
        public CSINotFoundException(string message)
            : base(message)
        {

        }
    }

    public class CSIValidationException : Exception
    {
        public CSIValidationException(string name, object data, ICSIValidator validator)
        {
            PropertyName = name;
            Value = data;
            CSIValidator = validator;
        }

        public CSIValidationException(string message) 
            : base(message)
        {
            this.message = message;
        }

        public string PropertyName { get; set; }
        public object Value { get; set; }
        public ICSIValidator CSIValidator { get; set; }

        private string message;
        public override string Message
        {
            get
            {
                return !string.IsNullOrEmpty(message) 
                    ? message 
                    : string.Format("CSI '{2}' validation failed for '{0}' with value '{1}'.", PropertyName, Value, CSIValidator.GetType().Name.Replace("Validator", ""));
            }
        }
    }

    public class DbRESTFulPermissionException : Exception
    {
        public string Object { get; set; }
        public TableOperation Operation { get; set; }

        public override string Message
        {
            get
            {
                return string.Format("The current user doesn't have [{0}] permission to '{1}'.", Operation, Object);
            }
        }
    }
}