using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using WulingWebApplication.Infrastructure;
using WulingWebApplication.Models;
using ALinq.Dynamic;
using System.Data;
using System.Dynamic;

namespace WulingWebApplication.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        /// <summary>
        /// 多条件统计，包括分页，采用MvcPager插件
        /// </summary>
        /// <param name="id">
        /// 页码
        /// </param>
        /// <returns></returns>
        public ActionResult MultiStatistics(int id = 1)
        {
            using (var db = new WuLinEntities1())
            {
                ViewData["info"] = "当前数据为示例数据。。。";
                var fields = new List<string>();
                fields.Add("品牌");
                fields.Add("合计");
                ViewData["fields"] = fields;
                var model = new List<MyDynamicType>() { new MyDynamicType { 品牌 = "大众", 合计 = 100 } }.ToPagedList(id, 1);
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
        /// <param name="CheckValues">统计参数列表 用空格隔开</param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult MultiStatistics(string Province, string City, string County, int id=1, string CheckValues ="")
        {
            string[] statisticsParam = CheckValues.Split(' ');
            //statisticsParam[statisticsParam.Length] = "合计";
            List<string> fields = new List<string>();
            
            string groupParams = CheckValues.Replace(' ', ',').Replace("国产_进口", "[国产/进口]");
            string selectParams = groupParams.Replace("[国产/进口]", "[国产/进口] as 国产进口");
            if (statisticsParam.Length > 0)
            {
                fields.AddRange(statisticsParam);//返回给前端，字段名集合
            }

            fields.Add("合计");
            if(fields.Contains("国产_进口"))
            {
                fields.Remove("国产_进口");
                fields.Add("国产进口");
            }
            string errorMessage = "";
            errorMessage = "";
            //using (var db = new WuLinEntities1())
            //var qry = GetOriginData(Province, City, County, out errorMessage);
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;

            if (CheckValues == "" || CheckValues == null)
            {
                ViewData["info"] = "当前数据为示例数据。。。";
                fields = new List<string>();
                fields.Add("品牌");
                fields.Add("合计");
                ViewData["fields"] = fields;
                var model1 = new List<MyDynamicType>() { new MyDynamicType { 品牌 = "大众", 合计 = 100 } }.ToPagedList(id, 1);
                if (Request.IsAjaxRequest())
                    return PartialView("_AjaxSearchPost", model1);
                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                return View(model1);
            }
            
            string userName = System.Web.HttpContext.Current.User.Identity.Name;
            if (userName == null || userName == "")
            {
                errorMessage = "您没有权限查询该地域的数据！！";
                ViewData["info"] = errorMessage;
                fields = new List<string>();
                fields.Add("品牌");
                fields.Add("合计");
                ViewData["fields"] = fields;
                var model1 = new List<MyDynamicType>() { new MyDynamicType { 品牌 = "大众", 合计 = 100 } }.ToPagedList(id, 1);
                if (Request.IsAjaxRequest())
                    return PartialView("_AjaxSearchPost", model1);
                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                return View(model1);
            }

            AppUser user = UserManager.FindByName(userName);
            List<Address> listAddress = GetUserAddress(user);
            if (listAddress == null || listAddress.Count == 0)
            {
                errorMessage = "您没有权限查询该地域的数据！！";
                ViewData["info"] = errorMessage;
                fields = new List<string>();
                fields.Add("品牌");
                fields.Add("合计");
                ViewData["fields"] = fields;
                var model1 = new List<MyDynamicType>() { new MyDynamicType { 品牌 = "大众", 合计 = 100 } }.ToPagedList(id, 1);
                if (Request.IsAjaxRequest())
                    return PartialView("_AjaxSearchPost", model1);
                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                return View(model1);
            }

            if (AddressListProvinceContainAll(listAddress) == false)//地址列表中没有全部省份
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
                    errorMessage = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                    ViewData["info"] = errorMessage;
                    fields = new List<string>();
                    fields.Add("品牌");
                    fields.Add("合计");
                    ViewData["fields"] = fields;
                    var model1 = new List<MyDynamicType>() { new MyDynamicType { 品牌 = "大众", 合计 = 100 } }.ToPagedList(id, 1);
                    if (Request.IsAjaxRequest())
                        return PartialView("_AjaxSearchPost", model1);
                    ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                    return View(model1);
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
                        errorMessage = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                        ViewData["info"] = errorMessage;
                        fields = new List<string>();
                        fields.Add("品牌");
                        fields.Add("合计");
                        ViewData["fields"] = fields;

                        var model1 = new List<MyDynamicType>() { new MyDynamicType { 品牌 = "大众", 合计 = 100 } }.ToPagedList(id, 1);
                        if (Request.IsAjaxRequest())
                            return PartialView("_AjaxSearchPost", model1);
                        ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                        return View(model1);
                    }

                    List<Address> CityAddressList = new List<Address>();//记录对应省含有本函数City参数值的地址
                    foreach (var item in provinceAddressList)
                    {
                        if (item.City.Contains(City) || City.Contains(item.City) || item.City.Equals(City))
                            CityAddressList.Add(item);
                    }
                    if (AddressListCountyContainAll(CityAddressList) == false)//地址列表中没有全部区县
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
                            errorMessage = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                            ViewData["info"] = errorMessage;
                            fields = new List<string>();
                            fields.Add("品牌");
                            fields.Add("合计");
                            ViewData["fields"] = fields;

                            var model1 = new List<MyDynamicType>() { new MyDynamicType { 品牌 = "大众", 合计 = 100 } }.ToPagedList(id, 1);
                            if (Request.IsAjaxRequest())
                                return PartialView("_AjaxSearchPost", model1);
                            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                            return View(model1);
                        }
                    }

                }

            }
            var esql = @"select " + @selectParams + ",sum(p.保有量) as 合计 from  PassengerVehicle as p " ;//"group by " + groupParams;
            var qry = db.PassengerVehicles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(Province) && !Province.Contains("全部"))
            {
                esql += @" where  (charindex('"+Province+ "',p.省)>0  or charindex(p.省, '" + Province + "')>0) ";
            }
                qry = qry.Where(x => x.省.Contains(Province) || Province.Contains(x.省));
            if (!string.IsNullOrWhiteSpace(City) && !City.Contains("全部"))
            {
               esql += @" and charindex('" + City + "',p.市)>0 ";
            }
                qry = qry.Where(x => x.市.Contains(City) || City.Contains(x.市));
            if (!string.IsNullOrWhiteSpace(County) && !County.Contains("全部"))
            {
                esql+= @" and charindex('" + County + "',p.县)>0 ";
            }

            esql += @" group by " + @groupParams;
            esql += @" order by " + groupParams.Split(',')[0];

            var items = db.Database.SqlQuery<MyDynamicType> (esql);


           ViewData["info"] = errorMessage;

           var  model = items.ToPagedList(id, 7);
            ViewData["fields"] = fields;
            if (Request.IsAjaxRequest())
                return PartialView("_AjaxSearchPost", model);
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View(model);
            
        }


        private IQueryable<PassengerVehicle> GetOriginData(string Province, string City, string County,out string errorMessage)
        {
            errorMessage = "";
            //using (var db = new WuLinEntities1())
            {
                //var model = db.PassengerVehicles.Take(1).OrderByDescending(a => a.时间).ToPagedList(id, 1);//示例数据

                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                string userName = System.Web.HttpContext.Current.User.Identity.Name;
                if (userName == null || userName == "")
                {
                    errorMessage = "您没有权限查询该地域的数据！！";
                    ViewData["info"] = errorMessage;
                    return null;
                }

                AppUser user =  UserManager.FindByName(userName);
                List<Address> listAddress = GetUserAddress(user);
                if (listAddress == null || listAddress.Count == 0)
                {
                   errorMessage = "您没有权限查询该地域的数据！！";
                   
                    return null;
                }

                if (AddressListProvinceContainAll(listAddress) == false)//地址列表中没有全部省份
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
                        errorMessage = "您没有权限查询该地域的数据！！当前数据为示例数据。。。";
                       
                        return null;
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
                            errorMessage = "您没有权限查询该地域的数据！！";
                           
                            return null;
                        }

                        List<Address> CityAddressList = new List<Address>();//记录对应省含有本函数City参数值的地址
                        foreach (var item in provinceAddressList)
                        {
                            if (item.City.Contains(City) || City.Contains(item.City) || item.City.Equals(City))
                                CityAddressList.Add(item);
                        }
                        if (AddressListCountyContainAll(CityAddressList) == false)//地址列表中没有全部区县
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
                                errorMessage = "您没有权限查询该地域的数据！！";
                                
                                return null;
                            }
                        }

                    }

                }

                var qry = db.PassengerVehicles.AsQueryable();
                if (!string.IsNullOrWhiteSpace(Province) && !Province.Contains("全部"))
                    qry = qry.Where(x => x.省.Contains(Province) || Province.Contains(x.省));
                if (!string.IsNullOrWhiteSpace(City) && !City.Contains("全部"))
                    qry = qry.Where(x => x.市.Contains(City) || City.Contains(x.市));
                if (!string.IsNullOrWhiteSpace(County) && !County.Contains("全部"))
                    qry = qry.Where(x => x.县.Contains(County) || County.Contains(x.县));
                //var model = qry.OrderByDescending(a => a.时间).ToPagedList(id, 7);


                return qry;
            }
        }

        private List<Address> GetUserAddress(AppUser user)
        {
            List<Address> list = new List<Address>();
            AppIdentityDbContext db = HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
            var roles = user.Roles;
            foreach (var role in roles)
            {
                var tempList = db.Addresses.Where(x => x.Role.Id == role.RoleId).ToList();
                if (tempList != null)
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
            foreach (var item in list)
            {
                if (item.Province.Contains("全部") || "全部".Contains(item.Province))
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


        private WuLinEntities1 db = new WuLinEntities1();

        //private SqlConnection connection;

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            //connection.Close();
            //connection.Dispose();
            base.Dispose(disposing);
        }
    }
}