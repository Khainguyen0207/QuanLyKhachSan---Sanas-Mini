using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    public class BaseController : Controller
    {

        private QuanLyKhachSanDataContext db;

        public BaseController()
        {
            string conn = ConfigurationManager.ConnectionStrings["QLKSConnectionString"].ConnectionString;
            db = new QuanLyKhachSanDataContext(conn);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var token = Request.Cookies.Get("remember_token");

            if (Session["User"] == null && token != null)
            {
                Customer customer = db.Customers.FirstOrDefault(m => m.remember_token == token.Value);

                if (customer != null)
                {
                    Session["User"] = customer;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
