using Consul;
using System;
using System.Collections.Generic;

namespace Wicture.MicroService
{
    public class ServiceCenter
    {
        public List<AgentService> QueryServices()
        {
            return null;
        }

        public List<AgentService> QuerySerices(string name)
        {
            throw new NotImplementedException();
        }
    }
}
