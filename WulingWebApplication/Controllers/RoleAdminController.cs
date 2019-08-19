using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WulingWebApplication.Infrastructure;
using WulingWebApplication.Models;

namespace WulingWebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleAdminController : Controller
    {
        public ActionResult Index()
        {
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View(RoleManager.Roles);
        }

        public ActionResult Create()
        {
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create([Required]string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result
                   = await RoleManager.CreateAsync(new AppRole(name));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View(name);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            AppRole role = await RoleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await RoleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                    return View("Error", result.Errors);
                }
            }
            else
            {
                ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                return View("Error", new string[] { "Role Not Found" });
            }
        }

        public async Task<ActionResult> Edit(string id)
        {
            AppRole role = await RoleManager.FindByIdAsync(id);
            string[] memberIDs = role.Users.Select(x => x.UserId).ToArray();
            IEnumerable<AppUser> members
                    = UserManager.Users.Where(x => memberIDs.Any(y => y == x.Id));
            IEnumerable<AppUser> nonMembers = UserManager.Users.Except(members);
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View(new RoleEditModel
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }

        [HttpPost]
        public async Task<ActionResult> Edit(RoleModificationModel model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.IdsToAdd ?? new string[] { })
                {
                    result = await UserManager.AddToRoleAsync(userId, model.RoleName);
                    if (!result.Succeeded)
                    {
                        ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                        return View("Error", result.Errors);
                    }
                }
                foreach (string userId in model.IdsToDelete ?? new string[] { })
                {
                    result = await UserManager.RemoveFromRoleAsync(userId,
                       model.RoleName);
                    if (!result.Succeeded)
                    {
                        ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
                        return View("Error", result.Errors);
                    }
                }

                
                return RedirectToAction("Index");
            }
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View("Error", new string[] { "Role Not Found" });
        }

        /// <summary>
        /// 更新角色的访问权限
        /// </summary>
        /// <param name="id">角色id</param>
        /// <returns></returns>
        public async Task<ActionResult> UpdateAccessPowers(string id)
        {
            AppRole role = await  RoleManager.FindByIdAsync(id);
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            ViewData["Role"] = role;
            List<Address> addressList = new List<Address>();
            using (AppIdentityDbContext db = new AppIdentityDbContext())
            {
                addressList = db.Addresses.Where(x => x.Role.Id == id).ToList();
            }

                return View(addressList);
        }

        //[HttpPost]
        //public async Task<ActionResult> UpdateAccessPowers(AppRole role)
        //{
        //    AppRole myRole = await RoleManager.FindByIdAsync(role.Id);
        //    myRole.Name = role.Name;
        //    myRole.AccessPowers = role.AccessPowers;
            
            
        //    var result = await RoleManager.UpdateAsync(myRole);
        //    if(result.Succeeded)
        //    {
        //        return RedirectToAction("Index");
        //    }
        //    ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
        //    return View("Error",new string[] { "更新"+role.Name+"角色的访问权限失败"});
        //}

        /// <summary>
        /// 增加访问地址
        /// </summary>
        /// <param name="id">AppRole 的Id</param>
        /// <returns></returns>
        public async Task<ActionResult> AddAddress(string id)
        {
            AppRole role = await RoleManager.FindByIdAsync(id);
            ViewData["Role"] = role;
            return View();
        }

        /// <summary>
        /// 增加访问地址
        /// </summary>
        /// <param name="id">AppRole 的Id</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddAddress(string Province,string City,string County,string roleId)
        {
            AppRole role =  RoleManager.FindById(roleId);
            Address address = new Address();
            address.Id = Guid.NewGuid();
            address.Province = Province;
            address.City = City;
            address.County = County;
            address.Role = role;
            AppIdentityDbContext db = HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
            db.Addresses.Add(address);
            await db.SaveChangesAsync();
            
            //ViewData["Role"] = role;
            return RedirectToAction("UpdateAccessPowers",new {id=roleId });
        }

        /// <summary>
        /// 删除访问地址
        /// </summary>
        /// <param name="id"> 地址id</param>
        /// <returns></returns>
        public async Task<string> DeleteAddress(string id)
        {
            AppIdentityDbContext db = HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
            var address = db.Addresses.Where(x => x.Id == new Guid(id)).FirstOrDefault();
            if(address != null)
            {
                db.Addresses.Remove(address);
                await db.SaveChangesAsync();
            }

            return "Success";
        }

        /// <summary>
        /// 编辑地址
        /// </summary>
        /// <param name="id"> 地址id</param>
        /// <returns></returns>
        public async Task<ActionResult> EditAddress(string id,string roleId)
        {
            AppIdentityDbContext db = HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
            var address = db.Addresses.Where(x => x.Id == new Guid(id)).FirstOrDefault();
            AppRole role = await RoleManager.FindByIdAsync(roleId);
            ViewData["Role"] = role;
            return View(address);
        }

        /// <summary>
        /// 编辑地址
        /// </summary>
        /// <param name="id"> 地址id</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditAddress(Address address, string roleId)
        {
            AppIdentityDbContext db = HttpContext.GetOwinContext().Get<AppIdentityDbContext>();
            var originalAddress = db.Addresses.Where(x => x.Id == address.Id).FirstOrDefault();
            if(originalAddress != null)
            {
                originalAddress.Province = address.Province;
                originalAddress.City = address.City;
                originalAddress.County = address.County;
            }
            await db.SaveChangesAsync();
            return RedirectToAction("UpdateAccessPowers", new { id = roleId });
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
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