namespace Wicture.DbRESTFul
{
    public class UserTablePermission
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string TableName { get; set; }

        public bool List { get; set; }

        public bool Insert { get; set; }

        public bool Update { get; set; }

        public bool Delete { get; set; }
    }
}