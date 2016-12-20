using System.Collections.Generic;

namespace Wicture.DbRESTFul
{
    public class PaginationListViewModel<M> where M : class
    {
        public IEnumerable<M> items { get; set; }

        public Pagination pagination { get; set; }
    }
}