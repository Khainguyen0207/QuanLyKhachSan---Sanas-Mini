using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyKhachSan.Models
{
    public class ResortCardVM
    {
        public Resort Resort { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public string FirstImage { get; set; }
        public double AvgRate { get; set; }
        public string LatestComment { get; set; }
    }
}