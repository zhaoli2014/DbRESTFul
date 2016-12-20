
namespace Wicture.DbRESTFul
{
    public class Pagination
    {
        public int size { get; set; }

        public int index { get; set; }

        public int count { get; set; }

        public Pagination()
        {
            size = 10;
        }

        public Pagination(int size, int index, int count)
        {
            this.size = size;
            this.index = index;
            this.count = count;
        }
    }
}
