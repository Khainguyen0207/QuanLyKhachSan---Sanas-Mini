using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Areas.Admin.Validations;
using QuanLyKhachSan.Helpers;
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

        [HttpPost]
        public ActionResult Store(FormCollection collection)
        {
            List<string> errors = new CustomerValidation(collection).Validation();

            if (errors.Count > 0)
            {
                TempData["error"] = errors[0].ToString();
                return RedirectToAction("Index");
            }

            Customer customer = db.Customers.FirstOrDefault(r => r.email == collection["email"]);

            if (customer != null)
            {
                TempData["error"] = "Email already in use";

                ViewBag.Collecttion = collection;

                return RedirectToAction("Index");
            }

            try
            {
                Customer c = ModelHelper.CreateModelFromCollection<Customer>(collection);

                db.Customers.InsertOnSubmit(c);
                db.SubmitChanges();

                TempData["success"] = "Customer create successful.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Show(int id)
        {
            Customer customer = db.Customers.FirstOrDefault(r => r.id == id);

            if (customer == null)
            {
                TempData["error"] = "Customer is not found";

                return RedirectToAction("Index");
            }

            return View("Form", customer);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            Customer customer = db.Customers.FirstOrDefault(r => r.id == id);

            try
            {
                if (customer == null)
                {
                    TempData["error"] = "Customer is not found";

                    return RedirectToAction("Index");
                }

                Dictionary<string, string> changes = ModelHelper.DirtyModelFromCollection(customer, collection);
                
                if (string.IsNullOrEmpty(changes["password"]) || changes["password"].Length < 6)
                {
                    changes.Remove("password");
                }

                var provider = new DictionaryValueProvider<string>(changes, CultureInfo.CurrentCulture);
                
                //changes;
                TryUpdateModel(customer, provider);
                db.SubmitChanges();

                TempData["success"] = "Edit customer #" + customer.id + " successful";
            } catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return View("Form", customer);
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