using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using WulingWebApplication.Infrastructure;
using WulingWebApplication.Models;

namespace WulingWebApplication.Controllers
{
    [Authorize]
    public class QueryController : Controller
    {
        // GET: Query
        [AllowAnonymous]
        public ActionResult PrimaryQuery()
        {
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult PrimaryQuery(string Province,string City,string County)
        {
            List<PassengerVehicle> list = new List<PassengerVehicle>();
            using(WuLinEntities1 db = new WuLinEntities1())
            {
                list = db.PassengerVehicles.Where(x => x.省.Contains(Province)).Take(2).ToList();
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
                ViewData["info"] = "当前数据为示例数据。。。";
                var model = db.PassengerVehicles.Take(1).OrderByDescending(a => a.时间).ToPagedList(id, 1);
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
        public async Task<ActionResult> MultiQuery(string Province, string City, string County,int id)
        {
           

            using (var db =  new WuLinEntities1())
            {
                var model = db.PassengerVehicles.Take(1).OrderByDescending(a => a.时间).ToPagedList(id, 1);//示例数据

                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                string userName = System.Web.HttpContext.Current.User.Identity.Name;
                if (userName == null || userName == "")
                {
                    ViewData["info"] = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                    if (Request.IsAjaxRequest())
                        return PartialView("_AjaxSearchPost", model);
                    return View(model);
                }

                AppUser user = await UserManager.FindByNameAsync(userName);
                List<Address> listAddress = GetUserAddress(user);
                if(listAddress== null ||listAddress.Count == 0)
                {
                    ViewData["info"] = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                    if (Request.IsAjaxRequest())
                        return PartialView("_AjaxSearchPost", model);
                    return View(model);
                }

                if(AddressListProvinceContainAll(listAddress) == false)//地址列表中没有全部省份
                {
                    bool isInProvince = false;
                    foreach (var item in listAddress)
                    {
                        if (item.Province.Contains(Province) || Province.Contains(item.Province) || item.Province.Equals(Province))
                        {
                            isInProvince = true;
                            break;
                        }
                           
                    }
                   
                    if (isInProvince == false)
                    {
                        ViewData["info"] = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                        if (Request.IsAjaxRequest())
                            return PartialView("_AjaxSearchPost", model);
                        return View(model);
                    }
                    List<Address> provinceAddressList = new List<Address>();//记录含有本函数Province参数值的地址
                    foreach (var item in listAddress)
                    {
                        if (item.Province.Contains(Province) || Province.Contains(item.Province) || item.Province.Equals(Province))
                            provinceAddressList.Add(item);
                    }

                    if (AddressListCityContainAll(provinceAddressList) == false)//对应省的地址列表中没有全部市
                    {
                        bool isInCity = false;
                        foreach (var item in provinceAddressList)
                        {
                            if (item.City.Contains(City) || City.Contains(item.City) || item.City.Equals(City))
                            {
                                isInCity = true;
                                break;
                            }
                               
                        }
                        if (isInCity == false)
                        {
                            ViewData["info"] = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                            if (Request.IsAjaxRequest())
                                return PartialView("_AjaxSearchPost", model);
                            return View(model);
                        }

                        List<Address> CityAddressList = new List<Address>();//记录对应省含有本函数City参数值的地址
                        foreach (var item in provinceAddressList)
                        {
                            if (item.City.Contains(City) || City.Contains(item.City) || item.City.Equals(City))
                                CityAddressList.Add(item);
                        }
                        if (AddressListCountyContainAll(CityAddressList) ==false)//地址列表中没有全部区县
                        {
                            bool isInCounty = false;
                            foreach (var item in CityAddressList)
                            {
                                if (item.County.Contains(County) || County.Contains(item.County) || item.County.Equals(County))
                                {
                                    isInCounty = true;
                                    break;
                                }
                                    
                            }
                            if (isInCounty == false)
                            {
                                ViewData["info"] = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                                if (Request.IsAjaxRequest())
                                    return PartialView("_AjaxSearchPost", model);
                                return View(model);
                            }
                        }
                       
                    }
                    
                }

               

                var qry = db.PassengerVehicles.AsQueryable();
                if (!string.IsNullOrWhiteSpace(Province) && !Province.Contains("全部"))
                    qry = qry.Where(x => x.省.Contains(Province)|| Province.Contains(x.省));
                if (!string.IsNullOrWhiteSpace(City) && !City.Contains("全部"))
                    qry = qry.Where(x => x.市.Contains(City) || City.Contains(x.市));
                if (!string.IsNullOrWhiteSpace(County) && !County.Contains("全部"))
                    qry = qry.Where(x => x.县.Contains(County) || County.Contains(x.县));
                model = qry.OrderByDescending(a => a.时间).ToPagedList(id, 7);
                if (Request.IsAjaxRequest())
                    return PartialView("_AjaxSearchPost", model);
                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                return View(model);
            }
        }

        private List<Address> GetUserAddress(AppUser user)
        {
            List<Address> list = new List<Address>();
            AppIdentityDbContext db = HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
            var roles = user.Roles;
            foreach(var role in roles)
            {
                var tempList = db.Addresses.Where(x => x.Role.Id == role.RoleId).ToList();
                if(tempList != null)
                {
                    list.AddRange(tempList);
                }
                tempList = null;
                
            }

            return list;
        }

        /// <summary>
        /// 查找地址列表中的省份是否包含“全部”
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool AddressListProvinceContainAll(List<Address> list)
        {
            bool flag = false;
            foreach(var item in list)
            {
                if(item.Province.Contains("全部") || "全部".Contains(item.Province))
                {
                    flag = true;
                    break;
                }
            }          

            return flag;
        }

        /// <summary>
        /// 查找地址列表中的城市是否包含“全部”
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool AddressListCityContainAll(List<Address> list)
        {
            bool flag = false;
            foreach (var item in list)
            {
                if (item.City.Contains("全部") || "全部".Contains(item.City))
                {
                    flag = true;
                    break;
                }
            }

          
            return flag;
        }

        /// <summary>
        /// 查找地址列表中的区县是否包含“全部”
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool AddressListCountyContainAll(List<Address> list)
        {
            bool flag = false;
            foreach (var item in list)
            {
                if (item.County.Contains("全部") || "全部".Contains(item.County))
                {
                    flag = true;
                    break;
                }
            }

            return flag;
        }
        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppRoleManager>();
            }
        }
    }
}