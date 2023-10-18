using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    [RouteArea("Admin/Dashboard")]
    public class DashboardController : Controller
    {

        private WebBanHangEntities db = new WebBanHangEntities();
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }

    }
}