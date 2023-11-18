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

        [HttpPost]
        public PartialViewResult AddCoupon(string code, Checkout ck, List<Coupon> cps)
        {
            ProductCart pc = ck.pc;

            List<Product> pros = pc.product.ToList();
        
            var coupon = (from cp in db.Coupons
                          //join p in pc.product on cp.ProductId equals p.Id // wrong pc product
                          where cp.Code == code
                          select cp).SingleOrDefault();
            Coupon ret_coupon = null;
            foreach(Product p in pros)
            {
                if(coupon != null && p.Id == coupon.ProductId)
                {
                    ret_coupon = coupon;
                }
            }
            if(ret_coupon == null)
            {
                ViewBag.ThongBao = "Bạn không có sản phẩm tương thích với mã giảm giá này";
                return PartialView("_CheckoutPartial", ck);
                //return Json( new { msg="Bạn không có sản phẩm tương thích với mã giảm giá này"}, JsonRequestBehavior.AllowGet);
            }
            var product = (from p in db.Products
                           where p.Id == coupon.ProductId
                           select p).SingleOrDefault();

            coupon.p = product;
            ck.coupons.Add(coupon);
            ck.calc_total();
            ViewBag.ThongBao = null;
            return PartialView("_CheckoutPartial", ck);
            //return Json(new { msg = "Tuyet voi", data=ck}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public PartialViewResult RemoveCoupon(string code, Checkout ck)
        {
            ProductCart pc = ck.pc;

            List<Product> pros = pc.product.ToList();
            List<Coupon> coupons = ck.coupons;

            for(int i = 0; i < coupons.Count(); i++)
            {
                if (coupons[i] == null) continue;
                if(coupons[i].Code == code)
                {
                    coupons.RemoveAt(i);
                    break;
                }
            }
            ck.coupons = coupons;
            ck.calc_total();
            return PartialView("_CheckoutPartial", ck);
            //return Json(new { msg = "Tuyet voi", data=ck}, JsonRequestBehavior.AllowGet);
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
            Checkout ck = new Checkout();
            ck.pc = (ProductCart)Session["ProductSelected"];


            if (ck.pc == null)
            {
                return RedirectToAction("Index", "Cart", new { isSelectedBeforeCheckout = 1 });
            }

            ViewBag.isSelectedBeforeCheckout = null;
            var productCart = (from p in db.Products
                               join crt in db.Carts
                               on p.Id equals crt.ProductId
                               where user.Username == crt.UserId
                               select new
                               {
                                   cart = crt,
                                   product = p

                               }).ToList();
            Coupon cp = new Coupon();
            cp.Code = "???";
            cp.Id = 0;
            cp.Discount = 0;
            cp.ProductId = 0;
            ck.coupons = new List<Coupon>();
            ck.coupons.Add(cp);
            ck.calc_total();

            return View(ck);
        }


    }
}