using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GcSite.ModuleSys.Models;
using GcSite.ModuleSys.DAL;

namespace GcSite.ModuleSys.BLL
{
    public class Visitors_BLL
    {
        //实例化数据库模型
        GcSiteDb db = new GcSiteDb();
        WorkOfUnit work = new WorkOfUnit();
        public static int webId = 0;
        public static UserInfo user = null;
        //查询访客明细
        public List<VisitorInfo> VisitorDetail(int siteId)
        {
            List<VisitorInfo> list = new List<VisitorInfo>();
            var visitor = work.CreateRepository<VisitorInfo>().GetPageList(p => p.WebInfo.Id == siteId).ToList();
            return visitor;
        }
        //根据IC访客标识码查询访客明细
        public List<VisitorInfo> VisitorDetail(int siteId, string IC)
        {
            List<VisitorInfo> list = new List<VisitorInfo>();
            var visitor = db.VisitorInfo.ToList().Where(p => p.Id == siteId && p.IC.Contains(IC));
            return list;
        }
        //查询访客明细
        public List<VisitorInfo> IPDetail(int siteId)
        {
            List<VisitorInfo> list = new List<VisitorInfo>();
            var visitor = db.VisitorInfo.ToList().Where(p => p.Id == siteId);
            return list;
        }
        //根据IC访客标识码查询访客明细
        public List<VisitorInfo> IPDetail(int siteId, string IP)
        {
            List<VisitorInfo> list = new List<VisitorInfo>();
            var visitor = db.VisitorInfo.ToList().Where(p => p.Id == siteId && p.IC.Contains(IP));
            return list;
        }
    }
}
