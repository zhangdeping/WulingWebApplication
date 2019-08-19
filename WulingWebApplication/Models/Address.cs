using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WulingWebApplication.Models
{
    /// <summary>
    /// 用于保持地址
    /// </summary>
    public class Address
    {
        public Guid Id { get; set; }
        public string Province { get; set; }//省
        public string City { get; set; }//市
        public string County { get; set; }//区县

        public AppRole Role { get; set; }//归属于哪个角色

        public Address()
        {
            Id = Guid.NewGuid();
            Province = "全部";
            City = "全部";
            County = "全部";
        }
    }
}