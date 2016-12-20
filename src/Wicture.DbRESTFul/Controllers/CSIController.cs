using Newtonsoft.Json.Linq;
using System;
using Microsoft.AspNetCore.Mvc;
using Wicture.DbRESTFul.Resources;

namespace Wicture.DbRESTFul.Controllers
{
    [Route("api/v1/csi")]
    public class CSIController : Controller
    {
        [HttpGet]
        [Route("query")]
        public object List()
        {
            return JArray.FromObject(ServiceResourceManager.CSIs);
        }

        [HttpGet]
        [Route("get")]
        public object Get(string module, string name = "")
        {
            return "value";
        }

        [HttpPost]
        [Route("create")]
        public void Create(JObject module)
        {

        }

        [HttpPost]
        [Route("push")]
        public object Push(JObject codes)
        {
            if(codes != null)
            {
                try
                {
                    ConfiguredCode csi = codes.ToObject<ConfiguredCode>();
                    return AjaxResult.SetResult("done!");
                }
                catch (Exception ex)
                {
                    return AjaxResult.SetError(ex.Message, "500");
                }
            }

            return AjaxResult.SetError("arguments is null", "500");
        }

        [HttpPut]
        [Route("update")]
        public void Update(string module, string value)
        {

        }

        [HttpDelete]
        [Route("delete")]
        public void Delete(string module)
        {
        }
    }
}
