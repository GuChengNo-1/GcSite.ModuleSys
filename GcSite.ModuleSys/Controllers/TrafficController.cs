using GcSite.ModuleSys.BLL;
using GcSite.ModuleSys.DAL;
using GcSite.ModuleSys.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GcSite.ModuleSys.Controllers
{
    public class TrafficController : Controller
    {
        WorkOfUnit work = new WorkOfUnit();
        GcSiteDb db = new GcSiteDb();
        public static int webId = 0;
        public static UserInfo user = null;
        // GET: Trend
        public ActionResult Visitors(int siteId)
        {
            webId = siteId;
            return View();
        }
        public ActionResult Trend(int siteId)
        {
            webId = siteId;
            return View();
        }

        [HttpGet]
        //public ActionResult GetReportData(int day)
        //{
        //    try
        //    {
        //        var webList = work.CreateRepository<WebInfo>().GetList(m => m.Id == webId).Select(m => new
        //        {
        //            Id = m.Id,
        //            WebPv = m.WebPv,
        //            WebUv = m.WebUv,
        //            BounceRate = m.BounceRate,
        //            IpCount = m.IpCount,
        //            WebTS = m.WebTS,
        //            WebDomain = m.WebDomain,
        //            WebConversion = m.WebConversion,
        //            WebKey = m.WebKey
        //        }).FirstOrDefault();
        //        //当前日期
        //        DateTime current = DateTime.Now.AddDays(day);
        //        var today = GetVistiorByDay(current, webList.Id);
        //        //昨天日期
        //        DateTime preterite = DateTime.Now.AddDays(day);
        //        var yesterday = GetVistiorByDay(preterite, webList.Id);

        //        return Json(new { today, yesterday, siteId = webId.ToString(), users = user.UserName.ToString() }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}

        /// <summary>
        /// 根据指定日期计算统计流量
        /// </summary>
        /// <param name="startTime">设置时间</param>
        /// <param name="site">网站id</param>
        /// <returns></returns>
        public WebInfo GetVistiorByDay(DateTime startTime, int site)
        {
            WebInfo web = new WebInfo();
            try
            {
                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site
                    ).ToList();
                //获取pv量,根据条件时间&网站id
                web.WebPv = work.CreateRepository<FlowComputer>().GetList(
                    m => DbFunctions.DiffDays(m.CurrentTime, startTime) == 0 && m.WebInfo.Id == site
                    ).Count();
                //获取uv值,根据条件时间&网站id
                web.WebUv = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site
                    ).Count();
                //获取ip数 去重
                List<VisitorInfo> webuv = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site
                    ).ToList();
                for (int i = 0; i < webuv.Count(); i++)
                {
                    for (int j = webuv.Count() - 1; j > i; j--)
                    {
                        if (webuv[i].IpAddress == webuv[j].IpAddress)
                        {
                            webuv.RemoveAt(j);
                        }
                    }
                }
                web.IpCount = webuv.Count;
                //计算跳出率,根据条件时间&网站id
                int rate = work.CreateRepository<VisitorInfo>().GetCount(
                    m => m.PageNumber == 0 && DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                //int sumPv = work.CreateRepository<WebInfo>().GetList(m => m.UserInfo.Id == site).FirstOrDefault().WebPv;
                //int sumUv = work.CreateRepository<VisitorInfo>().GetCount(
                //    m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                decimal rateResult = Math.Round((decimal)rate / web.WebPv, 4);
                if (rateResult >= 1) { rateResult = 1; }
                web.BounceRate = (rateResult * 100).ToString().Length >= 5 ?
                    (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                //计算转换次数,根据条件时间&网站id
                web.WebConversion = 0;
                //计算平均时长,根据条件时间&网站id
                int sumDuration = work.CreateRepository<VisitorInfo>().GetCount(
                    m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                double duration = work.CreateRepository<VisitorInfo>().GetCount(
                    m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                duration = vistio.Sum(a => a.Duration);
                int averageTime = (int)Math.Ceiling(Convert.ToDouble(duration / sumDuration));
                TimeSpan ts = new TimeSpan(0, 0, averageTime);
                web.WebTS = ts.ToString();

                if (web.WebTS.Contains("-24855.03:14:08")) { web.WebTS = "00:00:00"; }
            }
            catch (Exception ex)
            {
                web.IpCount = 0;
                web.WebConversion = 0;
                web.WebDomain = "";
                web.WebPv = 0;
                web.WebTS = "00:00:00";
                web.WebUv = 0;
                web.BounceRate = "0%";
                return web;
                throw ex;
            }
            return web;
        }

        public ActionResult VisitorsDetail(int page, int limit, string IC)
        {
            var ICValue = Request.Params["keywordValue"];
            var flowList = work.CreateRepository<VisitorInfo>().GetPageList(m => m.WebInfo.Id == webId, "", page, limit);
            var temp = flowList.Select(m => new
            {
                //最近访问时间
                AccessTime = m.AccessTime,
                //IP地域
                Address = m.Address.Replace("市", "").Trim(),
                //IP地址
                IpAddress = m.IpAddress,
                //平均访问页数
                VisitPage = m.PageNumber + 1,
                //Pv
                Pv = work.CreateRepository<FlowComputer>().GetList(
                    p => DbFunctions.DiffDays(p.CurrentTime, m.AccessTime) == 0 && p.WebInfo.Id == webId).Count(),
                //访客数（UV）
                Age = work.CreateRepository<FlowComputer>().GetList(p => DbFunctions.DiffDays(p.CurrentTime, m.AccessEndTime) == 0).Count(),
                //Duration = m.Duration.ToString("0"),
                //平均访问时长
                Duration = (m.Duration / work.CreateRepository<FlowComputer>().GetList(p => DbFunctions.DiffDays(p.CurrentTime, m.AccessEndTime) == 0).Count()).ToString("0"),
                //访问次数
                Key = work.CreateRepository<VisitorInfo>().GetList(
                    p => DbFunctions.DiffDays(p.AccessTime, m.AccessTime) == 0 && m.WebInfo.Id == webId).Count(),
                //访客标识码
                IC = m.IC
            }).ToList();
            var count = work.CreateRepository<VisitorInfo>().GetCount(m => m.WebInfo.Id == webId);
            if (IC != null)
            {
                if (IC != null && IC.ToString().Contains("."))
                {
                    flowList = work.CreateRepository<VisitorInfo>().GetPageList(m => m.WebInfo.Id == webId && m.IpAddress == IC, "", page, limit);
                    temp = flowList.Select(m => new
                    {
                        //最近访问时间
                        AccessTime = m.AccessTime,
                        //IP地域
                        Address = m.Address.Replace("市", "").Trim(),
                        //IP地址
                        IpAddress = m.IpAddress,
                        //平均访问页数
                        VisitPage = m.PageNumber + 1,
                        //Pv
                        Pv = work.CreateRepository<FlowComputer>().GetList(
                            p => DbFunctions.DiffDays(p.CurrentTime, m.AccessTime) == 0 && p.WebInfo.Id == webId && m.IpAddress == IC).Count(),
                        //访客数（UV）
                        Age = work.CreateRepository<FlowComputer>().GetList(p => DbFunctions.DiffDays(p.CurrentTime, m.AccessEndTime) == 0 && m.IpAddress == IC).Count(),
                        //Duration = m.Duration.ToString("0"),
                        //平均访问时长
                        Duration = (m.Duration / work.CreateRepository<FlowComputer>().GetList(p => DbFunctions.DiffDays(p.CurrentTime, m.AccessEndTime) == 0 && m.IpAddress == IC).Count()).ToString("0"),
                        //访问次数
                        Key = work.CreateRepository<VisitorInfo>().GetList(
                            p => DbFunctions.DiffDays(p.AccessTime, m.AccessTime) == 0 && m.WebInfo.Id == webId && m.IpAddress == IC).Count(),
                        //访客标识码
                        IC = m.IC
                    }).ToList();
                    count = work.CreateRepository<VisitorInfo>().GetCount(m => m.WebInfo.Id == webId && m.IpAddress == IC);
                }
                else
                {
                    flowList = work.CreateRepository<VisitorInfo>().GetPageList(m => m.WebInfo.Id == webId && m.IC == IC, "", page, limit);
                    temp = flowList.Select(m => new
                    {
                        //最近访问时间
                        AccessTime = m.AccessTime,
                        //IP地域
                        Address = m.Address.Replace("市", "").Trim(),
                        //IP地址
                        IpAddress = m.IpAddress,
                        //平均访问页数
                        VisitPage = m.PageNumber + 1,
                        //Pv
                        Pv = work.CreateRepository<FlowComputer>().GetList(
                            p => DbFunctions.DiffDays(p.CurrentTime, m.AccessTime) == 0 && p.WebInfo.Id == webId && m.IpAddress == IC).Count(),
                        //访客数（UV）
                        Age = work.CreateRepository<FlowComputer>().GetList(p => DbFunctions.DiffDays(p.CurrentTime, m.AccessEndTime) == 0 && m.IpAddress == IC).Count(),
                        //Duration = m.Duration.ToString("0"),
                        //平均访问时长
                        Duration = (m.Duration / work.CreateRepository<FlowComputer>().GetList(p => DbFunctions.DiffDays(p.CurrentTime, m.AccessEndTime) == 0 && m.IC == IC).Count()).ToString("0"),
                        //访问次数
                        Key = work.CreateRepository<VisitorInfo>().GetList(
                            p => DbFunctions.DiffDays(p.AccessTime, m.AccessTime) == 0 && m.WebInfo.Id == webId && m.IpAddress == IC).Count(),
                        //访客标识码
                        IC = m.IC
                    }).ToList();
                    count = work.CreateRepository<VisitorInfo>().GetCount(m => m.WebInfo.Id == webId && m.IC == IC);
                }
            }
            return Json(new { code = 0, msg = "", count = count, data = temp }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 根据指定时间段计算统计流量
        /// </summary>
        /// <param name="startTime">设置时间</param>
        /// <param name="site">网站id</param>
        /// <returns></returns>
        public WebInfo GetVistiorByHours(DateTime startTime, int site)
        {
            WebInfo web = new WebInfo();
            try
            {
                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site
                    ).ToList();
                //获取pv量,根据条件时间&网站id
                web.WebPv = work.CreateRepository<FlowComputer>().GetList(
                    m => DbFunctions.DiffHours(m.CurrentTime, startTime) == 0 && m.WebInfo.Id == site
                    ).Count();
                //获取uv值,根据条件时间&网站id
                web.WebUv = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site
                    ).Count();
                //获取ip数 去重
                List<VisitorInfo> webuv = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site
                    ).ToList();
                for (int i = 0; i < webuv.Count(); i++)
                {
                    for (int j = webuv.Count() - 1; j > i; j--)
                    {
                        if (webuv[i].IpAddress == webuv[j].IpAddress)
                        {
                            webuv.RemoveAt(j);
                        }
                    }
                }
                web.IpCount = webuv.Count;
                //计算跳出率,根据条件时间&网站id
                int rate = work.CreateRepository<VisitorInfo>().GetCount(
                    m => m.PageNumber == 0 && DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                //int sumPv = work.CreateRepository<WebInfo>().GetList(m => m.Id == site).FirstOrDefault().WebPv;
                //int sumPv = work.CreateRepository<WebInfo>().GetList(m => m.UserInfo.Id == site).FirstOrDefault().WebPv;
                //int sumUv = work.CreateRepository<VisitorInfo>().GetCount(
                //    m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                decimal rateResult = Math.Round((decimal)rate / web.WebPv, 4);
                if (rateResult >= 1) { rateResult = 1; }
                web.BounceRate = (rateResult * 100).ToString().Length >= 5 ?
                    (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                //计算转换次数,根据条件时间&网站id
                web.WebConversion = 0;
                //计算平均时长,根据条件时间&网站id
                int sumDuration = work.CreateRepository<VisitorInfo>().GetCount(
                    m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                double duration = work.CreateRepository<VisitorInfo>().GetCount(
                    m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                duration = vistio.Sum(a => a.Duration);
                int averageTime = (int)Math.Ceiling(Convert.ToDouble(duration / sumDuration));
                TimeSpan ts = new TimeSpan(0, 0, averageTime);
                web.WebTS = ts.ToString();
                if (web.WebTS.Contains("-24855.03:14:08")) { web.WebTS = "00:00:00"; }
            }
            catch (Exception ex)
            {
                web.IpCount = 0;
                web.WebConversion = 0;
                web.WebDomain = "";
                web.WebPv = 0;
                web.WebTS = "00:00:00";
                web.WebUv = 0;
                web.BounceRate = "0%";
                return web;
                throw ex;
            }
            return web;
        }
        /// <summary>
        /// 趋势图计算
        /// </summary>
        /// <param name="date"></param>
        /// <param name="type"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public List<WebInfo> TendencyChartByType(DateTime endTime, string date, int site)
        {
            List<WebInfo> flow = new List<WebInfo>();

            try
            {
                //今  昨
                if (date == "0" || date == "-1")
                {
                    date = date == "0" ? "0" : date;
                    endTime = DateTime.Now.AddDays(double.Parse(date));
                    //获取当天所有时间段的条件数据
                    for (int i = 0; i < 24; i++)
                    {
                        DateTime temp = new DateTime(endTime.Year, endTime.Month, endTime.Day, i, 0, 0);
                        int tag = temp.Hour;
                        WebInfo web = GetVistiorByHours(temp, site);
                        TimeSpan ts = new TimeSpan(temp.Hour, 0, 0);
                        web.WebDomain = ts.ToString();
                        web.Id = tag;
                        web.BounceRate = web.BounceRate.Replace("%", "");
                        DateTime dt = Convert.ToDateTime(web.WebTS);
                        int time = (Convert.ToInt32(dt.Hour) * 3600) + (Convert.ToInt32(dt.Minute) * 60) + Convert.ToInt32(dt.Second);
                        web.WebTS = time.ToString();
                        flow.Add(web);
                    }
                    flow = flow.OrderBy(m => m.Id).ToList();
                }
                //最近七天or最近三十天
                else
                {
                    int whereDay = int.Parse(date.Replace("-", ""));
                    for (int i = 0; i < whereDay + 1; i++)
                    {
                        endTime = DateTime.Now.AddDays(-i);
                        WebInfo web = GetVistiorByDay(endTime, site);
                        web.WebDomain = endTime.Date.ToShortDateString();
                        web.BounceRate = web.BounceRate.Replace("%", "");
                        string s = ConvertTime(web.WebTS);
                        DateTime dt = Convert.ToDateTime(web.WebTS);
                        int time = (Convert.ToInt32(dt.Hour) * 3600) + (Convert.ToInt32(dt.Minute) * 60) + Convert.ToInt32(dt.Second);
                        web.WebTS = time.ToString();
                        flow.Add(web);
                    }
                    flow = flow.OrderBy(m => DateTime.Parse(m.WebDomain)).ToList();
                }
            }
            catch (Exception ex)
            {
                return flow = null;
                throw ex;
            }
            return flow;
        }
        public ActionResult PostChart(string date)
        {
            date = date == "" ? "0" : date;
            DateTime endTime = DateTime.Now.AddDays(double.Parse(date));
            //趋势图
            var chart = TendencyChartByType(endTime, date, webId);
            return Json(new { chart }, JsonRequestBehavior.AllowGet);
        }
        public string ConvertTime(string time)
        {
            var str = time;
            var arr = str.Split(':');
            var hs = int.Parse(arr[0]) * 3600;
            var ms = int.Parse(arr[1]) * 60;
            var ss = int.Parse(arr[2]);
            var seconds = hs + ms + ss;
            return seconds.ToString();
        }
    }
}