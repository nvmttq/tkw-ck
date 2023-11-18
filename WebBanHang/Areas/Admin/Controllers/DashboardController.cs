using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{

    public class DashboardController : Controller
    {

        private WebBanHangEntities db = new WebBanHangEntities();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public DashboardController()
        {
        }

        public DashboardController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AuthorizeLogin(string returnUrl)
        {
            if (returnUrl == "https://localhost:44302/Admin/Dashboard/LoginAdmin") return Json(new { code = 103, msg = "Khon can" }, JsonRequestBehavior.AllowGet);
            User u = Session["user"] as User;
            if (u == null) return Json(new { code=101, msg="Can dang nhap"}, JsonRequestBehavior.AllowGet);

            return Json(new { code = 103, msg = "Khon can" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _AuthorizeLoginSQL()
        {
            User u = Session["user"] as User;
            if (u == null) return RedirectToAction("LoginAdmin");

            return PartialView("_AuthorizeLoginSQL", u);
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session["user"] = null;
            ViewBag.z = "LOGOUT";
            Console.WriteLine(ViewBag.z);
            return RedirectToAction("LoginAdmin");
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult LoginAdmin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginAdmin(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var checkLogin = model.EmailOrUsername.Contains('@');
            if (!checkLogin)
            {
                var userChecked = db.Users.ToList();
                if (userChecked.Any(u => u.Username == model.EmailOrUsername && u.Password == model.Password))
                {
                    var user1 = (from u in db.Users
                                 where u.Username == model.EmailOrUsername && u.Password == model.Password
                                 select u).Single();

                   

                    if (user1.RoleId == 1)
                    {
                        Session["user"] = user1;
                        return RedirectToAction("Index");
                    }
                }
                ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                return View(model);
            }
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.EmailOrUsername, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user1 = (from u in db.Users
                                 where u.Email == model.EmailOrUsername && u.Password == model.Password
                                 select u).Single();

                    

                    if (user1.RoleId == 1)
                    {
                        Session["user"] = user1;
                        return RedirectToAction("Index");
                    }
                    ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                    return View(model);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu");
                    return View(model);
            }
        }
    }
}