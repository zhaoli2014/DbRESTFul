
namespace Wicture.DbRESTFul
{
    public class AddOrUpdatePermissionViewModel
    {
        public int UserId { get; set; }

        public string TableName { get; set; }

        public bool CanList { get; set; }

        public bool CanInsert { get; set; }

        public bool CanUpdate { get; set; }

        public bool CanDelete { get; set; }
    }

}
