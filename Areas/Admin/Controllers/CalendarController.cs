using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class CalendarController : AdminController
    {
        // GET: Admin/Calendar

        public CalendarController() {
            ViewBag.name = "resort";
        }

        public ActionResult Index(int? resortId = null, int ?roomId = null, DateTime? from = null, DateTime? to = null)
        {
            if (resortId == null)
            {
                TempData["error"] = "Resort is Not Found";

                return Redirect(Request.UrlReferrer.ToString());
            }

            Resort resort = db.Resorts.FirstOrDefault(m => m.id == resortId);

            if (resort == null)
            {
                TempData["error"] = "Resort is Not Found";

                return RedirectToAction("Index", "Resort");
            }

            var rooms = db.Bookings.Where(m => m.resort_id == resort.id).AsQueryable();
            List<Room> dataRooms = db.Rooms.Where(m => m.resort_id == resort.id).ToList();

            if (from != null)
            {
                rooms.Where(m => m.check_in >= (DateTime) from).AsQueryable();
            }
            
            if (to != null)
            {
                rooms.Where(m => m.check_in <= (DateTime) to).AsQueryable();
            }

            ViewBag.Resort = resort;
            ViewBag.Rooms = dataRooms;
            ViewBag.RoomId = roomId;

            return View(rooms.Where(m => m.status == "confirmed").ToList());
        }
    }
}