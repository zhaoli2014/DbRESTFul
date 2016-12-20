
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Wicture.DbRESTFul.Controllers.V1
{
    [Route("dbrestful/api/permission")]
    public class PermissionController : ApiControllerBase<DbRESTFulRepository>
    {
        public PermissionController(ILoggerFactory loggerFactory) 
            : base(loggerFactory)
        {

        }

        [HttpPost]
        [Route("")]
        public object AddOrUpdatePermission([FromBody]AddOrUpdatePermissionViewModel model)
        {
            DbRESTFulRepository pr = new DbRESTFulRepository();
            return Result(pr.AddOrUpdatePermission(model.UserId, model.TableName, model.CanList, model.CanInsert, model.CanUpdate, model.CanDelete));
        }

        [HttpDelete]
        [Route("")]
        public object DeletePermission(int userId, string tableName)
        {
            DbRESTFulRepository pr = new DbRESTFulRepository();
            return Result(pr.DeletePermission(userId, tableName));
        }

        [HttpGet]
        [Route("")]
        public object ListPermission(int? userId = null, string tableName = null)
        {
            DbRESTFulRepository pr = new DbRESTFulRepository();
            return Result(pr.ListPermission(userId, tableName));
        }
    }
}
