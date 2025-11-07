using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyKhachSan.Helpers
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 🔒 Kiểm tra đăng nhập
            if (! AuthorizeHelper.IsLoggedIn())
            {
                // Nếu chưa đăng nhập -> quay về trang Login
                filterContext.Result = new RedirectResult("~/Account/Login");

                return;
            }

            // 🔒 Kiểm tra quyền admin
            if (! AuthorizeHelper.IsAdmin())
            {
                // Nếu không phải admin -> chuyển về trang chủ (hoặc báo lỗi)
                filterContext.Result = new RedirectResult("~/HomePage/Index");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}