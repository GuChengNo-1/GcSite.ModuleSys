using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GcSite.ModuleSys.Models
{
    /// <summary>
    /// pv信息
    /// </summary>
    public class FlowComputer:EntityBase
    {
        /// <summary>
        /// pv当前时间
        /// </summary>
        public DateTime CurrentTime { get; set; }
        /// <summary>
        /// pv当前页面
        /// </summary>
        public string VisitPage { get; set; }
        /// <summary>
        /// pv当前关键字
        /// </summary>
        public string SearchTerms { get; set; }
        /// <summary>
        /// pv当前搜索引擎
        /// </summary>
        public string VisitSE { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string DeviceType { get; set; }
        /// <summary>
        /// pv当前域名
        /// </summary>
        public string WebHost { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 外键(网站流量表)
        /// </summary>
        public virtual WebInfo WebInfo { get; set; }
    }
}
