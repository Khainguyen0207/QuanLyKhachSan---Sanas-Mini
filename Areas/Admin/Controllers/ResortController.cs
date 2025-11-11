using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                OptionSuccess = r.option_success,
                OptionError = r.option_error,
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

        public ActionResult CreateResort()
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

            return View();
        }

        [HttpPost]
        public ActionResult CreateResort(Resort resort, string[] successOptions, string[] errorOptions)
        {
            // 🧩 1. Kiểm tra đăng nhập
            var user = Session["User"] as Customer;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 🧱 2. Sinh slug từ tên
            if (!string.IsNullOrEmpty(resort.name))
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

            // ⚙️ 3. Gán các thông tin bổ sung
            resort.option_success = successOptions != null ? string.Join(", ", successOptions) : "";
            resort.option_error = errorOptions != null ? string.Join(", ", errorOptions) : "";

            resort.customer_id = user.id;  // 🔥 đây là dòng bạn hỏi
            resort.created_at = DateTime.Now;
            resort.updated_at = DateTime.Now;

            // 💾 4. Lưu xuống DB
            db.Resorts.InsertOnSubmit(resort);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        // GET
        public ActionResult EditResort(int id)
        {
            var resort = db.Resorts.FirstOrDefault(r => r.id == id);
            if (resort == null) return HttpNotFound();

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

            return View(resort);
        }

        [HttpPost]
        public ActionResult EditResort(Resort resort, string[] successOptions, string[] errorOptions)
        {
            var res = db.Resorts.FirstOrDefault(r => r.id == resort.id);
            if (res == null) return HttpNotFound();

            res.name = resort.name;
            res.address = resort.address;
            res.option_success = successOptions != null ? string.Join(", ", successOptions) : "";
            res.option_error = errorOptions != null ? string.Join(", ", errorOptions) : "";
            res.updated_at = DateTime.Now;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DeleteResort(long id)
        {
            try
            {
                // 🔍 1. Tìm resort theo ID
                var resort = db.Resorts.FirstOrDefault(r => r.id == id);
                if (resort == null)
                {
                    TempData["Error"] = "Không tìm thấy resort cần xóa.";
                    return RedirectToAction("Index");
                }

                // ⚠️ 2. Kiểm tra xem resort có liên quan đến room/booking hay không
                bool hasRooms = db.Rooms.Any(r => r.resort_id == id);
                bool hasBookings = db.Bookings.Any(b => b.resort_id == id);

                if (hasRooms || hasBookings)
                {
                    TempData["Error"] = "Không thể xóa vì resort đang có phòng hoặc đơn đặt liên kết.";
                    return RedirectToAction("Index");
                }

                // ✅ 3. Xóa resort
                db.Resorts.DeleteOnSubmit(resort);
                db.SubmitChanges();

                TempData["Success"] = "Đã xóa resort thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa resort: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}