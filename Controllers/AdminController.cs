using QuanLyKhachSan.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Helpers;

namespace QuanLyKhachSan.Controllers
{
    [AdminAuthorize]
    public class AdminController : BaseController
    {
        QuanLyKhachSanDataContext db;
        public AdminController()
        {
            string conn = ConfigurationManager.ConnectionStrings["QLKSConnectionString"].ConnectionString;
            db = new QuanLyKhachSanDataContext(conn);
        }

        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public ActionResult Dashboard(DateTime? startDate, DateTime? endDate, int? resortId = null)
        {

            // 🔹 Mặc định: 30 ngày gần nhất
            if (startDate == null) startDate = DateTime.Today.AddDays(-30);
            if (endDate == null) endDate = DateTime.Today.AddDays(1); // thêm 1 ngày để bao gồm hôm nay

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            // 🔹 Danh sách resort để filter
            ViewBag.Resorts = db.Resorts.ToList();
            ViewBag.SelectedResortId = resortId;

            // 🔹 Lọc booking trong khoảng thời gian
            var bookings = db.Bookings
                .Where(b => b.created_at >= startDate && b.created_at < endDate)
                .Where(b => b.status != "cancelled"); // bỏ đơn hủy

            if (resortId != null && resortId > 0)
                bookings = bookings.Where(b => b.resort_id == resortId);

            // 🔹 Doanh thu dự kiến (Confirmed + Completed)
            decimal estimatedRevenue = bookings
                .Where(b => b.status == "confirmed" || b.status == "completed")
                .Sum(b => (decimal?)b.total_price) ?? 0m;

            // 🔹 Doanh thu thực tế (đã thanh toán)
            decimal actualRevenue = bookings
                .Where(b => b.payment_status == "paid")
                .Sum(b => (decimal?)b.total_price) ?? 0m;

            // 🔹 Gom doanh thu theo ngày cho biểu đồ
            var revenueData = bookings
                .Where(b => b.created_at != null) 
                .GroupBy(b => b.created_at)
                .Select(g => new
                {
                    Date = g.Key, // DateTime?
                    Estimated = g.Where(x => (x.status ?? "") == "confirmed" || (x.status ?? "") == "completed")
                                 .Sum(x => (decimal?)x.total_price) ?? 0m, 
                    Actual = g.Where(x => (x.payment_status ?? "") == "paid").Sum(x => (decimal?) x.total_price) ?? 0m
                })
                .OrderBy(x => x.Date)
                .ToList();

            // 🔹 Tổng đơn & khách
            int totalBookings = bookings.Count();
            int totalCustomers = bookings.Select(b => b.customer_id).Distinct().Count();

            // 🔹 Đưa dữ liệu sang View
            ViewBag.EstimatedRevenue = estimatedRevenue;
            ViewBag.ActualRevenue = actualRevenue;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.ChartData = Newtonsoft.Json.JsonConvert.SerializeObject(revenueData);

            return View();
        }

        // ===================== DANH SÁCH RESORT =====================
        public ActionResult Resorts()
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

        public ActionResult Customers()
        {
            var customers = db.Customers
                           .OrderByDescending(r => r.created_at)
                           .ToList();

            return View(customers);
        }

        public ActionResult Orders(DateTime? startDate, DateTime? endDate, int? resortId = null, string status = "")
        {
            // ✅ Nếu chưa chọn, mặc định xem 30 ngày gần nhất
            if (startDate == null) startDate = DateTime.Today.AddMonths(-1);
            if (endDate == null) endDate = DateTime.Today.AddDays(1); // cộng thêm 1 ngày để không bị thiếu hôm nay

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.Resorts = db.Resorts.ToList();
            ViewBag.SelectedResortId = resortId;

            // ✅ Base query
            var q = db.Bookings.Where(b => b.created_at >= startDate && b.created_at < endDate);

            // ✅ Bộ lọc thêm
            if (resortId != null && resortId > 0)
                q = q.Where(b => b.resort_id == resortId);
            if (!string.IsNullOrEmpty(status))
                q = q.Where(b => b.status == status);

            // ✅ JOIN sang các bảng liên quan
            var data = (
                from b in q
                join c in db.Customers on (int?)b.customer_id equals (int?)c.id into cj
                from c in cj.DefaultIfEmpty()

                join r in db.Resorts on (int?)b.resort_id equals (int?)r.id into rj
                from r in rj.DefaultIfEmpty()

                join rm in db.Rooms on (int?)b.room_id equals (int?)rm.id into rmj
                from rm in rmj.DefaultIfEmpty()

                orderby b.created_at descending
                select new AdminBookingVM
                {
                    Id = (int)b.id,
                    CustomerName = c != null ? c.name : "Khách vãng lai",
                    Email = b.email,
                    Phone = b.phone,
                    ResortName = r != null ? r.name : "Không xác định",
                    RoomName = rm != null ? rm.name : "Không xác định",
                    CheckIn = b.check_in,
                    CheckOut = b.check_out,
                    TotalPrice = (decimal)(b.total_price ?? 0),
                    Status = b.status,
                    PaymentStatus = b.payment_status,
                    CreatedAt = b.created_at
                }
            ).ToList();

            // ✅ Nếu không có kết quả, đảm bảo không crash View
            if (data == null || data.Count == 0)
            {
                ViewBag.Message = "Không có dữ liệu trong khoảng thời gian này.";
            }

            return View(data);
        }

