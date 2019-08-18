using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WulingWebApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }
    }
}