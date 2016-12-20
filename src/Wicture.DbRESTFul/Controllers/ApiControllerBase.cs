using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using Wicture.DbRESTFul.Converters;

namespace Wicture.DbRESTFul.Controllers
{
    public abstract class ApiControllerBase<T> : Controller 
        where T : DbRESTFulRepository
    {
        protected readonly ILogger logger;

        protected T repository = null;
        private readonly bool checkPermission = false;

        public ApiControllerBase(ILoggerFactory loggerFactory, T repository = null)
	    {
            logger = loggerFactory.CreateLogger("API");
            this.repository = repository ?? (T)new DbRESTFulRepository();
            this.repository.CurrentUser = GetUserInfo();
	    }
         
        protected AjaxResult Result(object data)
        {
            return AjaxResult.SetResult(data);
        }

        protected AjaxResult Error(string message, string code)
        {
            return AjaxResult.SetError(message, code);
        }

        private IdentityInfo GetUserInfo()
        {
            var pricipal = User as ClaimsPrincipal;
            if (pricipal == null) return new IdentityInfo();

            var userIdclaim = pricipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var userNameclaim = pricipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var roleclaim = pricipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            var result = new IdentityInfo()
            {
                Id = userIdclaim == null || string.IsNullOrEmpty(userIdclaim.Value) ? "0" : userIdclaim.Value,
                Name = userNameclaim == null || string.IsNullOrEmpty(userNameclaim.Value) ? "" : userNameclaim.Value,
                Role = roleclaim == null || string.IsNullOrEmpty(roleclaim.Value) ? "" : roleclaim.Value
            };
            return result;
        }

        private string UserId
        {
            get
            {
                return GetUserInfo().Id;
            }
        }


        protected void CheckPermission(string obj, TableOperation operation, ObjectType type = ObjectType.TableOrView)
        {
            if (checkPermission && repository.HasPermission(obj, UserId, operation, type))
            {
                throw new DbRESTFulPermissionException() { Object = obj, Operation = operation };
            }
        }
    }
}