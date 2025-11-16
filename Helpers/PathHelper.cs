
using System.Web;

namespace QuanLyKhachSan.Helpers
{
    public class PathHelper
    {
        public static string GetUploadFilePath(string folder = "Images", string fileName = null)
        {
            return HttpContext.Current.Server.MapPath("~/Content/Upload/"+ folder + "/" + fileName);
        }
    }
}