namespace Wicture.DbRESTFul.Auth
{
    public static class AuthIndentifierManager
    {
        private static IAuthIdentifier authIdentifier;
        public static IAuthIdentifier AuthIdentifier
        {
            get
            {
                if (authIdentifier == null)
                {
                    authIdentifier = new DefaultAuthIdentifier();
                }

                return authIdentifier;
            }
        }

        public static void Use(IAuthIdentifier authIdentifier)
        {
            AuthIndentifierManager.authIdentifier = authIdentifier;
        }
    }
}