        // GET: /Admin/CreateResort
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

            return RedirectToAction("Resorts");
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
        public ActionResult EditCustomer(int id)
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




        // POST
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

            return RedirectToAction("Resorts");
        }

        // =========================
        // 🗑️ XÓA RESORT
        // =========================
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
                    return RedirectToAction("Resorts");
                }

                // ⚠️ 2. Kiểm tra xem resort có liên quan đến room/booking hay không
                bool hasRooms = db.Rooms.Any(r => r.resort_id == id);
                bool hasBookings = db.Bookings.Any(b => b.resort_id == id);

                if (hasRooms || hasBookings)
                {
                    TempData["Error"] = "Không thể xóa vì resort đang có phòng hoặc đơn đặt liên kết.";
                    return RedirectToAction("Resorts");
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

            return RedirectToAction("Resorts");
        }

        // ========================== DANH SÁCH PHÒNG ==========================
        public ActionResult RoomList(int resortId)
        {
            var resort = db.Resorts.FirstOrDefault(r => r.id == resortId);
            if (resort == null) return HttpNotFound();

            ViewBag.ResortName = resort.name;
            ViewBag.ResortId = resortId;

            var rooms = db.Rooms
                .Where(r => r.resort_id == resortId)
                .OrderByDescending(r => r.created_at)
                .ToList();

            return View(rooms);
        }

        // ========================== THÊM PHÒNG MỚI ==========================
        [HttpGet]
        public ActionResult CreateRoom(int resortId)
        {
            ViewBag.ResortId = resortId;
            return View();
        }

        [HttpPost]
        public ActionResult CreateRoom(Room room, int resortId)
        {
            if (ModelState.IsValid)
            {
                room.resort_id = resortId;
                room.created_at = DateTime.Now;
                room.updated_at = DateTime.Now;
                db.Rooms.InsertOnSubmit(room);
                db.SubmitChanges();

                return RedirectToAction("RoomList", new { resortId = resortId });
            }

            ViewBag.ResortId = resortId;
            return View(room);
        }

        // ========================== SỬA PHÒNG ==========================
        [HttpGet]
        public ActionResult EditRoom(int id)
        {
            var room = db.Rooms.FirstOrDefault(r => r.id == id);
            if (room == null) return HttpNotFound();

            return View(room);
        }

        [HttpPost]
        public ActionResult EditRoom(Room updatedRoom)
        {
            var room = db.Rooms.FirstOrDefault(r => r.id == updatedRoom.id);
            if (room == null) return HttpNotFound();

            room.name = updatedRoom.name;
            room.price = updatedRoom.price;
            room.quantity = updatedRoom.quantity;
            room.room_amenities = updatedRoom.room_amenities;
            room.updated_at = DateTime.Now;

            db.SubmitChanges();

            return RedirectToAction("RoomList", new { resortId = room.resort_id });
        }

        // ========================== XOÁ PHÒNG ==========================
        public ActionResult DeleteRoom(int id)
        {
            var room = db.Rooms.FirstOrDefault(r => r.id == id);
            if (room == null) return HttpNotFound();

            long resortId = room.resort_id;  // ✅ Đúng kiểu dữ liệu long
            db.Rooms.DeleteOnSubmit(room);
            db.SubmitChanges();

            return RedirectToAction("RoomList", new { resortId = resortId });
        }

    }
}