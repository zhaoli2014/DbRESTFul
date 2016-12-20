using System.Collections.Generic;

namespace Wicture.MicroService.Infrastructure
{
    public static class RoutesTable
    {
        public static List<string> Routes { get; private set; }

        static RoutesTable()
        {
            Routes = new List<string>();
        }
    }
}