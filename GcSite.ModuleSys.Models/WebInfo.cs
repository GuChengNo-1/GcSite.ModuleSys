using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GcSite.ModuleSys.Models
{
    /// <summary>
    /// 网站信息
    /// </summary>
    public class WebInfo:EntityBase
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string WebKey { get; set; }
        /// <summary>
        /// 网站域名
        /// </summary>
        public string WebDomain { get; set; }
        /// <summary>
        /// pv
        /// </summary>
        public int WebPv { get; set; }
        /// <summary>
        /// uv
        /// </summary>
        public int WebUv { get; set; }
        /// <summary>
        /// ip数
        /// </summary>
        public int IpCount { get; set; }
        /// <summary>
        /// 跳出率
        /// </summary>
        public string BounceRate { get; set; }
        /// <summary>
        /// 平均访问时长
        /// </summary>
        public string WebTS { get; set; }
        /// <summary>
        /// 转换次数
        /// </summary>
        public int WebConversion { get; set; }
        /// <summary>
        /// 外键(用户表)
        /// </summary>
        public virtual UserInfo UserInfo { get; set; }
        public virtual ICollection<VisitorInfo> VisitorInfo { get; set; }
        public virtual ICollection<FlowComputer> Flow { get; set; }
    }
}
