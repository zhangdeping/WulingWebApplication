using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WulingWebApplication.Infrastructure;
using WulingWebApplication.Models;

namespace WulingWebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return View("Error", new string[] { "已登录，您没有权限！！！请先退出，用正确的用户再登录！！" });
            }
            ViewBag.returnUrl = returnUrl;
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel details, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await UserManager.FindAsync(details.Name,
                            details.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid name or password.");
                }
                else
                {
                    ClaimsIdentity ident = await UserManager.CreateIdentityAsync(user,
                                DefaultAuthenticationTypes.ApplicationCookie);
                    AuthManager.SignOut();
                    AuthManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = false
                    }, ident);
                    if(returnUrl != "")
                    {
                        return Redirect(returnUrl);
                    }
                   
                }
            }
            ViewBag.returnUrl = returnUrl;
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View(details);
        }

        [Authorize]
        public ActionResult Logout()
        {
            AuthManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ChangePassWD()
        {
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }

        [HttpPost]
        public string  ChangePassWD( string oldPassword,string newPassword)
        {
            AppUser user = UserManager.FindById(System.Web.HttpContext.Current.User.Identity.GetUserId()) ;
            if(user == null)
            {
                return "用户未登录，请先登录！！";
            }
            if(UserManager.CheckPassword(user,oldPassword)==false)
            {
                return "旧密码不对";
            }
            IdentityResult result = UserManager.ChangePassword(user.Id, oldPassword, newPassword);
            if (result.Succeeded==true)
            {
                return "Success";
            }
            return result.Errors.ToString();
        }
        private IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }
    }
}