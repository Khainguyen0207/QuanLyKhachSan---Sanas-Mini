using QuanLyKhachSan.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyKhachSan.Controllers
{
    public class HomePageController : BaseController
    {
        QuanLyKhachSanDataContext db;
        public HomePageController()
        {
            string conn = ConfigurationManager.ConnectionStrings["QLKSConnectionString"].ConnectionString;
            db = new QuanLyKhachSanDataContext(conn);
        }
        public ActionResult Index(string search = "", string sort = "", string amenity = "", int page = 1)
        {
            int pageSize = 6;

            var model = db.Resorts
                .Where(r => db.Rooms.Any(p => p.resort_id == r.id && p.quantity > 0))
                .ToList()
                .Select(r =>
                {
                    var prices = db.Rooms
                                   .Where(p => p.resort_id == r.id && p.quantity > 0)
                                   .Select(p => (decimal?)p.price);

                    decimal min = prices.Any() ? prices.Min() ?? 0 : 0;
                    decimal max = prices.Any() ? prices.Max() ?? 0 : 0;

                    string firstImage = GetFirstImage(r.images);

                    // 🔹 Tính trung bình sao
                    var avgRate = db.Feedbacks.Where(f => f.resort_id == r.id).Average(f => (double?)f.rate) ?? 0;

                    // 🔹 Lấy comment gần nhất
                    var latestComment = db.Feedbacks
                        .Where(f => f.resort_id == r.id)
                        .OrderByDescending(f => f.created_at)
                        .Select(f => f.comment)
                        .FirstOrDefault();

                    return new ResortCardVM
                    {
                        Resort = r,
                        MinPrice = min,
                        MaxPrice = max,
                        FirstImage = firstImage,
                        AvgRate = avgRate,
                        LatestComment = latestComment
                    };
                })
                .ToList();

            // 🔍 Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                model = model.Where(m =>
                    m.Resort.name.Contains(search) ||
                    m.Resort.address.Contains(search)
                ).ToList();
            }

            // 🧩 Lọc theo tiện nghi
            if (!string.IsNullOrEmpty(amenity))
            {
                model = model
                    .Where(m => m.Resort.general_amenities != null &&
                                m.Resort.general_amenities.Contains(amenity))
                    .ToList();
            }

            // ↕️ Sắp xếp
            switch (sort)
            {
                case "price_asc":
                    model = model.OrderBy(m => m.MinPrice).ToList();
                    break;
                case "price_desc":
                    model = model.OrderByDescending(m => m.MaxPrice).ToList();
                    break;
                case "name_asc":
                    model = model.OrderBy(m => m.Resort.name).ToList();
                    break;
                case "name_desc":
                    model = model.OrderByDescending(m => m.Resort.name).ToList();
                    break;
                default:
                    model = model.OrderBy(m => m.Resort.id).ToList();
                    break;
            }

            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.Amenity = amenity;

            return View(model);
        }

        private string GetFirstImage(string images)
        {
            if (string.IsNullOrEmpty(images)) return "default.jpg";
            var clean = images.Replace("[", "").Replace("]", "").Replace("\"", "");
            return clean.Split(',').Select(x => x.Trim()).FirstOrDefault() ?? "default.jpg";
        }

    }
}