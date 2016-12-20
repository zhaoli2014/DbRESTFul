using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Wicture.DbRESTFul.Controllers.V1
{
    [Route("dbrestful/api/query")]
    public class QueryController : ApiControllerBase<DbRESTFulRepository>
    {
        public QueryController(ILoggerFactory loggerFactory) 
            : base(loggerFactory)
        {

        }

        [HttpGet]
        [Route("{tableName}")]
        public object Select(string tableName, string parameters = null)
        {
            base.CheckPermission(tableName, TableOperation.Select);
            return Result(repository.Query(tableName, parameters));
        }

        [HttpPost]
        [Route("{tableName}")]
        public object Create(string tableName, [FromBody]QueryViewModel model)
        {
            base.CheckPermission(tableName, TableOperation.Insert);
            return Result(repository.Insert(tableName, model.Data));
        }

        [HttpPut]
        [Route("{tableName}")]
        public object Update(string tableName, [FromBody]QueryViewModel model)
        {
            base.CheckPermission(tableName, TableOperation.Update);
            return Result(repository.Update(tableName, model.Parameters, model.Data));
        }

        [HttpDelete]
        [Route("{tableName}")]
        public object Delete(string tableName, [FromBody]QueryViewModel model)
        {
            base.CheckPermission(tableName, TableOperation.Delete);
            return Result(repository.Delete(tableName, model.Parameters));
        }
    }
}