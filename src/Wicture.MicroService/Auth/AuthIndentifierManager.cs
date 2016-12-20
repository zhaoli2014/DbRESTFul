using Wicture.DbRESTFul.Auth;

namespace Wicture.MicroService.Auth
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
            set
            {
                if (authIdentifier != value)
                {
                    authIdentifier = value;
                }
            }
        }
    }
}
