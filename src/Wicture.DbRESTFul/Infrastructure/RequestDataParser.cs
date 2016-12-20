using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Wicture.DbRESTFul.Infrastructure
{
    public class RequestDataParsingException : Exception
    {
        public RequestDataParsingException(string message, Exception innerExcption = null)
            : base(message, innerExcption)
        {

        }
    }

    public static class RequestDataParser
    {
        public static Action<JToken> PostParseQuery { get; set; }
        public static Action<JToken> PostParseBody { get; set; }

        public static JToken Parse(Schema[] query, Schema[] body, HttpContext httpContext)
        {
            JToken param = null;

            if (query != null && query.Length > 0 && httpContext.Request.Query != null)
            {
                param = ParseQuery(query, httpContext.Request.Query);
                VerifyParam(param, query);

                PostParseQuery?.Invoke(param);
            }

            if (body != null && body.Length > 0)
            {
                var data = ParseForm(body, httpContext.Request);
                VerifyParam(data, body);
                PostParseBody?.Invoke(param);

                param = (param != null && data != null && data is JObject) ? param.Merge(data) : data;
            }

            return param;
        }

        private static void VerifyParam(JToken param, Schema[] schema)
        {
            // TODO: 如果是JArray，需要分别处理。
            if (param is JArray) return;

            var keys = (param as IDictionary<string, JToken>).Keys.ToArray();
            foreach (var item in schema)
            {
                if (item.type == "array" || item.type == "object")
                {
                    continue;
                }
                else if (!item.nullable && (!keys.Contains(item.name, StringComparison.CurrentCultureIgnoreCase) || param[item.name] == null))
                {
                    throw new RequestDataParsingException($"The parameter '{item.name}' is not nullable.");
                }
            }
        }

        private static JToken ParseQuery(Schema[] schema, IQueryCollection query)
        {
            JToken param = new JObject();
            foreach (var item in query)
            {
                if (schema.All(s => !s.name.Equals(item.Key, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                if (item.Value.Count == 1)
                {
                    param[item.Key] = item.Value.First();
                }
                else if (item.Value.Count > 1)
                {
                    param[item.Key] = JToken.FromObject(item.Value.ToArray());
                }
            }

            return param;
        }

        private static JToken ParseForm(Schema[] schema, HttpRequest request)
        {
            using (StreamReader reader = new StreamReader(request.Body))
            {
                var body = reader.ReadToEnd();
                if (string.IsNullOrEmpty(body))
                {
                    return null;
                }

                string contentType = string.Empty;
                if (request.Headers.ContainsKey("Content-Type"))
                {
                    contentType = request.Headers["Content-Type"];
                }

                if (!string.IsNullOrEmpty(contentType) && contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                {
                    return ParseBodyForXWWWFormData(schema, body);
                }
                else if (body.Contains("Content-Disposition: form-data;"))
                {
                    // 处理 Multipart form-data类型的body。其格式如下：
                    //------WebKitFormBoundaryUiNRXQW4tEeI03kr
                    //Content-Disposition: form-data; name="size"

                    //10
                    //------WebKitFormBoundaryUiNRXQW4tEeI03kr
                    //Content-Disposition: form-data; name="test"

                    //asdfasd&asdf?afsdf=
                    //------WebKitFormBoundaryUiNRXQW4tEeI03kr--

                    return ParseMultipartFormData(body);
                }
                else
                {
                    return ParseJsonFormData(schema, body);
                }
            }
        }

        private static JToken ParseJsonFormData(Schema[] schema, string body)
        {
            try
            {
                var result = JToken.Parse(body);
                if(result is JObject)
                {
                    var param = result as JObject;
                    foreach (var item in param)
                    {
                        if (schema.All(s => !s.name.Equals(item.Key, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        param[item.Key] = item.Value;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new RequestDataParsingException("Failed to parse data from body", ex);
            }
        }


        private static JToken ParseMultipartFormData(string body)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(body)))
            {
                var parser = new MultipartFormDataParser(stream);
                JToken param = new JObject();

                foreach (var parameter in parser.Parameters)
                {
                    param[parameter.Name] = parameter.Data;
                }

                return param;
            }
        }

        private static JToken ParseBodyForXWWWFormData(Schema[] schema, string body)
        {
            JToken param = new JObject();
            string[] content = body.Split('&');
            for (int i = 0; i < content.Length; i++)
            {
                string[] fields = content[i].Split('=');
                if (schema.All(s => !s.name.Equals(fields[0], StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                param[fields[0]] = WebUtility.UrlDecode(fields[1]);
            }

            return param;
        }
    }
}
