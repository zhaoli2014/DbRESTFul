using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Wicture.DbRESTFul
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger logger;

        public ApiExceptionFilterAttribute(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("ApiExceptionFilter");
        }

        public ApiExceptionFilterAttribute(ILogger logger)
        {
            this.logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            logger.LogError("API excution exception", context.Exception);

            AjaxResult result;

            if (context.Exception is LogicalException)
            {
                result = AjaxResult.SetError(context.Exception.Message, (context.Exception as LogicalException).ErrorCode);
            }
            else
            {
                result = AjaxResult.SetError(
                    ConfigurationManager.Settings.API.ShowFullException
                    ? context.Exception.ToString()
                    : context.Exception.Message, ResultCode.InternalError.ToString());
            }

            context.Result = new JsonResult(result);

            base.OnException(context);
        }
    }
}
