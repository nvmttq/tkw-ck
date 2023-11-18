using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHang.Models
{
    public class Checkout
    {
        public List<Coupon> coupons { get; set; }
        public ProductCart pc { get; set; }
        public double total { get; set; }
        public double discount { get; set; }
        public void calc_total()
        {
            ProductCart pc = this.pc;
            List<Coupon> coupons = this.coupons;
            double sum = 0, discount = 0;
            for(int i = 0; i < pc.product.Count(); i++)
            {

                if(coupons != null && coupons.Count> 0)
                {
                    var exists = false;
                    foreach(Coupon cp in coupons)
                    {
                        if (cp != null && cp.ProductId == pc.product[i].Id) exists = true;
                    }
                    if(exists == true)
                    {
                        var coupon1 = coupons.Find(cp => cp != null && cp.ProductId == pc.product[i].Id);
                        double calc_dis = ((double)coupon1.Discount * (double)pc.product[i].Price) / 100.0;
                        discount += (calc_dis * (double)pc.cart[i].Quantity);
                    }
                }
                sum += ((double)pc.product[i].Price * (double)pc.cart[i].Quantity);

            }
            this.discount = discount;
            this.total = sum;
        }
    }
}