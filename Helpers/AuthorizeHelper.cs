using QuanLyKhachSan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyKhachSan.Helpers
{
    public static class AuthorizeHelper
    {
        public static Customer CurrentUser()
        {
            return HttpContext.Current.Session["User"] as Customer;
        }

        public static bool IsLoggedIn()
        {
            return CurrentUser() != null;
        }

        public static bool IsAdmin()
        {
            var u = CurrentUser();
            return u != null && (u.is_admin ?? false);
        }

        public static bool IsPartner()
        {
            var u = CurrentUser();
            return u != null && (u.is_partner ?? false);
        }
    }
}