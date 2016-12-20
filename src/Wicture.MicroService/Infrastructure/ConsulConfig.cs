namespace Wicture.MicroService.Models
{
    public class ConsulConfig
    {
        public string Address { get; set; }
    }

    public class ServiceConfig
    {
        public string Address { get; set; }
        public string ServiceName { get; set; }
        public string[] Tags { get; set; }
    }
}