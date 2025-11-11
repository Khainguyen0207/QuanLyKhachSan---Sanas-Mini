using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class OrderController : AdminController
    {
        // GET: Admin/Order

        public OrderController()
        {
            ViewBag.name = "order";
        }

        public ActionResult Index(DateTime? startDate, DateTime? endDate, int? resortId = null, string status = "")
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

    }
}