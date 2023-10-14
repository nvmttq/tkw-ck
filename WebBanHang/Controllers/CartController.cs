using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    
    public class CartController : Controller
    {
        WebBanHangEntities db = new WebBanHangEntities();

        public double CalcTotalPrice(int total, int discount, int ship)
        {

            return (total - (total*(discount/100)) + ship);
        }

        public double CalcProductPrice(double price, int discount)
        {
            return price - (price * ((double)discount / 100.0));
        }

        public List<Cart> GetProductCart(User user)
        {
           
            var cart = (from crt in db.Carts
                        where crt.UserId == user.Username
                        select crt).ToList();
            return (List<Cart>)cart;
        }
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
                                   
                               }).ToList() ;
            List<Product> lp = new List<Product>();
            List<Cart> lc = new List<Cart>();
            foreach(var item in productCart)
            {
                ViewBag.TotalPriceDiscount += CalcProductPrice((double)item.product.Price, item.product.Discount) * item.cart.Quantity;
                ViewBag.TotalPrice += ((double)item.product.Price * (double)item.cart.Quantity);
                lp.Add(item.product);
                lc.Add(item.cart);
            }

            ViewBag.Discount = ViewBag.TotalPrice - ViewBag.TotalPriceDiscount;
            pc.product = lp;
            pc.cart = lc;
            Session["carts"] = pc;
            return View(pc);
        }
      


        public ActionResult AddToCart(int ProductId, string returnUrl)
        {
            var user = (User)Session["user"];
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var findCart = (from c in db.Carts
                        where c.ProductId == ProductId
                            select c).FirstOrDefault();
            if(findCart != null)
            {
                findCart.Quantity += 1;
            } else
            {
                Cart cart = new Cart();
                cart.ProductId = ProductId;
                cart.UserId = user.Username;
                cart.Quantity = 1;
                cart.CreatedAt = DateTime.Now;
                cart.UpdatedAt = DateTime.Now;
                db.Carts.Add(cart);
            }
            db.SaveChanges();

            return Redirect(returnUrl);
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