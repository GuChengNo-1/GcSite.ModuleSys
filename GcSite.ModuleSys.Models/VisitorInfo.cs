using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GcSite.ModuleSys.Models
{
    /// <summary>
    /// 访客信息
    /// </summary>
    public class VisitorInfo : EntityBase
    {
        /// <summary>
        /// 访问页面
        /// </summary>
        public string VisitPage { get; set; }
        /// <summary>
        /// ip地址
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 搜索引擎
        /// </summary>
        public string VisitSE { get; set; }
        /// <summary>
        /// 访问时间
        /// </summary>
        public DateTime AccessTime { get; set; }
        /// <summary>
        /// 访问结束时间
        /// </summary>
        public DateTime AccessEndTime { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public Double Duration { get; set; }
        /// <summary>
        /// (0 or 1)访客是否只访问了一个页面
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// IC 随机生成标识
        /// </summary>
        //[Key]
        public string IC { get; set; }
        /// <summary>
        /// 更改访客状态(0 or 1)默认0 
        /// 超过了24小时后将修改成1
        /// </summary>
        public int Lock { get; set; }
        /// <summary>
        /// 外键(网站流量表)
        /// </summary>
        public virtual WebInfo WebInfo { get; set; }
    }
}
