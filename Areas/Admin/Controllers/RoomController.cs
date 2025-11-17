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
        public ActionResult Show(int id)
        {
            var room = db.Rooms.FirstOrDefault(r => r.id == id);

            if (room == null) 
                return HttpNotFound();

            ViewBag.Resorts = db.Resorts.ToList();
            return View("Form", room);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection, HttpPostedFileBase thumbnail, List<HttpPostedFileBase> images)
        {
            List<string> deletedImages = new List<string>();
            var room = db.Rooms.FirstOrDefault(r => r.id == id);

            if (room == null) 
                return HttpNotFound();

            Dictionary<string, string> changes = ModelHelper.DirtyModelFromCollection(room, collection);
            bool isChangeResort = changes.TryGetValue("resort_id", out string resort_id);

            if (isChangeResort && string.IsNullOrEmpty(resort_id))
            {
                Resort r = db.Resorts.FirstOrDefault(m => m.id == int.Parse(resort_id));

                if (r == null)
                {
                    TempData["error"] = "Resort is required";
                    TempData["form"] = room;

                    return Redirect(Request.UrlReferrer.ToString());
                }
            }

            if (thumbnail != null)
            {
                string thumbnailImage = FileHelper.UploadFile(thumbnail, PathHelper.GetUploadFilePath());

                if (! string.IsNullOrEmpty(thumbnailImage))
                {
                    deletedImages.Add(room.thumbnail);

                    room.thumbnail = thumbnailImage;
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

                deletedImages.AddRange(room.images.Split(',', (char) StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));

                room.images = pathImages.TrimEnd(',');
            }

            var provider = new DictionaryValueProvider<string>(changes, CultureInfo.CurrentCulture);
            room.updated_at = DateTime.Now;

            TryUpdateModel(room, provider);
            db.SubmitChanges();

            foreach (string item in deletedImages)
            {
                DeleteFile(item);
            }

            TempData["success"] = "Edit Room ID #"+ room.id +" successful.";

            return RedirectToAction("Index");
        }

        // ========================== XOÁ PHÒNG ==========================
        public ActionResult Delete(int id)
        {
            var room = db.Rooms.FirstOrDefault(r => r.id == id);
            if (room == null) return HttpNotFound();

            long resortId = room.resort_id;
            int orders = db.Bookings.Where(m => m.room_id == room.id).Count();

            if (orders > 0)
            {
                TempData["error"] = "This room is now available for reservation.";

                return Redirect(Request.UrlReferrer.ToString());
            }

            List<string> deletedImages = new List<string>();

            deletedImages.Add(room.thumbnail);

            deletedImages.AddRange(room.images.Split(',', (char)StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));

            db.Rooms.DeleteOnSubmit(room);
            db.SubmitChanges();

            foreach (string image in deletedImages)
            {
                DeleteFile(image);
            }

            TempData["success"] = "Delete Room ID #" + room.id + " successful.";

            return RedirectToAction("Index");
        }

        private bool DeleteFile(string fileName)
        {
            string root = "Images";

            return FileHelper.DeleteFile(fileName, root);
        }
    }
}