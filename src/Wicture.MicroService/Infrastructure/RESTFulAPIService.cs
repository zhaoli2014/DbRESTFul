using Polly;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Wicture.DbRESTFul;

namespace Wicture.MicroService
{
    public static class RESTFulAPIService
    {
        public const int ServiceErrorCode = 600;

        private static HttpStatusCode[] HttpStatusCodesWorthRetrying =
        {
            HttpStatusCode.RequestTimeout, // 408
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout, // 504
        };


        /// <summary>
        /// 异步调用Get类型服务
        /// </summary>
        /// <param name="url">指定的Url</param>
        /// <param name="param">请求参数，可以是n=v&s=2形式的字符串，也可以是一个POCO，如：new User { Name = "Jason", Id = 10 }类型</param>
        /// <param name="retries">重试次数，默认2次。对网络超时，或者网络暂时不可用，提供重试机制，重试次数不包括第一次调用。每次重试间隔500毫秒。</param>
        /// <returns>
        ///     返回如果指定AjaxResult结构数据。该方法无异常抛出，错误数据通过AjaxResult数据的statusCode与errorMessage返回。
        /// </returns>
        public static async Task<T> GetAsync<T>(string url, object param = null, int retries = 2)
            where T : class, new()
        {
            return await MakeRequest<T>(url, (c, u, d) => c.GetAsync(u), null, param, retries);
        }

        /// <summary>
        /// 异步调用Post类型服务
        /// </summary>
        /// <param name="url">指定的Url</param>
        /// <param name="data">表单参数，该数据会作为Post的Body以Json的形式调用。</param>
        /// <param name="param">请求参数，可以是n=v&s=2形式的字符串，也可以是一个POCO，如：new User { Name = "Jason", Id = 10 }类型</param>
        /// <param name="retries">重试次数，默认2次。对网络超时，或者网络暂时不可用，提供重试机制，重试次数不包括第一次调用。每次重试间隔500毫秒。</param>
        /// <returns>
        ///     返回如果指定AjaxResult结构数据。该方法无异常抛出，错误数据通过AjaxResult数据的statusCode与errorMessage返回。
        /// </returns>
        public static async Task<T> PostAsync<T>(string url, object data, object param = null, int retries = 2)
            where T : class, new()
        {
            return await MakeRequest<T>(url, (c, u, d) => c.PostAsJsonAsync(u, d), data, param, retries);
        }

        /// <summary>
        /// 异步调用Put类型服务
        /// </summary>
        /// <param name="url">指定的Url</param>
        /// <param name="data">表单参数，该数据会作为Post的Body以Json的形式调用。</param>
        /// <param name="param">请求参数，可以是n=v&s=2形式的字符串，也可以是一个POCO，如：new User { Name = "Jason", Id = 10 }类型</param>
        /// <param name="retries">重试次数，默认2次。对网络超时，或者网络暂时不可用，提供重试机制，重试次数不包括第一次调用。每次重试间隔500毫秒。</param>
        /// <returns>
        ///     返回如果指定AjaxResult结构数据。该方法无异常抛出，错误数据通过AjaxResult数据的statusCode与errorMessage返回。
        /// </returns>
        public static async Task<T> PutAsync<T>(string url, object data, object param = null, int retries = 2)
            where T : class, new()
        {
            return await MakeRequest<T>(url, (c, u, d) => c.PutAsJsonAsync(u, d), data, param, retries);
        }

        /// <summary>
        /// 异步调用Delete类型服务
        /// </summary>
        /// <param name="url">指定的Url</param>
        /// <param name="param">请求参数，可以是n=v&s=2形式的字符串，也可以是一个POCO，如：new User { Name = "Jason", Id = 10 }类型</param>
        /// <param name="retries">重试次数，默认2次。对网络超时，或者网络暂时不可用，提供重试机制，重试次数不包括第一次调用。每次重试间隔500毫秒。</param>
        /// <returns>
        ///     返回如果指定AjaxResult结构数据。该方法无异常抛出，错误数据通过AjaxResult数据的statusCode与errorMessage返回。
        /// </returns>
        public static async Task<T> DeleteAsync<T>(string url, object param = null, int retries = 2)
            where T : class, new()
        {
            return await MakeRequest<T>(url, (c, u, d) => c.DeleteAsync(u), null, param, retries);
        }


        private static string Parameterize(object param)
        {
            var properties =
                param.GetType()
                    .GetTypeInfo()
                    .GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            var result = properties.Select(pi => $"{pi.Name}={pi.GetValue(param)}");

            return string.Join("&", result);
        }

        private static async Task<T> MakeRequest<T>(string url,
            Func<HttpClient, string, object, Task<HttpResponseMessage>> request, object data, object param, int retries)
            where T : class, new()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var requestUri = url;
                    if (param is string && !string.IsNullOrEmpty(param.ToString()))
                    {
                        requestUri += "?" + param;
                    }
                    else if (param != null)
                    {
                        requestUri += "?" + Parameterize(param);
                    }

                    var policy = Policy.Handle<HttpRequestException>()
                        .OrResult<HttpResponseMessage>(
                            r => { return HttpStatusCodesWorthRetrying.Contains(r.StatusCode); })
                        .WaitAndRetryAsync(retries,
                            (retryAttempt => { return TimeSpan.FromMilliseconds(500*retryAttempt); }));

                    var result =
                        await policy.ExecuteAsync(async () => { return await request(client, requestUri, data); });

                    result.EnsureSuccessStatusCode();

                    return await result.Content.ReadAsAsync<T>();
                }
            }
            catch (HttpRequestException ex)
            {
                if (typeof(T) == typeof(AjaxResult))
                {
                    return
                        new AjaxResult
                        {
                            errorMessage = ex.Message + "  " + ex.InnerException?.Message,
                            statusCode = ServiceErrorCode.ToString()
                        } as T;
                }

                throw;
            }
            catch (Exception ex)
            {
                if (typeof(T) == typeof(AjaxResult))
                {
                    return new AjaxResult {errorMessage = ex.Message, statusCode = ServiceErrorCode.ToString()} as T;
                }

                throw;
            }
        }
    }
}