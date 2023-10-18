using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        const int recordPerPage = 10;
        private WebBanHangEntities db = new WebBanHangEntities();
        public ActionResult Index()
        {
            var user = Session["user"] as User;
            if(user != null)
            {
                ProductCart pc = ProductCart.GetProductCart(user.Username, db.Carts.ToList(), db.Products.ToList());
                Session["cart"] = pc;
            }
            return View();
        }

        public bool CheckContainsKeyword(string name, string keyword)
        {
            return (name.ToUpper()).Contains(keyword.ToUpper()) == true;
        }

     
        public ActionResult Search(int? page, string categoryId = null,  string keyword = null, string sortby = null, string order = null, string fromPrice = null, string toPrice = null)
        {
            var isAjax = Request.IsAjaxRequest();
            ViewBag.url = Request.Url.ToString();
            if (isAjax == false) return RedirectToAction("Index");
            IQueryable<Product> model = db.Products;

            var CategoryIdParams = Request.QueryString["CategoryId"];
            var KeywordParams = Request.QueryString["keyword"];

            if (!String.IsNullOrEmpty(categoryId))
            {
                var split = categoryId.Split(',').ToList();
                model = from p in model
                        join pc in db.ProductCategories on p.Id equals pc.ProductId
                        join c in db.Categories on pc.CategoryId equals c.Id
                        where split.Contains(c.Id.ToString()) == true
                        select p;
            }

            if (!String.IsNullOrEmpty(keyword))
            {
                model = from p in model
                        where p.Name.ToString().ToUpper().Contains(keyword.ToUpper())
                        select p;
            }


            if (!String.IsNullOrEmpty(sortby))
            {
                order = order.ToUpper();
                sortby = sortby.ToUpper();
                if (order == "DESC" || order == "DESCENDING")
                {
                    if (sortby == "NAME") model = model.OrderByDescending(p => p.Name.ToUpper());
                    if (sortby == "PRICE") model = model.OrderByDescending(p => p.Price);
                }
                else
                {
                    if (sortby == "NAME") model = model.OrderBy(p => p.Name.ToUpper());
                    if (sortby == "PRICE") model = model.OrderBy(p => p.Price);
                }
            }


            if (!String.IsNullOrEmpty(fromPrice))
            {
                double from = double.Parse(fromPrice);
                double to = double.Parse(toPrice);
                model = model.Where(p => (double)p.Price >= from && (double)p.Price <= to);
            }
            //IQueryable<Product> products = db.Products;
            //if (!String.IsNullOrEmpty(keyword)) products = products.Where(p => p.Name.ToString().ToUpper().Contains(keyword.ToUpper()));


            var tmp = model;
            var pagesize = tmp.ToList().Count();
            ViewBag.TotalPage = pagesize / 10; if (pagesize % 10 != 0) ViewBag.TotalPage++;
            ViewBag.ActivePage = (page ?? 1);

            return PartialView("_ProductsPartial", model.ToList().Distinct().Skip(((page ?? 1) - 1) * recordPerPage).Take(recordPerPage).ToList());
        }


        public ActionResult _ProductsPartial(int? CategoryId, int page)
        {

            ViewBag.ActivePage = 1;
            if (CategoryId == null)
            {
                var tmp1 = db.Products;
                var pagesize1 = tmp1.ToList().Count();
                ViewBag.ActiveCategoryId = CategoryId;
                ViewBag.TotalPage = pagesize1 / 10;
                if (pagesize1 % 10 != 0) ViewBag.TotalPage++;
                return PartialView(db.Products.Take(10).ToList());
            }
            var defaultListProduct = (from p in db.Products
                                      join pc in db.ProductCategories on p.Id equals pc.ProductId
                                      join c in db.Categories on pc.CategoryId equals c.Id
                                      where c.Id == CategoryId
                                      select p).ToList().Distinct();
        
            var tmp = defaultListProduct;
            var pagesize = tmp.ToList().Count();
            ViewBag.ActiveCategoryId = CategoryId;
            ViewBag.TotalPage = pagesize / 10;
            if (pagesize % 10 != 0) ViewBag.TotalPage++;

            return PartialView("_ProductsPartial", (defaultListProduct.OrderBy(p=>p.Name).Skip((page - 1) * 10).Take(10)));
        }
          
        public ActionResult _FilterProductPartial()
        {
            var queryCategoryId = Request.QueryString["CategoryId"];
            if(queryCategoryId == null)
            {
                List<StoreFilterModel> a = new List<StoreFilterModel>();
                var result = (from c in db.Categories
                              select new
                              {
                                  c = c,
                                  isCheck = false
                              });
                foreach(var item in result.ToList())
                {
                    StoreFilterModel sf = new StoreFilterModel();
                    sf.c = item.c;
                    sf.isCheck = item.isCheck;
                    a.Add(sf);
                }
                return PartialView(a);
            }
            var querySplit = queryCategoryId.Split(',').Distinct();
            var resultFilter = (from c in db.Categories
                                      select new
                                      {
                                          c = c,
                                          isCheck = (querySplit.Contains(c.Id.ToString()) == true ? true : false) 
                                      });
            List<StoreFilterModel> b = new List<StoreFilterModel>();
            foreach (var item in resultFilter.ToList())
            {
                StoreFilterModel sf = new StoreFilterModel();
                sf.c = item.c;
                sf.isCheck = item.isCheck;
                b.Add(sf);
            }
            return PartialView(b);
        }

     
 
        [HttpPost]
        public ActionResult FilterProduct(FormCollection f)
        {
            List<string> filters = new List<string>();
            foreach(var filter in f.AllKeys)
            {
                filters.Add(filter.ToUpper());
            }

            var products = from p in db.Products
                    join pc in db.ProductCategories
                    on p.Id equals pc.ProductId
                    join c in db.Categories
                    on pc.CategoryId equals c.Id
                    where filters.Any(filter => c.Name.ToString().ToUpper() == filter) == true
                    select p;

            return View("Index", products.ToList());
        }


        public ActionResult _LoadProductPartial(int CategoryId) 
        {
            var defaultListProduct = from p in db.Products
                                     join pc in db.ProductCategories on p.Id equals pc.ProductId
                                     join c in db.Categories on pc.CategoryId equals c.Id
                                     where c.Id == CategoryId
                                     select p;
            return PartialView(defaultListProduct);
        }

        public ActionResult _NavPillPartial(int? ActiveCategoryId)
        {
            ViewBag.ActiveCategoryId = ActiveCategoryId != null ? ActiveCategoryId : 0;
            return PartialView(db.Categories.Take(3).ToList());
        }

        public ActionResult _CarouselPartial()
        {
            return PartialView();
        }
    }
}