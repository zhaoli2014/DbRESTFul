using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Wicture.DbRESTFul.Controllers.V1
{
    [Route("dbrestful/api/procedure")]
    public class ProcedureController : ApiControllerBase<DbRESTFulRepository>
    {
        public ProcedureController(ILoggerFactory loggerFactory) 
            : base(loggerFactory)
        {

        }

        [Route("")]
        [HttpPost]
        public object Execute([FromBody]ExecutionViewModel viewModel)
        {
            base.CheckPermission(viewModel.Name, TableOperation.Select, ObjectType.StoreProcedure);
            return Result(repository.Execute(viewModel.Name, viewModel.Parameters));
        }
    }
}
