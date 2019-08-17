using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WulingWebApplication.Models;

namespace WulingWebApplication.Controllers
{
    [Authorize]
    public class QueryController : Controller
    {
        // GET: Query
        public ActionResult PrimaryQuery()
        {
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
    }
}