using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class CustomerController : AdminController
    {
        // GET: Admin/Customer
        public CustomerController()
        {
            ViewBag.name = "customer";
        }

        public ActionResult Index()
        {
            var customers = db.Customers
                         .OrderByDescending(r => r.created_at)
                         .ToList();

            return View(customers);
        }

        public ActionResult Create()
        {
            return View();
        }
    }
}