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
        public ActionResult Index(int? isSelectedBeforeCheckout)
        {
            Session["ProductSelected"] = null;
            if (isSelectedBeforeCheckout != null && isSelectedBeforeCheckout == 1)
            {
                ViewBag.isSelectedBeforeCheckout = 1;
            }
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
            string url = Request.Url.ToString();
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

                ProductCart pc = (ProductCart)Session["ProductSelected"];

                if (pc != null)
                {

                    int id = 0;
                    foreach (Cart c in pc.cart)
                    {
                        if (c.ProductId == productId)
                        {
                            pc.cart[id].Quantity = cart.Quantity;
                            break;
                        }
                        id++;
                    }
                    Session["ProductSelected"] = pc;
                }
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

        [HttpPost]
        public JsonResult SelectProductCheckout(Product p, Cart c, string type)
        {
            ProductCart pc = (ProductCart)Session["ProductSelected"];
            if(pc == null)
            {
                pc = new ProductCart();
                pc.product = new List<Product>();
                pc.cart = new List<Cart>();
            }
            
            if(type == "ADD")
            {
                if(pc.product.Any(product => product.Id == p.Id))
                {
                    int id = 0;
                    foreach (Product val in pc.product)
                    {
                        if (val.Id == p.Id)
                        {
                            pc.product.RemoveAt(id);
                            break;
                        }
                        id++;
                    }
                    id = 0;
                    foreach (Cart val in pc.cart)
                    {
                        if (val.Id == c.Id)
                        {
                            pc.cart.RemoveAt(id);
                            break;
                        }
                        id++;
                    }
                }
                pc.product.Add(p);
                pc.cart.Add(c);

                pc.GetTotalPrice();
            } else
            {
                int id = 0;
                foreach(Product val in pc.product)
                {
                    if(val.Id == p.Id)
                    {
                        pc.product.RemoveAt(id);
                        break;
                    }
                    id++;
                }

                id = 0;
                foreach (Cart val in pc.cart)
                {
                    if (val.Id == c.Id)
                    {
                        pc.cart.RemoveAt(id);
                        break;
                    }
                    id++;
                }
                pc.GetTotalPrice();
            }

            Session["ProductSelected"] = pc;
            return Json( new { code= 200, pc=pc});
        }
    }
}