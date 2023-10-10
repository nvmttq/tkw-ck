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
    }
}