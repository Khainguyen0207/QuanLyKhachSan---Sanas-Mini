using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Helpers;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public abstract class AdminController : Controller
    {
        private static string conn = ConfigurationManager.ConnectionStrings["QLKSConnectionString"].ConnectionString;
        protected QuanLyKhachSanDataContext db = new QuanLyKhachSanDataContext(conn);
    }
}