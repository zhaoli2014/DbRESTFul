using System;

namespace Wicture.DbRESTFul
{
    public static class ApiInvocationHandler
    {
        public static event EventHandler<ApiInvokingArgs> Invoking;
        public static event EventHandler<ApiInvokedArgs> Invoked;
        public static event EventHandler<ApiExceptionArgs> ErrorOccured;

        public static void OnInvoking(ApiInvokingArgs args)
        {
            try
            {
                Invoking?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                throw new ApiInvocationException("Handling Api Invoking event failed.", ex);
            }
        }

        public static void OnInvoked(ApiInvokedArgs args)
        {
            try
            {
                Invoked?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                throw new ApiInvocationException("Handling Api Invoked event failed.", ex);
            }
        }

        public static void OnError(ApiExceptionArgs args)
        {
            try
            {
                ErrorOccured?.Invoke(null, args);
            }
            catch (Exception ex)
            {
                throw new ApiInvocationException("Handling Api error event failed.", ex);
            }
        }
    }
}
