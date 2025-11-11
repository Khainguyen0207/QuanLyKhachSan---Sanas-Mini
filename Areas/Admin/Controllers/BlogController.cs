using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class BlogController : AdminController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}