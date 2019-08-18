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
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> UpdateAccessPowers(string id)
        {
            AppRole role = await  RoleManager.FindByIdAsync(id);
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View(role);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAccessPowers(AppRole role)
        {
            AppRole myRole = await RoleManager.FindByIdAsync(role.Id);
            myRole.Name = role.Name;
            myRole.AccessPowers = role.AccessPowers;
            
            
            var result = await RoleManager.UpdateAsync(myRole);
            if(result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            ViewData["user"] = System.Web.HttpContext.Current.User.Identity.Name;
            return View("Error",new string[] { "更新"+role.Name+"角色的访问权限失败"});
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