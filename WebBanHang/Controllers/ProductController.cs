using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        private WebBanHangEntities db = new WebBanHangEntities();
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int ProductId)
        {
            var product = from p in db.Products
                          where p.Id == ProductId
                          select p;
            return View(product.Single());
        }

        
    }
}