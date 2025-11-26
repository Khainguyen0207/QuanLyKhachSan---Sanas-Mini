using QuanLyKhachSan.Models;
using QuanLyKhachSan.Requests;
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
        private const int PerPage = 12;

        public RoomController()
        {
            string conn = ConfigurationManager.ConnectionStrings["QLKSConnectionString"].ConnectionString;
            db = new QuanLyKhachSanDataContext(conn);
        }

        public ActionResult Index(ResortFilterRequest filter)
        {
            // check-in: DateTime
            DateTime checkin = filter.CheckinDate ?? DateTime.Now.AddDays(1);
            // check-out: DateTime
            DateTime checkout = filter.CheckoutDate ?? checkin.AddDays(1);
         
            var rooms = db.Rooms.AsQueryable();

            // -------------------- [ Lọc theo resort ] --------------------
            if (filter.ResortId.HasValue)
            {
                var resort = db.Resorts.FirstOrDefault(m => m.id == filter.ResortId.Value);

                if (resort == null)
                {
                    return HttpNotFound();
                }

                rooms = rooms.Where(r => r.resort_id == filter.ResortId.Value && r.quantity > 0);
            }

            // -------------------- [ Tìm kiếm theo tên / mô tả / địa chỉ ] --------------------
            if (! string.IsNullOrWhiteSpace(filter.Search))
            {
                string keyword = filter.Search.Trim();
                rooms = rooms.Where(r =>
                    r.name.Contains(keyword) ||
                    r.description.Contains(keyword) ||
                    r.Resort.address.Contains(keyword)
                );
            }
            
            // -------------------- [ Lọc theo khoảng giá ] --------------------
            if (! string.IsNullOrWhiteSpace(filter.PriceRange))
            {
                var parts = filter.PriceRange.Split('-');

                if (parts.Length >= 1)
                {
                    int from;
                    int to;

                    // parse mềm, tránh văng exception
                    int.TryParse(parts[0].Trim(), out from);

                    if (parts.Length > 1 && int.TryParse(parts[parts.Length - 1].Trim(), out to))
                    {
                        if (from == to)
                        {
                            rooms = rooms.Where(r => r.price >= from);
                        }
                        else
                        {
                            rooms = rooms.Where(r => r.price >= from && r.price <= to);
                        }
                    }
                    else
                    {
                        rooms = rooms.Where(r => r.price >= from);
                    }
                }
            }

            // -------------------- [ Lọc theo khoảng giá ] --------------------
            if (filter.Adults.HasValue)
            {
                rooms = rooms.Where(m => m.number_of_adults >= filter.Adults);
            } 
            
            if (filter.Children.HasValue)
            {
                rooms = rooms.Where(m => m.number_of_children >= filter.Children);
            }
            
            if (filter.Quantity.HasValue)
            {
                rooms = rooms.Where(m => m.quantity >= filter.Quantity);
            }

            // -------------------- [ Sắp xếp ] --------------------
            switch (filter.Sort)
            {
                case "price_asc":
                    rooms = rooms.OrderBy(r => r.price);
                    break;
                case "price_desc":
                    rooms = rooms.OrderByDescending(r => r.price);
                    break;
                default:
                    rooms = rooms.OrderBy(r => r.id);
                    break;
            }

            // -------------------- [ Phân trang ] --------------------

            rooms = rooms.Where(m => m.quantity > 0).ToList().AsQueryable();

            int totalItems = rooms.Count();
            int totalPages = (int) Math.Ceiling((double) totalItems / PerPage);

            int currentPage = filter.Page < 1 ? 1 : filter.Page;

            var dataRooms = rooms
                .Skip((currentPage - 1) * PerPage)
                .Take(PerPage)
                .ToList()
                .Select(room => new RoomViewModel
                {
                    Room = room,
                    address = room.Resort.address
                })
                .ToList();

            ViewBag.Rooms = rooms;
            ViewBag.Search = filter.Search;
            ViewBag.Sort = filter.Sort;
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.PriceRange = filter.PriceRange;
            ViewBag.CheckIn = checkin.ToString("yyyy-MM-dd");
            ViewBag.CheckOut = checkout.ToString("yyyy-MM-dd");
            ViewBag.ResortId = filter.ResortId;
            ViewBag.adults = filter.Adults;
            ViewBag.children = filter.Children;
            ViewBag.quantity = filter.Quantity;


            return View(dataRooms);
        }


        // -------------------- [ GET: Chi tiết phòng ] --------------------
        public ActionResult Details(int id = -1)
        {
            //, DateTime checkin, DateTime checkout
            var room = db.Rooms.FirstOrDefault(r => r.id == id);

            if (room == null)
            {
                return HttpNotFound();
            }

            var resort = db.Resorts.FirstOrDefault(r => r.id == room.resort_id);
            ViewBag.Resort = resort;

            return View(room);
        }
    }
}