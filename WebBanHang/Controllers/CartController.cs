using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    
    public class CartController : Controller
    {
        WebBanHangEntities db = new WebBanHangEntities();

    

     

       

        // GET: Cart
        public ActionResult Index()
        {
           
            ViewBag.TotalPriceDiscount = 0;
            ViewBag.TotalPrice = 0;
            var user = (User)Session["user"];
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ProductCart pc = ProductCart.GetProductCart(user.Username, db.Carts.ToList(), db.Products.ToList());
            Session["cart"] = pc;
            return View(pc);
        }


        [HttpPost]
        public JsonResult AddToCart(int productId, int quantities)
        {
            var user = (User)Session["user"];
            string returnUrl = ConfigurationManager.AppSettings["host_port"] + "/";

            if (user == null)
            {
                return Json(new { code = 500, msg = "Vui lòng đăng nhập trước khi thêm vào giỏ hàng", returnUrl = returnUrl });
            }

            var findCart = (from c in db.Carts
                            where c.ProductId == productId
                            select c).FirstOrDefault();
            if (findCart != null)
            {
                findCart.Quantity += 1;
            } else
            {
                Cart cart = new Cart();
                cart.ProductId = productId;
                cart.UserId = user.Username;
                cart.Quantity = 1;
                cart.CreatedAt = DateTime.Now;
                cart.UpdatedAt = DateTime.Now;
                db.Carts.Add(cart);
            }
            db.SaveChanges();

            var pc = ProductCart.GetProductCart(user.Username, db.Carts.ToList(), db.Products.ToList());
            Session["cart"] = pc;
            return Json(new { code = 200, msg = "Thêm vào giỏ hàng thành công", quantities= pc.product.Count()});
        }

        public void UpdateQuantity(int? productId, int quantities = 0)
        {
            var user = (User)Session["user"];
            
            if(productId != null)
            {
                var cart = (from p in db.Products
                               join c in db.Carts on p.Id equals c.ProductId
                               where p.Id == productId && c.UserId == user.Username
                               select c).SingleOrDefault();

                cart.Quantity = quantities;
                cart.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }
        }

        public void RemoveProduct(int? productId)
        {
            var user = (User)Session["user"];

            if (productId != null)
            {
                var cart = (from p in db.Products
                            join c in db.Carts on p.Id equals c.ProductId
                            where p.Id == productId && c.UserId == user.Username
                            select c).SingleOrDefault();

                db.Carts.Remove(cart);
                db.SaveChanges();
            }
        }
    }
}