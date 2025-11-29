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
                if (file.FileName == null)
                {
                    return null;
                }

                string extension = Path.GetExtension(file.FileName);

                string fileName;
                string PathAvatar;

                do
                {
                    fileName = BaseHelper.RandomString(20) + extension;

                    if (! Directory.Exists(RootPath))
                    {
                        Directory.CreateDirectory(RootPath);
                    }

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

        public static bool DeleteFile(string fileName, string RootPath)
        {
            try
            {
                var data = RootPath + fileName;
                string filePath = PathHelper.GetUploadFilePath(RootPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return File.Exists(filePath);
            } catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}