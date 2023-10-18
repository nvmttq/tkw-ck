using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    public class ProductManagermentController : Controller
    {
        private WebBanHangEntities db = new WebBanHangEntities();
        // GET: Admin/ProductManagerment
        public ActionResult Index()
        {
            return View();
        }
    }
}