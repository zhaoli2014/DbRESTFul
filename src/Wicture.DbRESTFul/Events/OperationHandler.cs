using System;

namespace Wicture.DbRESTFul
{
    public static class OperationHandler
    {
        public static event Action<OperationArgs> Operating;
        public static event Action<OperationArgs> Operated;

        public static void OnOperating(OperationArgs args)
        {
            if (Operating != null)
            {
                try
                {
                    Operating(args);
                }
                catch (Exception ex)
                {
                    throw new OperationException("Handling Repository Operating event failed.", ex);
                }
            }
        }

        public static void OnOperated(OperationArgs args)
        {
            if (Operated != null)
            {
                try
                {
                    Operated(args);
                }
                catch (Exception ex)
                {
                    throw new OperationException("Handling Repository Operated event failed.", ex);
                }
            }
        }
    }
}
