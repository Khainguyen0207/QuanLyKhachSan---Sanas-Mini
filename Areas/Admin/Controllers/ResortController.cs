using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Helpers;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class ResortController : AdminController
    {
        public ResortController() {
            ViewBag.name = "resort";
        }

        // GET: Resort
        public ActionResult Index()
        {
            var resorts = (from r in db.Resorts
            let roomPrices = db.Rooms.Where(x => x.resort_id == r.id)
            select new ResortViewModel
            {
                Id = (int)r.id,
                Name = r.name,
                Address = r.address,
                Map = r.map,
                CreatedAt = r.created_at,
                UpdatedAt = r.updated_at,

                // ✅ Nếu resort chưa có phòng thì Min/Max = 0
                MinPrice = roomPrices.Any() ? (decimal)roomPrices.Min(x => x.price) : 0,
                MaxPrice = roomPrices.Any() ? (decimal)roomPrices.Max(x => x.price) : 0
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

            return View(resorts);
        }

        public ActionResult Create()
        {
            ViewBag.Features = new List<string>
            {
                "Máy lạnh", "Tivi", "Wi-Fi", "Bồn tắm", "Bàn làm việc",
                "Ban công", "Tủ lạnh", "Két an toàn", "Máy sấy tóc", "Hồ bơi"
            };

                ViewBag.Errors = new List<string>
            {
                "Không có bồn tắm", "Không có ban công", "Không có bếp riêng",
                "Không có máy giặt", "Không có chỗ đậu xe", "Wi-Fi yếu"
            };

            List<Customer> managers = db.Customers
                .Where(m => m.is_partner == true || m.is_admin == true)
                .ToList();

            ViewBag.Managers = managers;

            return View("Form");
        }

        [HttpPost]
        public ActionResult Store(FormCollection collection, HttpPostedFileBase thumbnail, List<HttpPostedFileBase> images)
        {
            Resort resort = ModelHelper.CreateModelFromCollection<Resort>(collection);

            if (! string.IsNullOrEmpty(resort.name))
            {
                resort.slug = resort.name
                    .Trim()
                    .ToLower()
                    .Replace(" ", "-")
                    .Replace("đ", "d")
                    .Replace("Đ", "D");
            }
            else
            {
                resort.slug = "resort-" + Guid.NewGuid().ToString().Substring(0, 8);
            }

            Customer customer = db.Customers.FirstOrDefault(m => m.id == resort.customer_id);

            if (customer == null)
            {
                TempData["error"] = "Customer manager is required";
                TempData["form"] = resort;

                return Redirect(Request.UrlReferrer.ToString());
            }

            if (thumbnail == null)
            {
                TempData["error"] = "Thumbnail is required";
                TempData["form"] = resort;

                return Redirect(Request.UrlReferrer.ToString());
            }
            
            if (images.Count == 0)
            {
                TempData["error"] = "Images is required";
                TempData["form"] = resort;

                return Redirect(Request.UrlReferrer.ToString());
            }

            string thumbnailImage = FileHelper.UploadFile(thumbnail, PathHelper.GetUploadFilePath());

            string pathImages = null;

            foreach (HttpPostedFileBase image in images)
            {
                string imageUpload = FileHelper.UploadFile(thumbnail, PathHelper.GetUploadFilePath());

                if (! string.IsNullOrEmpty(imageUpload))
                {
                    pathImages += imageUpload + ",";
                }
            }

            resort.thumbnail = thumbnailImage;
            resort.images = pathImages.TrimEnd(',');
            resort.created_at = DateTime.Now;
            resort.updated_at = DateTime.Now;

            db.Resorts.InsertOnSubmit(resort);
            db.SubmitChanges();

            TempData["success"] = "Resort create successful.";

            return RedirectToAction("Index");
        }

        // GET
        public ActionResult Show(int id)
        {
            var resort = db.Resorts.FirstOrDefault(r => r.id == id);

            if (resort == null)
            {
                TempData["error"] = "Resort not found";
                return RedirectToAction("Index");
            }

            List<Customer> managers = db.Customers
              .Where(m => m.is_partner == true || m.is_admin == true)
              .ToList();

            ViewBag.Managers = managers;

            return View("Form", resort);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection, HttpPostedFileBase thumbnail, List<HttpPostedFileBase> images)
        {
            List<string> deletedImages = new List<string>();

            var res = db.Resorts.FirstOrDefault(r => r.id == id);

            if (res == null) 
                return HttpNotFound();

            Dictionary<string, string> changes = ModelHelper.DirtyModelFromCollection(res, collection);

            if (thumbnail != null)
            {
                string thumbnailImage = FileHelper.UploadFile(thumbnail, PathHelper.GetUploadFilePath());

                if (!string.IsNullOrEmpty(thumbnailImage))
                {
                    deletedImages.Add(res.thumbnail);

                    res.thumbnail = thumbnailImage;
                }
            }

            if (images.Count > 0 && images[0] != null)
            {
                string pathImages = null;

                foreach (HttpPostedFileBase image in images)
                {
                    string imageUpload = FileHelper.UploadFile(image, PathHelper.GetUploadFilePath());

                    if (!string.IsNullOrEmpty(imageUpload))
                    {
                        
                        pathImages += imageUpload + ",";
                    }
                }

                deletedImages.AddRange(res.images.Split(',', (char) StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));

                res.images = pathImages.TrimEnd(',');
            }

            var provider = new DictionaryValueProvider<string>(changes, CultureInfo.CurrentCulture);
            res.updated_at = DateTime.Now;

            TryUpdateModel(res, provider);
            db.SubmitChanges();

            foreach (string image in deletedImages)
            {
                FileHelper.DeleteFile(image, "Images");
            }

            TempData["success"] = "Edit Room ID #" + res.id + " successful.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(long id)
        {
            try
            {
                var resort = db.Resorts.FirstOrDefault(r => r.id == id);

                if (resort == null)
                {
                    TempData["error"] = "Resort not found";
                    return RedirectToAction("Index");
                }

                bool hasRooms = db.Rooms.Any(r => r.resort_id == id);
                bool hasBookings = db.Bookings.Any(b => b.resort_id == id);

                if (hasRooms || hasBookings)
                {
                    TempData["error"] = "Không thể xóa vì resort đang có phòng hoặc đơn đặt liên kết.";
                    return RedirectToAction("Index");
                }

                List<string> deletedImages = new List<string>();
                deletedImages.Add(resort.thumbnail);
                deletedImages.AddRange(resort.images.Split(',', (char)StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));

                db.Resorts.DeleteOnSubmit(resort);
                db.SubmitChanges();

                foreach (string image in deletedImages)
                {
                    FileHelper.DeleteFile(image, "Images");
                }

                TempData["success"] = "Resort delete ID #" + resort.id + " successfully.";
            }
            catch (Exception ex)
            {
                TempData["error"] = "An unexpected error occurred!\n" + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}