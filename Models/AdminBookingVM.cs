using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyKhachSan.Models
{
    public class AdminBookingVM
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ResortName { get; set; }
        public string RoomName { get; set; }
        public System.DateTime? CheckIn { get; set; }
        public System.DateTime? CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public System.DateTime? CreatedAt { get; set; }
    }
}