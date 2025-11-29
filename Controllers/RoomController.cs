using QuanLyKhachSan.Models;
using QuanLyKhachSan.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace QuanLyKhachSan.Controllers
{
    public class RoomController : BaseController
    {
        private const int PerPage = 12;

        public RoomController() {}

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
                    int from, to;
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

            // -------------------- [ Lọc theo khoảng còn phòng ] --------------------
            if (checkin != null && checkout != null)
            {
               
                rooms = rooms.Where(m => m.quantity > 
                    db.Bookings
                       .Where(b => b.room_id == m.id)
                       .Where(b => b.check_in < checkout && b.check_out > checkin)
                       .Count()
                );
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
        public ActionResult Details(int id = -1, DateTime from = default(DateTime), DateTime to = default(DateTime))
        {
            if (from == default || from < DateTime.Now)
            {
                from = DateTime.Now.AddDays(1);
            }

            if (to == default || to < from)
            {
                to = from.AddDays(1);
            }

            int CountConflicts = db.Bookings
                .Where(b => b.room_id == id)
                .Where(b => b.check_in < to && b.check_out > from)
                .Count();

            var DataRoom = db.Rooms.AsQueryable();

            var room = DataRoom.FirstOrDefault(r => r.id == id);

            room.quantity -= CountConflicts;

            if (room == null)
            {
                return HttpNotFound();
            }

            var resort = db.Resorts.FirstOrDefault(r => r.id == room.resort_id);

            var rooms = db.Rooms.Where(m => m.resort_id == resort.id)
                .Take(6)
                .Select(r => new RoomViewModel
            {
                Room = r,
                address = resort.address
            }).ToList();

            ViewBag.Resort = resort;
            ViewBag.Rooms = rooms;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.CheckIn = from.ToString("yyyy-MM-dd");
            ViewBag.CheckOut = to.ToString("yyyy-MM-dd");

            return View(room);
        }
    }
}