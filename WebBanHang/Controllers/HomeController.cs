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
        public ActionResult Index(int? CategoryId)
        {
            ViewBag.ActivePage = 1;
            return View();
        }

        public bool CheckContainsKeyword(string name, string keyword)
        {
            return (name.ToUpper()).Contains(keyword.ToUpper()) == true;
        }

     
        public ActionResult Search(int? pageCurrent, string categoryId = null,  string keyword = null)
        {
            var isAjax = Request.IsAjaxRequest();
            ViewBag.url = Request.Url.ToString();
            if (isAjax == false) return RedirectToAction("Index");
            var splitCategoryId = new List<string>();
            if(categoryId == null || categoryId == "")
            {
                foreach(var c in db.Categories.ToList())
                {
                    splitCategoryId.Add(c.Id.ToString());
                }
            } else
            {
                foreach(var c in categoryId.Split(','))
                {
                    splitCategoryId.Add(c.ToString());
                }
            }

            var defaultListProduct = (from p in db.Products
                                      join pc in db.ProductCategories on p.Id equals pc.ProductId
                                      join c in db.Categories on pc.CategoryId equals c.Id
                                      where (splitCategoryId).Contains(c.Id.ToString()) == true &&
                                      (keyword == null ? p.Name.ToString() : keyword.ToString()).Contains(p.Name.ToString()) == true
                                      select p);

            var tmp = defaultListProduct;
            var pagesize = tmp.ToList().Count();
            ViewBag.ActiveCategoryId = categoryId;
            ViewBag.TotalPage = pagesize / 10;
            ViewBag.ActivePage = (pageCurrent ?? 1);
            if (pagesize % 10 != 0) ViewBag.TotalPage++;

            return PartialView("_ProductsPartial", defaultListProduct.ToList().Distinct().OrderBy(p=>p.Name).Skip(((pageCurrent ?? 1)-1)*recordPerPage).Take(recordPerPage).ToList());
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

        [HttpGet]
        public ActionResult ViewPage(int CategoryId, int page)
        {
            string keyword = Request.QueryString["Page"];
            ViewBag.Text = keyword;
            var ProductsCurrent = from p in db.Products
                                  join pc in db.ProductCategories on p.Id equals pc.ProductId
                                  join c in db.Categories on pc.CategoryId equals c.Id
                                  where c.Id == CategoryId
                                  select p;

            var pagesize = ProductsCurrent.Count();
            ViewBag.TotalPage = pagesize / 10;
            if (pagesize % 10 != 0) ViewBag.TotalPage++;
            ViewBag.ActiveCategoryId = CategoryId;
            ViewBag.PageCurrent = page;

            var resultProducts = (ProductsCurrent.OrderBy(p => p.Name).Skip((page - 1) * 10).Take(10)).ToList();

            return RedirectToAction("_ProductsPartial", new { CategoryId=CategoryId, page=page});
            //return Json(new { code=200, activePage=page, activeCategory=CategoryId, data = resultProducts }, JsonRequestBehavior.AllowGet);
        }

 
        public ActionResult SearchProducts(int? CategoryId, int? page)
        {
            string keyword = Request.QueryString["keyword"];
            string tim_kiem_theo = Request.QueryString["tim_kiem_theo"];

            var products = db.Products.ToList();
            var z = from p in db.Products
                    where p.Name.ToString().ToUpper().Contains(keyword.ToUpper())
                    select p;

            var pagesize = z.Count();
            ViewBag.TotalPage = pagesize / 10;
            if (pagesize % 10 != 0) ViewBag.TotalPage++;
            ViewBag.ActiveCategoryId = CategoryId;
            ViewBag.PageCurrent = 1;

            if (tim_kiem_theo.ToUpper() == "DANH MỤC")
            {
                z = from p in db.Products
                        join pc in db.ProductCategories
                        on p.Id equals pc.ProductId
                        join c in db.Categories
                        on pc.CategoryId equals c.Id
                        where c.Name.ToString().ToUpper().Contains(keyword.ToUpper())
                        select p;

            }
            return View("Index", z.ToList());
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

        
        public ActionResult SortingProduct(string sortby)
        {
            var ProductsCurrent = (List<Product>)Session["ProductsCurrent"];

            List<string> pp = new List<string>();
            foreach(var item in ProductsCurrent)
            {
                pp.Add(item.Id.ToString());
            }

            if (sortby == "Name")
            {
                var products = (from p in db.Products
                                where (pp.Any(pc => pc == p.Id.ToString())) == true
                                select p).ToList().OrderBy(p => p.Name).Select(p => p).ToList();

                return View("Index", products);
            }
            if(sortby == "NameDesc")
            {
                var products = (from p in db.Products
                                where (pp.Any(pc => pc == p.Id.ToString())) == true
                                select p).ToList().OrderByDescending(p => p.Name).Select(p => p).ToList();
                return View("Index", products);
            }
            if(sortby == "Price")
            {
                var products = (from p in db.Products
                                where (pp.Any(pc => pc == p.Id.ToString())) == true
                                select p).ToList().OrderBy(p => p.Price).Select(p => p).ToList();

                return View("Index", products);
            }
            if(sortby == "PriceDesc")
            {
                var products = (from p in db.Products
                                where (pp.Any(pc => pc == p.Id.ToString())) == true
                                select p).ToList().OrderByDescending(p => p.Price).Select(p => p).ToList();
                return View("Index", products);
            }
            return View("Index", db.Products.ToList());
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