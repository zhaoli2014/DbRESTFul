using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Wicture.MicroService
{
    public class LogHelper : Microsoft.Extensions.Logging.ILogger
    {
        private static string ToMsg(object message)
        {
            return message is string ? message.ToString() : JsonConvert.SerializeObject(message, Formatting.Indented);
        }

        public static void Debug(object message)
        {
            Serilog.Log.Debug(ToMsg(message));
        }

        public static void Debug(object message, Exception exception)
        {
            Serilog.Log.Debug(ToMsg(message), exception);
        }

        public static void Info(object message)
        {
            Serilog.Log.Information(ToMsg(message));
        }

        public static void Info(object message, Exception exception)
        {
            Serilog.Log.Information(ToMsg(message), exception);
        }

        public static void Warn(object message)
        {
            Serilog.Log.Warning(ToMsg(message));
        }

        public static void Warn(object message, Exception exception)
        {
            Serilog.Log.Warning(ToMsg(message), exception);
        }

        public static void Error(object message)
        {
            Serilog.Log.Error(ToMsg(message));
        }

        public static void Error(object message, Exception exception)
        {
            Serilog.Log.Error(ToMsg(message), exception);
        }


        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    Debug(formatter(state, exception), exception);
                    break;
                case LogLevel.Information:
                    Info(formatter(state, exception), exception);
                    break;
                case LogLevel.Warning:
                    Warn(formatter(state, exception), exception);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Error(formatter(state, exception), exception);
                    break;
                case LogLevel.None:
                    break;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}