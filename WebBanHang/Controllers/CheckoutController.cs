using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class CheckoutController : Controller
    {
        WebBanHangEntities db = new WebBanHangEntities();

        public double CalcTotalPrice(int total, int discount, int ship)
        {

            return (total - (total * (discount / 100)) + ship);
        }

        public double CalcProductPrice(double price, int discount)
        {
            return price - (price * ((double)discount / 100.0));
        }
        // GET: Checkout
        public ActionResult Index()
        {
            ViewBag.TotalPriceDiscount = 0;
            ViewBag.TotalPrice = 0;
            var user = (User)Session["user"];
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var carts = db.Carts.ToList();
            var products = db.Products.ToList();
            ProductCart pc = new ProductCart();
            var productCart = (from p in db.Products
                               join crt in db.Carts
                               on p.Id equals crt.ProductId
                               where user.Username == crt.UserId
                               select new
                               {
                                   cart = crt,
                                   product = p

                               }).ToList();
            List<Product> lp = new List<Product>();
            List<Cart> lc = new List<Cart>();
            foreach (var item in productCart)
            {
                ViewBag.TotalPriceDiscount += CalcProductPrice((double)item.product.Price, (int)item.product.Discount) * item.cart.Quantity;
                ViewBag.TotalPrice += ((double)item.product.Price * (double)item.cart.Quantity);
                lp.Add(item.product);
                lc.Add(item.cart);
            }
            
            ViewBag.Discount = ViewBag.TotalPrice - ViewBag.TotalPriceDiscount;
            pc.product = lp;
            pc.cart = lc;
            return View(pc);
        }


    }
}