using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    public class ProductManagermentController : Controller
    {
        private WebBanHangEntities db = new WebBanHangEntities();
        // GET: Admin/ProductManagerment
        public ActionResult Index()
        {
            return View(db.Products.ToList());
        }

        public ActionResult Create()
        {
           
            return View(db.Categories.ToList());
        }

        [HttpPost]
        public ActionResult Create(FormCollection f)
        {
            string Name = f["ProductName"];
            double Price = double.Parse(f["ProductPrice"]);
            int Category = int.Parse(f["ProductCategories"]);
            int Quantity = int.Parse(f["ProductQuantity"]);
            string Description = f["ProductDescription"];

            Product p = new Product();
            p.Name = Name;
            p.Price = (decimal)Price;
            p.Quantity = Quantity;
            p.Description = Description;

            db.Products.Add(p);
            db.SaveChanges();

           
            ProductCategory pc = new ProductCategory();
            pc.ProductId = p.Id;
            pc.CategoryId = Category;
            db.ProductCategories.Add(pc);
            db.SaveChanges();


            return RedirectToAction("Index");
        }
   
        
        public ActionResult Delete(int productId)
        {
            var product = (from p in db.Products
                           where p.Id == productId
                           select p).FirstOrDefault();
            db.Products.Remove(product);
            db.SaveChanges();


            return RedirectToAction("Index");
        } 

        public ActionResult Edit(int productId)
        {
            var product = (from p in db.Products
                           where p.Id == productId
                           select p).FirstOrDefault();
            var productCategory = (from pc in db.ProductCategories
                                   where pc.ProductId == productId
                                   select pc).FirstOrDefault();
            productCategory.categories = db.Categories.ToList();
            productCategory.product = product;
            return View(productCategory);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection f, int ?productId)
        {
            string Name = f["ProductName"];
            double Price = double.Parse(f["ProductPrice"]);
            int Category = int.Parse(f["ProductCategories"]);
            int Quantity = int.Parse(f["ProductQuantity"]);
            string Description = f["ProductDescription"];

            Product product = (from p in db.Products
                           where p.Id == productId
                           select p).FirstOrDefault();
            product.Name = Name;
            product.Price = (decimal)Price;
            product.Quantity = Quantity;
            product.Description = Description;

            var pc = (from pcc in db.ProductCategories
                      where pcc.ProductId == productId
                      select pcc).FirstOrDefault();
            db.ProductCategories.Remove(pc);

            ProductCategory record_pc = new ProductCategory();
            record_pc.CategoryId = Category;
            record_pc.ProductId = (int)productId;

            db.SaveChanges();
            
            return RedirectToAction("Index");
        }
    }
}