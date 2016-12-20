using Microsoft.AspNetCore.Http;
using System;

namespace Wicture.DbRESTFul
{
    public class ApiInvokingArgs
    {
        public long InvokingAt { get; private set; }
        public HttpContext Context { get; private set; }
        public ConfiguredAPI API { get; private set; }
        public object Param { get; private set; }

        public ApiInvokingArgs(HttpContext context, ConfiguredAPI api, object param)
        {
            InvokingAt = DateTime.Now.GetUnixLongTime();
            Context = context;
            API = api;
            Param = param;
        }
    }

    public class ApiInvokedArgs
    {
        public long InvokingAt { get { return invokingArgs.InvokingAt; } }
        public HttpContext Context { get { return invokingArgs.Context; } }
        public ConfiguredAPI API { get { return invokingArgs.API; } }
        public object Param { get { return invokingArgs.Param; } }

        public IdentityInfo Identity { get; private set; }
        public long InvokedAt { get; private set; }
        public object Result { get; private set; }

        private readonly ApiInvokingArgs invokingArgs;

        public ApiInvokedArgs(ApiInvokingArgs invokingArgs, IdentityInfo identity, object result)
        {
            this.invokingArgs = invokingArgs;
            InvokedAt = DateTime.Now.GetUnixLongTime();
            Identity = identity;
            Result = result;
        }
    }

    public class ApiExceptionArgs
    {
        public long InvokingAt { get { return invokingArgs.InvokingAt; } }
        public HttpContext Context { get { return invokingArgs.Context; } }
        public ConfiguredAPI API { get { return invokingArgs.API; } }
        public object Param { get { return invokingArgs.Param; } }

        public IdentityInfo Identity { get; private set; }
        public long InvokedAt { get; private set; }

        public Exception Exception { get; private set; }

        private readonly ApiInvokingArgs invokingArgs;

        public ApiExceptionArgs(ApiInvokingArgs invokingArgs, IdentityInfo identity, Exception exception)
        {
            this.invokingArgs = invokingArgs;
            InvokedAt = DateTime.Now.GetUnixLongTime();
            Identity = identity;
            Exception = exception;
        }
    }
}
