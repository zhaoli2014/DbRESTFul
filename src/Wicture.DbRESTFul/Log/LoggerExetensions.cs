using Microsoft.Extensions.Logging;
using System;

namespace Wicture.DbRESTFul
{
    public static class LoggerExetensions
    {
        public static void LogError(this ILogger logger, Exception exception, string message)
        {
            logger.LogError(new EventId(), exception, message);
        }
    }
}
