using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyKhachSan.Helpers;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Areas.Admin.Controllers
{
    public class RoomController : AdminController
    {
        public RoomController() {
            ViewBag.name = "room";
        }

        public ActionResult Index(int? resortId = null)
        {
            var rooms = new List<Room>();

            if (resortId == null)
            {
                rooms = db.Rooms.ToList();
            } else
            {
                Resort resort = db.Resorts.FirstOrDefault(r => r.id == resortId);

                if (resort == null)
                {
                    TempData["error"] = "Resort not found";

                    return Redirect(Request.UrlReferrer.ToString());
                }

                rooms = db.Rooms
                    .Where(r => r.resort_id == resortId)
                    .ToList();

                ViewBag.ResortId = resortId;
            }

            ViewBag.Resorts = db.Resorts.ToList();

            return View(rooms);
        }

        // ========================== THÊM PHÒNG MỚI ==========================
        public ActionResult Create()
        {
            ViewBag.Resorts = db.Resorts.ToList();

            return View("Form");
        }


        [HttpPost]
        public ActionResult Store(FormCollection collection, HttpPostedFileBase thumbnail, List<HttpPostedFileBase> images)
        {
            Room room = ModelHelper.CreateModelFromCollection<Room>(collection);
            Resort resort = db.Resorts.FirstOrDefault(m => m.id == room.resort_id);

            if (resort == null)
            {
                TempData["error"] = "Resort is required";
                TempData["form"] = room;

                return Redirect(Request.UrlReferrer.ToString());
            }

            if (thumbnail == null)
            {
                TempData["error"] = "Thumbnail is required";
                TempData["form"] = room;

                return Redirect(Request.UrlReferrer.ToString());
            }

            if (images.Count == 0)
            {
                TempData["error"] = "Images is required";
                TempData["form"] = room;

                return Redirect(Request.UrlReferrer.ToString());
            }

            string thumbnailImage = FileHelper.UploadFile(thumbnail, PathHelper.GetUploadFilePath());

            string pathImages = null;

            foreach (HttpPostedFileBase image in images)
            {
                string imageUpload = FileHelper.UploadFile(thumbnail, PathHelper.GetUploadFilePath());

                if (!string.IsNullOrEmpty(imageUpload))
                {
                    pathImages += imageUpload + ",";
                }
            }

            room.thumbnail = thumbnailImage;
            room.images = pathImages.TrimEnd(',');
            room.created_at = DateTime.Now;
            room.updated_at = DateTime.Now;

            db.Rooms.InsertOnSubmit(room);
            db.SubmitChanges();

            TempData["success"] = "Room create successful.";

            return RedirectToAction("Index");
        }
       
        // ========================== SỬA PHÒNG ==========================
        [HttpGet]
        public ActionResult EditRoom(int id)
        {
            var room = db.Rooms.FirstOrDefault(r => r.id == id);
            if (room == null) return HttpNotFound();

            ViewBag.Resorts = db.Resorts.ToList();
            return View("Form", room);
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