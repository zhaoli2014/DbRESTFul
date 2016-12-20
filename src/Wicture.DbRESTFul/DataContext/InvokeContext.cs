using Newtonsoft.Json.Linq;
using System.Data;

namespace Wicture.DbRESTFul
{
    public class InvokeContext
    {
        public DbRESTFulRepository Repository { get; set; }
        public IDbConnection Connection { get; set; }
        public IDbTransaction Transaction { get; set; }
        public JToken Param { get; set; }
        public object CommandParam { get; set; }
        public CSIItem CodeItem { get; set; }
        public object Result { get; set; }

        public void FireOperatingEvent(ObjectType objectType = ObjectType.ConfiguredCodeInvocation)
        {
            var args = new OperationArgs
            {
                Operator = Repository.CurrentUser.Id,
                Operation = TableOperation.Invoke,
                ObjectType = objectType,
                Target = CodeItem.name,
                Data = CommandParam
            };
            OperationHandler.OnOperating(args);
        }

        public void FireOperatedEvent(ObjectType objectType = ObjectType.ConfiguredCodeInvocation)
        {
            var args = new OperationArgs
            {
                Operator = Repository.CurrentUser.Id,
                Operation = TableOperation.Invoke,
                ObjectType = objectType,
                Target = CodeItem.name,
                Data = CommandParam,
                Result = Result,
                QueryOnly = CodeItem.queryOnly
            };
            OperationHandler.OnOperating(args);
        }
    }
}