namespace Wicture.DbRESTFul
{
    public class OperationArgs
    {
        public object Operator { get; set; }
        public TableOperation Operation { get; set; }
        public ObjectType ObjectType { get; set; }
        public string Target { get; set; }
        public object Data { get; set; }
        public object Result { get; set; }
        public bool QueryOnly { get; set; }

        public OperationArgs()
        {

        }

        public OperationArgs(object _operator, TableOperation operation, ObjectType objectType, string target, object data, object result = null, bool queryOnly = false)
        {
            Operator = _operator;
            Operation = operation;
            ObjectType = objectType;
            Target = target;
            Data = data;
            Result = result;
            QueryOnly = queryOnly;
        }
    }
}
