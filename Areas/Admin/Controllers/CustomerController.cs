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

        //[HttpPost]
        public ActionResult DeleteCustomer(int id)
        {
            try
            {
                Customer customer = Session["User"] as Customer;

                if (customer.id == id)
                {
                    throw new Exception("Delete action failed.");
                }

                Customer customerDeleted = db.Customers.FirstOrDefault(m => m.id == id);

                if (customerDeleted == null)
                {
                    throw new Exception("Delete action failed.");
                }

                db.Customers.DeleteOnSubmit(customerDeleted);
                db.SubmitChanges();

                TempData["success"] = "Customer ID #" + id + " delete successful.";

                return RedirectToAction("Index");
            } catch (Exception e)
            {
                TempData["error"] = e.Message;

                return RedirectToAction("Index");
            }
        }
    }
}