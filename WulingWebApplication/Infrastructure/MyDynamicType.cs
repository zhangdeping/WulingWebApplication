using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace WulingWebApplication.Infrastructure
{
    public class MyDynamicType:DynamicObject
    {
        public int 合计 { get; set; }
        public string 时间 { get; set; }
        public string 国产_进口 { get; set; }
        public string 省 { get; set; }
        public string 市 { get; set; }
        public string 县 { get; set; }
        public string 制造商 { get; set; }
        public string 车辆型号 { get; set; }
        public string 品牌 { get; set; }
        public string 车型 { get; set; }
        public string 排量 { get; set; }
        public string 变速器 { get; set; }
        public string 车辆类型 { get; set; }
        public string 车身型式 { get; set; }
        public string 燃油类型 { get; set; }
        public string 使用性质 { get; set; }
        public string 所有权 { get; set; }
        public string 抵押标记 { get; set; }
        public string 性别 { get; set; }
        public string 年龄 { get; set; }
        public string 车身颜色 { get; set; }
        public string 发动机型号 { get; set; }
        public string 功率 { get; set; }
        public string 排放标准 { get; set; }
        public string 轴距 { get; set; }
        public string 轮胎规格 { get; set; }
        public string 车外廓长 { get; set; }
        public string 车外廓宽 { get; set; }
        public string 车外廓高 { get; set; }
        public string 准确排量 { get; set; }
        public string 核定载客 { get; set; }
        public string 总质量 { get; set; }
        public string 整备质量 { get; set; }
        public string 轴数 { get; set; }
        public string 前轮距 { get; set; }
        public string 后轮距 { get; set; }
        public Nullable<int> 保有量 { get; set; }


        Dictionary<string, object> Properties = new Dictionary<string, object>();

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!Properties.Keys.Contains(binder.Name))
            {
                //在此可以做一些小动作
                //if (binder.Name == "Col")
                //　　Properties.Add(binder.Name + (Properties.Count), value.ToString());
                //else
                //　　Properties.Add(binder.Name, value.ToString());

                Properties.Add(binder.Name, value.ToString());
            }
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return Properties.TryGetValue(binder.Name, out result);
        }

    }
}