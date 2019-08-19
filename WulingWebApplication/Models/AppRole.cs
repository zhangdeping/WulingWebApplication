using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WulingWebApplication.Models
{
    public class AppRole : IdentityRole
    {
        //用于存储该角色可以访问的范围（弃用），改成Addresses数据表存储
        [Column(TypeName = "nvarchar(MAX)")]
        public string AccessPowers { get; set; }
        public AppRole() : base() { }

        public AppRole(string name) : base(name) { }
    }
}