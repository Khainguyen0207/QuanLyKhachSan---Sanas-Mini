using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyKhachSan.Requests
{
    public class ResortFilterRequest
    {
        public int? ResortId { get; set; }
        public string Search { get; set; }
        public string Sort { get; set; }
        public int Page { get; set; } = 1;
        public string PriceRange { get; set; }
        public int? Adults { get; set; }
        public int? Children { get; set; }
        public int? Quantity { get; set; }
        public DateTime? CheckinDate { get; set; }
        public DateTime? CheckoutDate { get; set; }
    }

}