using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            product.categories = (from c in db.Categories
                                  join pc in db.ProductCategories on c.Id equals pc.CategoryId
                                  where pc.ProductId == product.Id
                                  select c).ToList();
            product.CategorySelections = db.Categories.Select(x => new CategorySelections
            {
                Name = x.Name,
                Id = x.Id,
                IsSelected = (from pc in db.ProductCategories
                              where pc.CategoryId == x.Id && pc.ProductId == productId
                              select pc).ToList().Count() > 0
            }).ToList();
            return View(product);
        }

        [HttpPost]
        public ActionResult Edit([Bind(Include = "Id,Name,Price,Discount,Image,Quantity,Description,CategorySelections")] Product product)
        {
            var proCat = (from pc in db.ProductCategories
                          join c in db.Categories on pc.CategoryId equals c.Id
                          where pc.ProductId == product.Id
                          select pc);

            db.ProductCategories.RemoveRange(proCat);

            foreach(var c in product.CategorySelections)
            {
                if (c.IsSelected == false) continue;
                ProductCategory pc = new ProductCategory();
                pc.ProductId = product.Id;
                pc.CategoryId = c.Id;

                db.ProductCategories.Add(pc);
            }
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            

            return RedirectToAction("Index");
        }
    }
}