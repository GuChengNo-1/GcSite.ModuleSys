using GcSite.ModuleSys.DAL;
using GcSite.ModuleSys.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GcSite.ModuleSys.Controllers
{
    public class HomeController : Controller
    {
        WorkOfUnit work = new WorkOfUnit();
        GcSiteDb db = new GcSiteDb();
        public static int webId = 0;
        //[Route("~/Home/")]
        //[Route("~/Home/Index.html")]
        public ActionResult Index()
        {
            var userList = work.CreateRepository<UserInfo>().GetList(m => m.Id == 1).FirstOrDefault();
            var webList = work.CreateRepository<WebInfo>().GetList(m => m.UserInfo.Id == userList.Id).FirstOrDefault();
            ViewBag.host = webList;
            //当前日期
            DateTime current = DateTime.Now;
            var flow = GetVistiorByDay(current, webList.Id);
            ViewBag.flow = flow;
            //昨天日期
            DateTime preterite = DateTime.Now.AddDays(-1);
            var yesterday = GetVistiorByDay(preterite, webList.Id);
            ViewBag.preterite = yesterday;
            //预计今日
            #region 预计今日
            DateTime predictTime = DateTime.Now.AddDays(-2);
            var tempPredict = GetVistiorByDay(predictTime, webList.Id);
            WebInfo predict = new WebInfo()
            {
                WebPv = (int)Math.Ceiling(Convert.ToDouble((flow.WebPv + yesterday.WebPv) / 2)),
                WebUv = (int)Math.Ceiling(Convert.ToDouble((flow.WebUv + yesterday.WebUv) / 2)),
                IpCount = (int)Math.Ceiling(Convert.ToDouble((flow.IpCount + yesterday.IpCount) / 2))
            };
            #endregion
            ViewBag.predict = predict;
            return View();
        }
        //[Route("~/Home/")]
        //[Route("~/Home/report.html")]
        public ActionResult Report(int siteId)
        {
            webId = siteId;
            return View();
        }
        public ActionResult GetReportData()
        {
            try
            {
                var webList = work.CreateRepository<WebInfo>().GetList(m => m.Id == webId).Select(m => new
                {
                    Id = m.Id,
                    WebPv = m.WebPv,
                    WebUv = m.WebUv,
                    BounceRate = m.BounceRate,
                    IpCount = m.IpCount,
                    WebTS = m.WebTS,
                    WebDomain = m.WebDomain,
                    WebConversion = m.WebConversion,
                    WebKey = m.WebKey
                }).FirstOrDefault();
                ViewBag.host = webList;
                //总共 
                JsonSerializerSettings setting = new JsonSerializerSettings()
                {
                    //序列化
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.None
                };
                var total = webList;
                //当前日期
                DateTime current = DateTime.Now;
                var today = GetVistiorByDay(current, webList.Id);
                //昨天日期
                DateTime preterite = DateTime.Now.AddDays(-1);
                var yesterday = GetVistiorByDay(preterite, webList.Id);
                //昨日此时
                var atThisTime = GetVistiorByHours(preterite, webList.Id);
                //历史峰值
                var historical = GetVistiorByMax(webList.Id);

                #region 预计今日
                DateTime predictTime = DateTime.Now.AddDays(-2);
                var tempPredict = GetVistiorByDay(predictTime, webList.Id);
                WebInfo predict = new WebInfo()
                {
                    WebPv = (int)Math.Ceiling(Convert.ToDouble((today.WebPv + yesterday.WebPv) / 2)),
                    WebUv = (int)Math.Ceiling(Convert.ToDouble((today.WebUv + yesterday.WebUv) / 2)),
                    IpCount = (int)Math.Ceiling(Convert.ToDouble((today.IpCount + yesterday.IpCount) / 2))
                };
                #endregion

                #region 每日平均(计算条件:前三天)
                WebInfo average = new WebInfo();
                DateTime averageTime = DateTime.Now.AddDays(-3);
                var tempAverage = GetVistiorByDay(averageTime, webList.Id);
                average.WebPv = (int)Math.Ceiling(Convert.ToDouble((today.WebPv + yesterday.WebPv + tempAverage.WebPv) / 3));
                average.WebUv = (int)Math.Ceiling(Convert.ToDouble((today.WebUv + yesterday.WebUv + tempAverage.WebUv) / 3));
                average.IpCount = (int)Math.Ceiling(Convert.ToDouble((today.IpCount + yesterday.IpCount + tempAverage.IpCount) / 3));

                int number = int.Parse(today.BounceRate.Length <= 2 ? "0" : today.BounceRate.Substring(0, 3).Replace(".", "").Replace("%", ""));
                int number2 = int.Parse(yesterday.BounceRate.Length <= 2 ? "0" : yesterday.BounceRate.Substring(0, 3).Replace(".", "").Replace("%", ""));
                int number3 = int.Parse(tempAverage.BounceRate.Length <= 2 ? "0" : tempAverage.BounceRate.Substring(0, 3).Replace(".", "").Replace("%", ""));
                decimal rateResult = (int)Math.Ceiling(Convert.ToDouble((number + number2 + number3) / 3));
                rateResult = rateResult / 100;
                if (rateResult >= 1) { rateResult = 1; }
                average.BounceRate = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";

                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessEndTime, preterite) == 0 && m.WebInfo.Id == webList.Id).ToList();
                List<VisitorInfo> vistio2 = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessEndTime, predictTime) == 0 && m.WebInfo.Id == webList.Id).ToList();
                List<VisitorInfo> vistio3 = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessEndTime, averageTime) == 0 && m.WebInfo.Id == webList.Id).ToList();

                double a = vistio.Sum(m => m.Duration);
                double b = vistio2.Sum(m => m.Duration);
                double c = vistio3.Sum(m => m.Duration);
                TimeSpan ts = new TimeSpan(0, 0, (int)Math.Ceiling(Convert.ToDouble((a + b + c) / 3)));
                average.WebTS = ts.ToString();
                #endregion

                return Json(new { today, yesterday, total, atThisTime, historical, predict, average }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public ActionResult PostReportData(string date)
        {
            date = date == "" ? "0" : date;
            DateTime endTime = DateTime.Now.AddDays(double.Parse(date));
            //来源网站计算
            var sourceSite = SourceSiteByDay(endTime, date, webId);
            return Json(new { mages = "this is temp test", sourceSite },JsonRequestBehavior.AllowGet);
        }
        //[Route("~/Home/")]
        //[Route("~/Home/analysis.html")]
        public ActionResult Analysis()
        {
            return View();
        }
        /// <summary>
        /// 根据时间计算来源网站(数量,占比)
        /// </summary>
        /// <param name="endTime"></param>
        /// <param name="date"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public List<FlowComputer> SourceSiteByDay(DateTime endTime, string date, int site)
        {
            List<FlowComputer> flow = new List<FlowComputer>();

            try
            {
                List<FlowComputer> vistio = null;
                if (date == "0" || date == "-1")
                {
                    date = date == "0" ? "0" : date;
                    endTime = DateTime.Now.AddDays(double.Parse(date));
                    vistio = work.CreateRepository<FlowComputer>().GetList(m => DbFunctions.DiffDays(m.CurrentTime, endTime) == 0 && m.WebInfo.Id == site).ToList();
                }
                else
                {
                    vistio = work.CreateRepository<FlowComputer>().GetList(m => DbFunctions.DiffDays(m.CurrentTime, endTime) <= 0 && m.WebInfo.Id == site).ToList();
                }
                if (vistio.Count(m => m.VisitSE.Contains("谷歌") || m.VisitSE.Contains("Google")) > 0)
                {
                    FlowComputer gk = new FlowComputer();
                    gk.VisitSE = "谷歌";
                    gk.Id = vistio.Count(m => m.VisitSE.Contains("谷歌") || m.VisitSE.Contains("Google"));
                    decimal rateResult = Math.Round((decimal)gk.Id / vistio.Count, 4);
                    if (rateResult >= 1) { rateResult = 1; }
                    gk.SearchTerms = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                    flow.Add(gk);
                }
                if (vistio.Count(m => m.VisitSE.Contains("火狐")) > 0)
                {
                    FlowComputer hf = new FlowComputer();
                    hf.VisitSE = "火狐";
                    hf.Id = vistio.Count(m => m.VisitSE == "火狐");
                    decimal rateResult = Math.Round((decimal)hf.Id / vistio.Count, 4);
                    if (rateResult >= 1) { rateResult = 1; }
                    hf.SearchTerms = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                    flow.Add(hf);
                }
                if (vistio.Count(m => m.VisitSE.Contains("苹果")) > 0)
                {
                    FlowComputer pg = new FlowComputer();
                    pg.VisitSE = "苹果";
                    pg.Id = vistio.Count(m => m.VisitSE.Contains("苹果"));
                    decimal rateResult = Math.Round((decimal)pg.Id / vistio.Count, 4);
                    if (rateResult >= 1) { rateResult = 1; }
                    pg.SearchTerms = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                    flow.Add(pg);
                }
                if (vistio.Count(m => m.VisitSE.Contains("Opera")) > 0)
                {
                    FlowComputer opera = new FlowComputer();
                    opera.VisitSE = "Opera";
                    opera.Id = vistio.Count(m => m.VisitSE.Contains("Opera"));
                    decimal rateResult = Math.Round((decimal)opera.Id / vistio.Count, 4);
                    if (rateResult >= 1) { rateResult = 1; }
                    opera.SearchTerms = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                    flow.Add(opera);
                }
                if (vistio.Count(m => m.VisitSE.Contains("猎豹")) > 0)
                {
                    FlowComputer lb = new FlowComputer();
                    lb.VisitSE = "猎豹";
                    lb.Id = vistio.Count(m => m.VisitSE.Contains("猎豹"));
                    decimal rateResult = Math.Round((decimal)lb.Id / vistio.Count, 4);
                    if (rateResult >= 1) { rateResult = 1; }
                    lb.SearchTerms = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                    flow.Add(lb);
                }
                if (vistio.Count(m => m.VisitSE.Contains("搜狗") || m.VisitSE.Contains("Sogou")) > 0)
                {
                    FlowComputer sg = new FlowComputer();
                    sg.VisitSE = "搜狗";
                    sg.Id = vistio.Count(m => m.VisitSE.Contains("搜狗") || m.VisitSE.Contains("Sogou"));
                    decimal rateResult = Math.Round((decimal)sg.Id / vistio.Count, 4);
                    if (rateResult >= 1) { rateResult = 1; }
                    sg.SearchTerms = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                    flow.Add(sg);
                }
                if (vistio.Count(m => m.VisitSE.Contains("傲游")) > 0)
                {
                    FlowComputer ay = new FlowComputer();
                    ay.VisitSE = "傲游";
                    ay.Id = vistio.Count(m => m.VisitSE.Contains("傲游"));
                    decimal rateResult = Math.Round((decimal)ay.Id / vistio.Count, 4);
                    if (rateResult >= 1) { rateResult = 1; }
                    ay.SearchTerms = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                    flow.Add(ay);
                }
                if (vistio.Count(m => m.VisitSE.Contains("未知")) > 0)
                {
                    FlowComputer qt = new FlowComputer();
                    qt.VisitSE = "其他";
                    qt.Id = vistio.Count(m => m.VisitSE.Contains("未知"));
                    decimal rateResult = Math.Round((decimal)qt.Id / vistio.Count, 4);
                    if (rateResult >= 1) { rateResult = 1; }
                    qt.SearchTerms = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                    flow.Add(qt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flow;
        }
        public List<FlowComputer> PageviewByDay(DateTime endTime, string date, int site)
        {
            List<FlowComputer> flow = new List<FlowComputer>();

            try
            {
                List<FlowComputer> vistio = null;
                if (date == "0" || date == "-1")
                {
                    date = date == "0" ? "0" : date;
                    endTime = DateTime.Now.AddDays(double.Parse(date));
                    vistio = work.CreateRepository<FlowComputer>().GetList(m => DbFunctions.DiffDays(m.CurrentTime, endTime) == 0 && m.WebInfo.Id == site).ToList();
                }
                else
                {
                    vistio = work.CreateRepository<FlowComputer>().GetList(m => DbFunctions.DiffDays(m.CurrentTime, endTime) <= 0 && m.WebInfo.Id == site).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return flow;
        }
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
                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site).ToList();
                //获取pv量,根据条件时间&网站id
                web.WebPv = work.CreateRepository<FlowComputer>().GetList(m => DbFunctions.DiffDays(m.CurrentTime, startTime) == 0 && m.WebInfo.Id == site).Count();
                //获取uv值,根据条件时间&网站id
                web.WebUv = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site).Count();
                //获取ip数 去重
                List<VisitorInfo> webuv = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site).ToList();
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
                int rate = work.CreateRepository<VisitorInfo>().GetCount(m => m.PageNumber == 0 && DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                int sumPv = work.CreateRepository<WebInfo>().GetList(m => m.UserInfo.Id == site).FirstOrDefault().WebPv;
                decimal rateResult = Math.Round((decimal)rate / web.WebPv, 4);
                if (rateResult >= 1) { rateResult = 1; }
                web.BounceRate = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                //计算转换次数,根据条件时间&网站id
                web.WebConversion = 0;
                //计算平均时长,根据条件时间&网站id
                int sumDuration = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                double duration = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
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
                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site).ToList();
                //获取pv量,根据条件时间&网站id
                web.WebPv = work.CreateRepository<FlowComputer>().GetList(m => DbFunctions.DiffHours(m.CurrentTime, startTime) == 0 && m.WebInfo.Id == site).Count();
                //获取uv值,根据条件时间&网站id
                web.WebUv = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site).Count();
                //获取ip数 去重
                List<VisitorInfo> webuv = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site).ToList();
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
                int rate = work.CreateRepository<VisitorInfo>().GetCount(m => m.PageNumber == 0 && DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                int sumPv = work.CreateRepository<WebInfo>().GetList(m => m.Id == site).FirstOrDefault().WebPv;
                decimal rateResult = Math.Round((decimal)rate / web.WebPv, 4);
                if (rateResult >= 1) { rateResult = 1; }
                web.BounceRate = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                //计算转换次数,根据条件时间&网站id
                web.WebConversion = 0;
                //计算平均时长,根据条件时间&网站id
                int sumDuration = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
                double duration = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffHours(m.AccessTime, startTime) == 0 && m.WebInfo.Id == site);
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
        /// 根据历史巅峰计算统计流量
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public WebInfo GetVistiorByMax(int site)
        {
            WebInfo web = new WebInfo();
            try
            {
                var whereMax = work.CreateRepository<FlowComputer>().GetList(m => m.WebInfo.Id == site).OrderBy(m => m.CurrentTime).FirstOrDefault();
                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(m => m.WebInfo.Id == site && DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0).ToList();
                //获取pv量,根据条件时间&网站id
                web.WebPv = work.CreateRepository<FlowComputer>().GetList(m => m.WebInfo.Id == site).OrderBy(m => m.CurrentTime).Count();
                web.WebDomain = "" + whereMax.CurrentTime.ToShortDateString();
                //获取uv值,根据条件时间&网站id
                web.WebUv = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site).Count();
                //获取ip数 去重
                List<VisitorInfo> webuv = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site).ToList();
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
                int rate = work.CreateRepository<VisitorInfo>().GetCount(m => m.PageNumber == 0 && DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site);
                int sumPv = work.CreateRepository<WebInfo>().GetList(m => m.UserInfo.Id == site).FirstOrDefault().WebPv;
                decimal rateResult = Math.Round((decimal)rate / web.WebPv, 4);
                if (rateResult >= 1) { rateResult = 1; }
                web.BounceRate = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                //计算转换次数,根据条件时间&网站id
                web.WebConversion = 0;
                //计算平均时长,根据条件时间&网站id
                int sumDuration = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site);
                double duration = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site);
                duration = vistio.Sum(a => a.Duration);
                int averageTime = (int)Math.Ceiling(Convert.ToDouble(duration / sumDuration));
                TimeSpan ts = new TimeSpan(0, 0, averageTime);
                web.WebTS = ts.ToString();

                if (web.WebTS.Contains("0-3:0-14:0-8")) { web.WebTS = "00:00:00"; }
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
        /// 根据每天平均计算统计流量
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public WebInfo GetVistiorByDayMean(int site)
        {
            var whereMax = work.CreateRepository<FlowComputer>().GetList(m => m.WebInfo.Id == site).OrderBy(m => m.CurrentTime).ToList();
            DateTime time1 = DateTime.Now.AddDays(-1);
            var vistior1 = GetVistiorByDay(time1, site);
            DateTime time2 = DateTime.Now.AddDays(-2);
            var vistior2 = GetVistiorByDay(time1, site);
            DateTime time3 = DateTime.Now.AddDays(-3);
            var vistior3 = GetVistiorByDay(time1, site);
            DateTime time4 = DateTime.Now.AddDays(-4);
            var vistior4 = GetVistiorByDay(time1, site);
            DateTime time5 = DateTime.Now.AddDays(-5);
            var vistior5 = GetVistiorByDay(time1, site);
            whereMax.Sum(m => m.Id);
            WebInfo web = new WebInfo();
            return web;

        }
        public string TimeConvert(TimeSpan time_distance)
        {
            // 天时分秒换算 
            string int_hour = time_distance.Hours.ToString();

            string int_minute = time_distance.Minutes.ToString();

            string int_second = time_distance.Seconds.ToString();
            // 时分秒为单数时、前面加零 
            if (int.Parse(int_hour) < 10)
            {
                int_hour = "0" + int_hour;
            }
            if (int.Parse(int_minute) < 10)
            {
                int_minute = "0" + int_minute;
            }
            if (int.Parse(int_second) < 10)
            {
                int_second = "0" + int_second;
            }
            return int_hour + ":" + int_minute + ":" + int_second;
        }
    }
}