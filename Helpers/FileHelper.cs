using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Services.Description;

namespace QuanLyKhachSan.Helpers
{
    public class FileHelper
    {
        public static string UploadFile(HttpPostedFileBase file, string RootPath)
        {
            try
            {
                string extension = Path.GetExtension(file.FileName);
                string fileName;
                string PathAvatar;

                do
                {
                    fileName = BaseHelper.RandomString(20) + extension;
                    PathAvatar = RootPath + fileName;
                }
                while (File.Exists(PathAvatar));

                file.SaveAs(PathAvatar);

                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}