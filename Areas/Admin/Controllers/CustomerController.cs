using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            HttpPostedFileBase file = Request.Files["avatar"];

            try
            {
                if (customer == null)
                {
                    TempData["error"] = "Customer is not found";

                    return RedirectToAction("Index");
                }

                Dictionary<string, string> changes = ModelHelper.DirtyModelFromCollection(customer, collection);
                bool issPasswordChange = changes.TryGetValue("password", out string password);
                bool isAvatarChange = changes.TryGetValue("avatar", out string avatar);

                if (issPasswordChange)
                {
                    if (string.IsNullOrEmpty(password.ToString()) || password.ToString().Length < 6)
                    {
                        changes.Remove("password");
                    }
                }

                if (file != null && file.ContentLength > 0)
                {
                    string PathAvatar = PathHelper.GetPathUploadAvatar(customer.avatar);

                    bool isExistAvatar = System.IO.File.Exists(PathAvatar);

                    if (! string.IsNullOrEmpty(customer.avatar) && isExistAvatar)
                    {
                        System.IO.File.Delete(PathAvatar);
                    }

                    string extension = Path.GetExtension(file.FileName);
                    string fileName;

                    do
                    {
                        fileName = BaseHelper.RandomString(20) + extension;
                        PathAvatar = PathHelper.GetPathUploadAvatar(fileName);
                    }
                    while (System.IO.File.Exists(PathAvatar));

                    file.SaveAs(PathAvatar);

                    changes.Add("avatar", fileName);
                }

                var provider = new DictionaryValueProvider<string>(changes, CultureInfo.CurrentCulture);
                
                TryUpdateModel(customer, provider);
                db.SubmitChanges();

                TempData["success"] = "Edit customer #" + customer.id + " successful";
            } catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction("Show", new { customer.id });
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