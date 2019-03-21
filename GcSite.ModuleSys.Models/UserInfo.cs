using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GcSite.ModuleSys.Models
{
    public class UserInfo:EntityBase
    {
        public string UserName { get; set; }
        public string UserPwd { get; set; }
        public string UserPhone { get; set; }
        public virtual ICollection<WebInfo> WebInfo { get; set; }
    }
}
