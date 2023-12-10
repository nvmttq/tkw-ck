using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;
using WebBanHangOnline.Models.Payments;

namespace WebBanHang.Controllers
{
    public class OrderController : Controller
    {
        private WebBanHangEntities db = new WebBanHangEntities();      
        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        public double CalcProductPrice(double price, int discount)
        {
            return price - (price * ((double)discount / 100.0));
        }


        [HttpPost]
        public ActionResult SetOrder(FormCollection f)
        {

            string fullname = f["fullname-shipping"];
            string phone = f["phone-shipping"];
            string address = f["address-shipping"];
            string city = f["city-shipping"];
            string note = f["note-shipping"];
            string email = f["email-shipping"];
            int TypePaymentVN = int.Parse((f["TypePaymentVN"] == null ? "500" : f["TypePaymentVN"]));

            var user = (User)Session["user"];

            var carts = db.Carts.ToList();
            var products = db.Products.ToList();
            ProductCart pc = (ProductCart)Session["ProductSelected"];
            var productCart = pc;

            ViewBag.TotalPriceDiscount = 0;
            ViewBag.TotalPrice = 0;
            var url = "";
            try
            {
                var id = 0;
                foreach (var item in pc.product)
                {
                    double a = CalcProductPrice((double)item.Price, (int)item.Discount) * (double)pc.cart[id].Quantity;
                    ViewBag.TotalPriceDiscount += a;

                    ViewBag.TotalPrice += ((double)item.Price * (double)pc.cart[id].Quantity);
                    id++;
                }
                ViewBag.Discount = ViewBag.TotalPrice - ViewBag.TotalPriceDiscount;
                double tot_price = ViewBag.TotalPriceDiscount;
                Models.Order order = new Models.Order();
                order.UserId = user.Username;
                order.Address = address;
                order.Phone = phone;
                order.Note = (String.IsNullOrEmpty(note) ? "Không có" : note);
                order.FullName = fullname;
                order.OrderDate = DateTime.Now;
                order.TotalPrice = (decimal)ViewBag.TotalPrice;
                order.Discount = (decimal)ViewBag.Discount;

                db.Orders.Add(order);
                db.SaveChanges();

                id = 0;
                foreach (var item in pc.product)
                {
                    OrderDetail orderDetail = new OrderDetail();
                    orderDetail.OrderId = order.Id;
                    orderDetail.ProductId = item.Id;
                    orderDetail.Price = (decimal)CalcProductPrice((double)item.Price, (int)item.Discount) * pc.cart[id].Quantity;
                    orderDetail.Quantity = pc.cart[id].Quantity;
                    db.OrderDetails.Add(orderDetail);
                    db.SaveChanges();
                    id++;
                    // code tiep o day
                }
                ViewBag.Discount = ViewBag.TotalPrice - ViewBag.TotalPriceDiscount;

                db.Carts.RemoveRange(db.Carts.ToList());
                db.SaveChanges();

                try
                {
                     url = UrlPayment(TypePaymentVN, order.Id.ToString(), tot_price);
                     if(String.IsNullOrEmpty(url))
                        {
                            return View("COD_Return");
                        }

                    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/sendOrtherSuccess.html"));

                    //send mail cho khach hang 
                    var strSanPham = "";
                    var thanhtien = decimal.Zero;
                    var TongTien = decimal.Zero;
                    var oderDetails = (from od in db.OrderDetails
                                       where od.OrderId == order.Id
                                       select od);
                    foreach (var od in oderDetails)
                    {
                        Product product = (from p in db.Products
                                           where p.Id == od.ProductId
                                           select p).SingleOrDefault();
                        od.p = product;
                    }
                    foreach (var sp in oderDetails)
                    {
                        if (sp.p == null) continue;
                        strSanPham += "<tr>";
                        strSanPham += "<td>" + sp.p.Name + "</td>";
                        strSanPham += "<td>" + sp.Quantity + "</td>";
                        strSanPham += "<td>" + WebBanHang.Common.Common.FormatNumber(sp.p.Price * sp.Quantity, 0) + "</td>";
                        strSanPham += "</tr>";
                        thanhtien += ((decimal)sp.p.Price * (decimal)sp.Quantity);
                    }
                    TongTien = thanhtien;


                    contentCustomer = contentCustomer.Replace("{{MaDon}}", "DH" + order.Id.ToString());
                    contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                    contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                    contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.FullName);
                    contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
                    contentCustomer = contentCustomer.Replace("{{Email}}", email); // order.Email
                    contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);

                    contentCustomer = contentCustomer.Replace("{{ThanhTien}}", WebBanHang.Common.Common.FormatNumber(thanhtien, 0));

                    contentCustomer = contentCustomer.Replace("{{TongTien}}", WebBanHang.Common.Common.FormatNumber(TongTien, 0));

                    WebBanHang.Common.Common.SendMail("ShopOnline", "Đơn Hàng #" + order.Id, contentCustomer.ToString(), email);

                } catch(Exception ex)
                {
                    return View("FailureView");
                }
                
            } catch(Exception ex)
            {
                return View("FailureView");

            }
            if (String.IsNullOrEmpty(url))
            {
                return View("COD_Return");
            }
            return Redirect(url);


        }

        public ActionResult FailureView()
        {
            return View();
        }
        #region Thanh toán vnpay
        public string UrlPayment(int TypePaymentVN, string orderCode, double total)
        {
            var urlPayment = "";
            var order = db.Orders.FirstOrDefault(x => x.Id.ToString() == orderCode);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();
            var Price = total * 100;
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", Price.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000  
            if (TypePaymentVN == 1)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (TypePaymentVN == 2)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (TypePaymentVN == 3)
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            } else if(TypePaymentVN == 500)
            {
                return null;
            }

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.Id.ToString());
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString()); 

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
            return urlPayment;
        }
        #endregion


        public ActionResult COD_Return()
        {
            return View();
        }
        public ActionResult VnpayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var itemOrder = db.Orders.FirstOrDefault(x => x.Id.ToString() == orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.Status = false;//đã thanh toán
                            db.Orders.Attach(itemOrder);
                            db.Entry(itemOrder).State =         System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //Thanh toan thanh cong
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                        
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                        //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}", orderId, vnpayTranId, vnp_ResponseCode);
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();
                    ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND):" + vnp_Amount.ToString();
                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
            }
            //var a = UrlPayment(0, "DH3574");
            return View();
        }
    }
}