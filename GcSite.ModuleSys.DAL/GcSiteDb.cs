using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using GcSite.ModuleSys.Models;

namespace GcSite.ModuleSys.DAL
{
    public class GcSiteDb:DbContext
    {
        public GcSiteDb()
            :base("connStr")
        {
            
        }
        public IDbSet<UserInfo> UserInfo { get; set; }

        public IDbSet<WebInfo> WebInfo { get; set; }

        public IDbSet<VisitorInfo> VisitorInfo { get; set; }
        public IDbSet<FlowComputer> FlowComputer { get; set; }
    }
}
