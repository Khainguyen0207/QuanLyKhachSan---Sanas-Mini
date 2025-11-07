using QuanLyKhachSan.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QuanLyKhachSan.Controllers
{
    public class AccountController : Controller
    {
        QuanLyKhachSanDataContext db;
        public AccountController()
        {
            string conn = ConfigurationManager.ConnectionStrings["QLKSConnectionString"].ConnectionString;
            db = new QuanLyKhachSanDataContext(conn);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // -------------------- [ POST: Đăng nhập ] --------------------
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            var user = db.Customers.FirstOrDefault(u => u.email == email && u.password == password);

            if (user != null)
            {
                Session["User"] = user; // lưu người dùng

                // ✅ Nếu là admin → chuyển đến trang Admin
                if (user.is_admin == true)
                {
                    return RedirectToAction("Index", "Admin");
                }

                //// ✅ Nếu là partner (chủ resort) → chuyển đến trang riêng
                //if (user.is_partner == true)
                //{
                //    return RedirectToAction("PartnerDashboard", "Partner");
                //}

                // ✅ Còn lại là khách → trang HomePage
                return RedirectToAction("Index", "HomePage");
            }

            else
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                return View();
            }
        }



        // -------------------- [ GET: Đăng ký ] --------------------
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // -------------------- [ POST: Đăng ký ] --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string name, string email, string password, string confirmPassword)
        {
            // Kiểm tra rỗng
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            // Kiểm tra xác nhận mật khẩu
            if (password != confirmPassword)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp.";
                return View();
            }

            // Kiểm tra email đã tồn tại
            var existingUser = db.Customers.FirstOrDefault(c => c.email == email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email này đã được đăng ký.";
                return View();
            }

            // Tạo người dùng mới
            Customer newUser = new Customer
            {
                name = name,
                email = email,
                password = password,
                is_admin = false,
                is_partner = false,
                created_at = DateTime.Now
            };

            db.Customers.InsertOnSubmit(newUser);
            db.SubmitChanges();

            ViewBag.Success = "✅ Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // -------------------- [ Đăng xuất ] --------------------
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "HomePage");
        }

        // -------------------- [ GET: Thông tin cá nhân + lịch sử đặt phòng ] --------------------
        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "Account");

            var user = Session["User"] as Customer;

            var bookings = db.Bookings
                .Where(b => b.customer_id == user.id)
                .OrderByDescending(b => b.created_at)
                .ToList();

            ViewBag.User = user;
            return View(bookings);
        }

    }
}