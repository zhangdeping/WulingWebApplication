using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using WulingWebApplication.Models;

namespace WulingWebApplication.Controllers
{
    [Authorize]
    public class QueryController : Controller
    {
        // GET: Query
        public ActionResult PrimaryQuery()
        {
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }

        [HttpPost]
        public ActionResult PrimaryQuery(string Province,string City,string County)
        {
            List<PassengerVehicle> list = new List<PassengerVehicle>();
            using(WuLinEntities1 db = new WuLinEntities1())
            {
                list = db.PassengerVehicles.Where(x => x.省.Contains(Province)).Take(20).ToList();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 多条件查询，包括分页，采用MvcPager插件
        /// </summary>
        /// <param name="id">
        /// 页码
        /// </param>
        /// <returns></returns>
        public ActionResult MultiQuery(int id = 1)
        {
            using (var db = new WuLinEntities1())
            {
                var model = db.PassengerVehicles.OrderByDescending(a => a.时间).ToPagedList(id, 7);
                if (Request.IsAjaxRequest())
                    return PartialView("_AjaxSearchPost", model);
                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                return View(model);
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Province"></param>
        /// <param name="City"></param>
        /// <param name="County"></param>
        /// <param name="id"> 页码</param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult MultiQuery(string Province, string City, string County,int id)
        {
           

            using (var db =  new WuLinEntities1())
            {
                var qry = db.PassengerVehicles.AsQueryable();
                if (!string.IsNullOrWhiteSpace(Province) && !Province.Contains("请选择"))
                    qry = qry.Where(x => x.省.Contains(Province)|| Province.Contains(x.省));
                if (!string.IsNullOrWhiteSpace(City) && !City.Contains("请选择"))
                    qry = qry.Where(x => x.市.Contains(City) || City.Contains(x.市));
                if (!string.IsNullOrWhiteSpace(County) && !County.Contains("请选择"))
                    qry = qry.Where(x => x.县.Contains(County) || County.Contains(x.县));
                var model = qry.OrderByDescending(a => a.时间).ToPagedList(id, 7);
                if (Request.IsAjaxRequest())
                    return PartialView("_AjaxSearchPost", model);
                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                return View(model);
            }
        }
    }
}