using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class DashboardController : AdminController
    {
        // GET: Dashboard

        public DashboardController()
        {
            ViewBag.name = "dashboard";
        }

        public ActionResult Index(DateTime? startDate, DateTime? endDate, int? resortId = null)
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
                    Actual = g.Where(x => (x.payment_status ?? "") == "paid").Sum(x => (decimal?)x.total_price) ?? 0m
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
    }
}