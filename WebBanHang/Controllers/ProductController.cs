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
            foreach (Product p in product)
            {

                List<Category> categories = (from c in db.Categories
                                             join pc in db.ProductCategories on c.Id equals pc.CategoryId
                                             where pc.ProductId == p.Id
                                             select c).ToList();
                p.categories = categories;
            }
            return View(product.Single());
        }

        
    }
}