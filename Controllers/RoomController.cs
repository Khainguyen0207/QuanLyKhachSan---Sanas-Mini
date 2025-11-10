using QuanLyKhachSan.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using System.Web.WebPages;

namespace QuanLyKhachSan.Controllers
{
    public class RoomController : BaseController
    {
        QuanLyKhachSanDataContext db;
        private int PerPage = 12;
        public RoomController()
        {
            string conn = ConfigurationManager.ConnectionStrings["QLKSConnectionString"].ConnectionString;
            db = new QuanLyKhachSanDataContext(conn);
        }

        // -------------------- [ GET: Danh sách phòng theo resort ] --------------------
        public ActionResult ListByResort(int resortId = -1, string search = null, string sort = null, int page = 1, string priceRange = null)
        {
            var rooms = db.Rooms.AsQueryable();

            if (resortId != -1)
            {
                var resorts = db.Resorts.Where(m => m.id == resortId).FirstOrDefault();

                if (resorts == null)
                {
                    return HttpNotFound();
                }

                rooms = rooms.Where(r => r.resort_id == resortId && r.quantity > 0).AsQueryable();
            }
            
            // -------------------- [ Tìm kiếm theo tên / mô tả ] --------------------
            if (!string.IsNullOrEmpty(search))
            {
                rooms = rooms.Where(r => r.name.Contains(search) || r.description.Contains(search) || r.Resort.address.Contains(search));
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                string[] range = priceRange.Trim().Split('-');
                
                int from = int.Parse(range[0]);
                int to = int.Parse(range[range.Length - 1]);

                if (from == to)
                {
                    rooms = rooms.Where(r => r.price >= from);

                }
                else
                {
                    rooms = rooms.Where(r => r.price >= from && r.price <= to);
                }
            }

            // -------------------- [ Sắp xếp theo giá hoặc tên ] --------------------
            switch (sort)
            {
                case "price_asc":
                    rooms = rooms.OrderBy(r => r.price);
                    break;
                case "price_desc":
                    rooms = rooms.OrderByDescending(r => r.price);
                    break;
                case "name_asc":
                    rooms = rooms.OrderBy(r => r.name);
                    break;
                case "name_desc":
                    rooms = rooms.OrderByDescending(r => r.name);
                    break;
                default:
                    rooms = rooms.OrderBy(r => r.id);
                    break;
            }

            // -------------------- [ Phân trang ] --------------------
            int totalItems = rooms.Count();
            int totalPages = (int)Math.Floor((double)totalItems / PerPage);

            var dataRooms = rooms.Skip((page - 1) * PerPage).Take(PerPage).ToList().Select(room =>
            {
                return new RoomViewModel
                {
                    Room = room,
                    address = room.Resort.address,
                };
            }).ToList();

            // -------------------- [ Truyền dữ liệu sang View ] --------------------
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("Index", dataRooms);
        }

        // -------------------- [ GET: Chi tiết phòng ] --------------------
        public ActionResult Details(int id = -1)
        {
            var room = db.Rooms.FirstOrDefault(r => r.id == id);

            if (room == null)
            {
                return HttpNotFound();
            }

            var resort = db.Resorts.FirstOrDefault(r => r.id == room.resort_id);
            ViewBag.Resort = resort;

            return View(room);
        }

        public ActionResult Index(int page = 1)
        {

            if (page < 1)
            {
                page = 1;
            }

            List<Room> query = db.Rooms
                .Where(m => m.quantity > 0)
                .OrderBy(p => p.id).ToList();

            var dataRooms = query.Skip((page - 1) * PerPage).Take(PerPage).Select(room => 
            {   
                return new RoomViewModel
                {
                    Room = room,
                    address = room.Resort.address,
                };
            }).ToList();


            int totalItems = query.Count();
            int totalPages = (int)Math.Floor((double) totalItems / PerPage);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(dataRooms);
        }

        
        public string GetFirstImage(string images)
        {
            if (string.IsNullOrEmpty(images)) return "default.jpg";
            var clean = images.Replace("[", "").Replace("]", "").Replace("\"", "");
            return clean.Split(',').Select(x => x.Trim()).FirstOrDefault() ?? "default.jpg";
        }
    }
}