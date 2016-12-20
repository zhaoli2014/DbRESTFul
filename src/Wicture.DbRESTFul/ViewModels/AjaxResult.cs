
namespace Wicture.DbRESTFul
{
    public class AjaxResult
    {
        public string statusCode { get; set; }

        public string errorMessage { get; set; }

        public object data { get; set; }


        public static AjaxResult SetResult(object result)
        {
            return new AjaxResult() { statusCode = ResultCode.OK, data = result };
        }

        public static AjaxResult SetError(string error, string code)
        {
            return new AjaxResult() { errorMessage = error, statusCode = code.ToString() };
        }
    }
}