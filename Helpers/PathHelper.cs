
using System.Web;

namespace QuanLyKhachSan.Helpers
{
    public class PathHelper
    {
        public static string GetPathUploadAvatar(string fileName = null)
        {
            return HttpContext.Current.Server.MapPath("~/Content/Upload/Avatar/" + fileName);
        }
    }
}