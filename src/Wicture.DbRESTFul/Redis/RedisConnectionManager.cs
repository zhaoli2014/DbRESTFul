namespace Wicture.DbRESTFul.Redis
{
    public class RedisConnectionManager
    {
        public string ConnectionString
        {
            get
            {
                return ConfigurationManager.Settings.CRI.ConnectionString;
            }
        }
    }
}
