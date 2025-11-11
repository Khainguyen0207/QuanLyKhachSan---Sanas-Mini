using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class RoomController : AdminController
    {
        public RoomController() {
            ViewBag.name = "resort";
        }

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