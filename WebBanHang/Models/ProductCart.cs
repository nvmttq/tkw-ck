using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHang.Models
{

    public class ProductCart
    {

        public List<Cart> cart { get; set; }
        public List<Product> product { get; set; }
        public double total_price { get; set; }
        public double totalPriceDisCount { get; set; }
        public double totalDiscount { get; set; }

        public double CaclcPriceDiscount(double price, double discount)
        {
            discount = Math.Max(1, discount);
            return price - (price * ((int)discount / 100));
        }
        public void GetTotalPrice()
        {
            ProductCart pc = this;
            double total_price = 0, totalPriceDiscount = 0, totalDiscount = 0;
            for(int i = 0; i < pc.product.Count(); i++)
            {
                Cart cart = pc.cart[i];
                Product product = pc.product[i];
                total_price += (double)product.Price * (double)cart.Quantity;
                totalPriceDiscount += CaclcPriceDiscount((double)product.Price, (double)product.Discount) * (double)cart.Quantity;

            }
            totalDiscount = total_price - totalPriceDiscount;
            this.total_price = total_price;
            this.totalPriceDisCount = totalPriceDiscount;
            this.totalDiscount = totalDiscount;
        }

        public static ProductCart GetProductCart(string username, List<Cart> carts, List<Product> products)
        {
            var productCart = (from p in products
                               join crt in carts
                               on p.Id equals crt.ProductId
                               where username == crt.UserId
                               select new
                               {
                                   cart = crt,
                                   product = p

                               });
            List<Product> lp = new List<Product>();
            List<Cart> lc = new List<Cart>();
            foreach (var item in productCart)
            {
                lp.Add(item.product);
                lc.Add(item.cart);
            }
            var pc = new ProductCart() { cart = lc, product = lp };
            pc.GetTotalPrice();
            return pc;
        }
    }
}