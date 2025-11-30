using QuanLyKhachSan.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyKhachSan.Controllers
{
    public class BookingController : BaseController
    {
        public BookingController()  {}

        // -------------------- [ STEP 1: Trang nhập thông tin đặt phòng ] --------------------
        [HttpGet]
        public ActionResult Book(int roomId = -1, DateTime from = default(DateTime), DateTime to = default(DateTime))
        {
            if (roomId == -1 || from == default || to == default)
            {
                return HttpNotFound();
            }
            
            if (from < DateTime.Now)
            {
                return HttpNotFound();

            }

            if (to < from)
            {
                return HttpNotFound();
            }

            if (Session["User"] == null)
                return RedirectToAction("Login", "Account");

            var room = db.Rooms.FirstOrDefault(r => r.id == roomId);
            if (room == null)
                return HttpNotFound();

            ViewBag.Room = room;
            ViewBag.Resort = db.Resorts.FirstOrDefault(r => r.id == room.resort_id);
            ViewBag.CheckOut = to;
            ViewBag.CheckIn = from;
            ViewBag.Days = (to.Date - from.Date).TotalDays;

            return View();
        }

        // -------------------- [ STEP 2: Xác nhận thông tin & kiểm tra trùng email/phone ] --------------------
        [HttpPost]
        public ActionResult Confirm(int roomId, DateTime checkin, DateTime checkout, string email, string phone, string name)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "Account");

            var room = db.Rooms.FirstOrDefault(r => r.id == roomId);
            if (room == null)
                return HttpNotFound();

            // ✅ Tính tổng giá
            double days = (checkout.Date - checkin.Date).TotalDays;

            if (days < 1) days = 1;
            double total = (double)(room.price ?? 0) * days;

            ViewBag.Room = room;
            ViewBag.Resort = db.Resorts.FirstOrDefault(r => r.id == room.resort_id);
            ViewBag.Checkin = checkin;
            ViewBag.Checkout = checkout;
            ViewBag.Days = days;
            ViewBag.Adults = room.number_of_adults;
            ViewBag.Children = room.number_of_children;
            ViewBag.Total = total;
            ViewBag.Email = email;
            ViewBag.Phone = phone;
            ViewBag.Name = name;

            return View();
        }

        // -------------------- [ STEP 3: Thanh toán & lưu DB ] --------------------
        [HttpPost]
        public ActionResult Success(int roomId, DateTime checkin, DateTime checkout, double total, string email, string phone, string name)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "Account");

            var user = Session["User"] as Customer;
            var room = db.Rooms.FirstOrDefault(r => r.id == roomId);

            if (room == null)
                return HttpNotFound();

            int CountConflicts = db.Bookings
                .Where(b => b.room_id == room.id)
                .Where(b => b.check_in < checkout && b.check_out > checkin)
                .Count();


            // ✅ Kiểm tra còn phòng
            if (room.quantity - CountConflicts < 1)
            {
                ViewBag.Error = "❌ Rất tiếc! Phòng này đã hết chỗ.";
                return RedirectToAction("Index", "Room", new { resortId = room.resort_id });
            }

            // ✅ Tạo bản ghi đặt phòng
            Booking newBooking = new Booking    
            {
                customer_id = Convert.ToInt32(user.id),
                resort_id = room.resort_id,
                room_id = roomId,
                check_in = checkin,
                check_out = checkout,
                total_price = (decimal) total,
                total_price_temporary = (decimal) total,
                status = "pending",
                payment_status = "unpaid",
                note = "Thanh toán sau",
                email = email,
                phone = phone,
                name = name,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };


            db.Bookings.InsertOnSubmit(newBooking);
            db.SubmitChanges();

            ViewBag.CustomerName = user.name;
            ViewBag.CustomerEmail = user.email;
            ViewBag.ResortName = db.Resorts.FirstOrDefault(r => r.id == room.resort_id)?.name;
            ViewBag.RoomName = room.name;
            ViewBag.Total = total;
            ViewBag.Checkin = checkin.ToString("dd/MM/yyyy");
            ViewBag.Checkout = checkout.ToString("dd/MM/yyyy");

            return View();
        }
    }
}
