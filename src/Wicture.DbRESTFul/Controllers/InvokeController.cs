using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Wicture.DbRESTFul.Controllers.V1
{
    [Route("dbrestful/api/invoke")]
    public class InvokeController : ApiControllerBase<DbRESTFulRepository>
    {
        public InvokeController(ILoggerFactory loggerFactory) 
            : base(loggerFactory)
        {
            
        }

        [HttpPost]
        [Route("")]
        public object Execute([FromBody]ExecutionViewModel viewModel)
        {
            base.CheckPermission(viewModel.Name, TableOperation.Select, ObjectType.ConfiguredCodeInvocation);
            return Result(repository.Invoke(viewModel.Name, viewModel.Parameters));
        }
    }
}
