using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WulingWebApplication.Models
{
    public class CreateModel
    {
        [Required]
        [Display(Name ="用户名")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "电子邮箱")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "密码")]
        public string Password { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "密码")]
        public string Password { get; set; }
    }

    public class RoleEditModel
    {
        public AppRole Role { get; set; }
        public IEnumerable<AppUser> Members { get; set; }
        public IEnumerable<AppUser> NonMembers { get; set; }
    }

    public class RoleModificationModel
    {
        [Required]
        public string RoleName { get; set; }
        public string[] IdsToAdd { get; set; }
        public string[] IdsToDelete { get; set; }
    }
}