using Microsoft.Extensions.Logging;
using Wicture.DbRESTFul.Log;

namespace Wicture.DbRESTFul
{
    public static class LoggerManager
    {
        private static ILogger logger;
        public static ILogger Logger
        {
            get
            {
                if (logger == null)
                {
                    logger = new DefaultLogger();
                }

                return logger;
            }
        }

        public static void Use(ILogger logger)
        {
            LoggerManager.logger = logger;
        }
    }
}
