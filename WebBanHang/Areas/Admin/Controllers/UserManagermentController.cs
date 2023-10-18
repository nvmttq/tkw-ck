using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    public class UserManagermentController : Controller
    {
        private WebBanHangEntities db = new WebBanHangEntities();
        // GET: Admin/UserManagerment
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }


        public ActionResult Add()
        {
            var data = from p in db.Products
                       select new
                       {
                           productId = p.Id,
                           productName = p.Name,
                       };
            return View(data.ToList());
        }
    }
}