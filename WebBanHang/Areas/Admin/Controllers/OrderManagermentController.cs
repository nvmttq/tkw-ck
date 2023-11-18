using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;
namespace WebBanHang.Areas.Admin.Controllers
{
    public class OrderManagermentController : Controller
    {
        private WebBanHangEntities db = new WebBanHangEntities();
        // GET: Admin/OrderManagerment
        public ActionResult Index()
        {
            return View(db.Orders.ToList());
        }

        public ActionResult Details(int orderId)
        {
            var orderDetails = (from od in db.OrderDetails
                                where od.OrderId == orderId
                                select od).ToList();
            foreach(var od in orderDetails)
            {
                Product product = (from p in db.Products
                               where p.Id == od.ProductId
                               select p).FirstOrDefault();
                od.p = product;
            }

            return View(orderDetails);
        }
    }
}