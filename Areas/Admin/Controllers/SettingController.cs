using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class SettingController : Controller
    {
        // GET: Admin/Setting

        public SettingController()
        {
            ViewBag.name = "sys-setting";
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}