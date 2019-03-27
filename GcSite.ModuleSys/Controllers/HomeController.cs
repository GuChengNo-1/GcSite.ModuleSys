using GcSite.ModuleSys.common;
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
        public static UserInfo user = null;
        //[Route("~/Home/")]
        //[Route("~/Home/Index.html")]
        public ActionResult Index(int userId)
        {
            user = work.CreateRepository<UserInfo>().GetList(m => m.Id == userId).FirstOrDefault();
            var webList = work.CreateRepository<WebInfo>().GetList(m => m.UserInfo.Id == user.Id).FirstOrDefault();
            ViewBag.user = user;
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
            var model = db.UserInfo.ToList().Where(p => p.Id == userId);
            string name = "";
            foreach (var item in model)
            {
                name = item.UserName;
            }
            ViewBag.name = name;
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
                //总共 
                JsonSerializerSettings setting = new JsonSerializerSettings()
                {
                    //序列化
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.None
                };
                //var users = JsonConvert.SerializeObject(user, Formatting.Indented);
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

                int number = int.Parse(today.BounceRate.Length <= 2 ?
                    "0" : today.BounceRate.Substring(0, 3).Replace(".", "").Replace("%", ""));
                int number2 = int.Parse(yesterday.BounceRate.Length <= 2 ?
                    "0" : yesterday.BounceRate.Substring(0, 3).Replace(".", "").Replace("%", ""));
                int number3 = int.Parse(tempAverage.BounceRate.Length <= 2 ?
                    "0" : tempAverage.BounceRate.Substring(0, 3).Replace(".", "").Replace("%", ""));
                decimal rateResult = (int)Math.Ceiling(Convert.ToDouble((number + number2 + number3) / 3));
                rateResult = rateResult / 100;
                if (rateResult >= 1) { rateResult = 1; }
                average.BounceRate = (rateResult * 100).ToString().Length >= 5 ?
                    (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";

                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffDays(m.AccessEndTime, preterite) == 0 && m.WebInfo.Id == webList.Id
                    ).ToList();
                List<VisitorInfo> vistio2 = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffDays(m.AccessEndTime, predictTime) == 0 && m.WebInfo.Id == webList.Id
                    ).ToList();
                List<VisitorInfo> vistio3 = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffDays(m.AccessEndTime, averageTime) == 0 && m.WebInfo.Id == webList.Id
                    ).ToList();

                double a = vistio.Sum(m => m.Duration);
                double b = vistio2.Sum(m => m.Duration);
                double c = vistio3.Sum(m => m.Duration);
                TimeSpan ts = new TimeSpan(0, 0, (int)Math.Ceiling(Convert.ToDouble((a + b + c) / 3)));
                average.WebTS = ts.ToString();
                #endregion

                return Json(new {today, yesterday, total, atThisTime, historical, predict, average , siteId = webId.ToString(),users = user.UserName.ToString()}, JsonRequestBehavior.AllowGet);
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
            //关键词计算
            var keyword = KeywordByDay(endTime, date, webId);
            //地域分布
            var territory = TerritoryByDay(endTime, date, webId);
            //网络设备
            var networkDevice = NetworkDeviceByDay(endTime, date, webId);
            //Top10受访页面
            var TopPageView = PageviewByDay(endTime, date, webId);
            //入口页面
            var EntryPage = EntryPageByDay(endTime, date, webId);
            //新老访客
            var Visiter = NewOldVisit(endTime, date, webId);
            return Json(new { sourceSite, keyword, territory, networkDevice, TopPageView, EntryPage, Visiter }, JsonRequestBehavior.AllowGet);
        }
        //[Route("~/Home/")]
        //[Route("~/Home/analysis.html")]
        public ActionResult Analysis()
        {
            return View();
        }

        public ActionResult PostChart(string date)
        {
            date = date == "" ? "0" : date;
            DateTime endTime = DateTime.Now.AddDays(double.Parse(date));
            //趋势图
            var chart = TendencyChartByType(endTime, date, webId);
            return Json(new { chart }, JsonRequestBehavior.AllowGet);
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
                List<FlowComputer> vistio = null;
                //今  昨
                if (date == "0" || date == "-1")
                {
                    date == "0" ? "0" : date;
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
        /// <summary>
        /// 根据时间计算地域分布(数量,占比)
        /// </summary>
        /// <param name="endTime"></param>
        /// <param name="date"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public List<FlowComputer> TerritoryByDay(DateTime endTime, string date, int site)
        {
            List<FlowComputer> flow = new List<FlowComputer>();

            try
            {
                List<FlowComputer> vistio = null;
                if (date == "0" || date == "-1")
                {
                    date = date == "0" ? "0" : date;
                    endTime = DateTime.Now.AddDays(double.Parse(date));
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) == 0 && m.WebInfo.Id == site && m.Address != ""
                        ).ToList();
                }
                else
                {
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) <= 0 && m.WebInfo.Id == site && m.Address != ""
                        ).ToList();
                }
                //数量&占比计算
                for (int i = 0; i < vistio.Count; i++)
                {
                    if (flow.Count == 0)
                    {
                        FlowComputer model = new FlowComputer();
                        model.Address = AddressHelper.FindObjectProvince(vistio[i].Address);
                        model.Id = vistio.Count(m => m.Address == vistio[i].Address);
                        decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                        if (rateResult >= 1) { rateResult = 1; }
                        model.VisitPage = (rateResult * 100).ToString().Length >= 5 ?
                            (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                        flow.Add(model);
                    }
                    else
                    {
                        for (int j = 0; j < flow.Count; j++)
                        {
                            if (vistio[i].Address.Contains("市"))
                            {
                                int index = vistio[i].Address.IndexOf("市");
                                vistio[i].Address = vistio[i].Address.Substring(0, index);

                            }
                            if (!(flow[j].Address.Trim().Contains(vistio[i].Address.Trim())))
                            {
                                FlowComputer model = new FlowComputer();
                                model.Address = AddressHelper.FindObjectProvince(vistio[i].Address);
                                model.Id = vistio.Count(m => m.Address == vistio[i].Address);
                                decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                                if (rateResult >= 1) { rateResult = 1; }
                                model.VisitPage = (rateResult * 100).ToString().Length >= 5 ?
                                    (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                                flow.Add(model);
                            }
                        }

                    }
                }
                //去除冗余数据
                for (int i = 0; i < flow.Count(); i++)
                {
                    for (int j = flow.Count() - 1; j > i; j--)
                    {
                        if (flow[i].Address == flow[j].Address)
                        {
                            flow.RemoveAt(j);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return flow = null;
                throw ex;
            }
            return flow.OrderByDescending(m => m.Id).ToList();
        }
        /// <summary>
        /// 根据时间计算网络设备(数量,占比)
        /// </summary>
        /// <param name="endTime"></param>
        /// <param name="date"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public List<FlowComputer> NetworkDeviceByDay(DateTime endTime, string date, int site)
        {
            List<FlowComputer> flow = new List<FlowComputer>();

            try
            {
                List<FlowComputer> vistio = null;
                if (date == "0" || date == "-1")
                {
                    date = date == "0" ? "0" : date;
                    endTime = DateTime.Now.AddDays(double.Parse(date));
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) == 0 && m.WebInfo.Id == site && m.DeviceType != "" &&
                        m.DeviceType != null).ToList();
                }
                else
                {
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) <= 0 && m.WebInfo.Id == site && m.DeviceType != "" &&
                        m.DeviceType != null).ToList();
                }
                //数量&占比计算
                for (int i = 0; i < vistio.Count; i++)
                {
                    if (flow.Count == 0)
                    {
                        FlowComputer model = new FlowComputer();
                        model.DeviceType = vistio[i].DeviceType;
                        model.Id = vistio.Count(m => m.DeviceType == vistio[i].DeviceType);
                        decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                        if (rateResult >= 1) { rateResult = 1; }
                        model.VisitPage = (rateResult * 100).ToString().Length >= 5 ?
                            (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                        flow.Add(model);
                    }
                    else
                    {
                        if (flow.Count <= 10)
                        {
                            for (int j = 0; j < flow.Count; j++)
                            {
                                if (flow[j].DeviceType.Trim() != vistio[i].DeviceType.Trim())
                                {
                                    FlowComputer model = new FlowComputer();
                                    model.DeviceType = vistio[i].DeviceType;
                                    model.Id = vistio.Count(m => m.DeviceType == vistio[i].DeviceType);
                                    decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                                    if (rateResult >= 1) { rateResult = 1; }
                                    model.VisitPage = (rateResult * 100).ToString().Length >= 5 ?
                                        (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                                    flow.Add(model);
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < flow.Count; i++)
                {
                    if (!flow[i].DeviceType.Contains("iPad"))
                    {
                        FlowComputer model = new FlowComputer();
                        model.DeviceType = "iPad";
                        model.Id = 0;
                        model.VisitPage = "0%";
                        flow.Add(model);
                    }
                    if (!flow[i].DeviceType.Contains("iPhone"))
                    {
                        FlowComputer model = new FlowComputer();
                        model.DeviceType = "iPhone";
                        model.Id = 0;
                        model.VisitPage = "0%";
                        flow.Add(model);
                    }
                    if (!flow[i].DeviceType.Contains("PC"))
                    {
                        FlowComputer model = new FlowComputer();
                        model.DeviceType = "PC";
                        model.Id = 0;
                        model.VisitPage = "0%";
                        flow.Add(model);
                    }
                    break;
                }
                //去除冗余数据
                for (int i = 0; i < flow.Count(); i++)
                {
                    for (int j = flow.Count() - 1; j > i; j--)
                    {
                        if (flow[i].DeviceType == flow[j].DeviceType)
                        {
                            flow.RemoveAt(j);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return flow = null;
                throw ex;
            }
            return flow.OrderByDescending(m => m.Id).ToList();
        }
        /// <summary>
        /// 根据时间计算关键词(数量,占比)
        /// </summary>
        /// <param name="endTime"></param>
        /// <param name="date"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public List<FlowComputer> KeywordByDay(DateTime endTime, string date, int site)
        {
            List<FlowComputer> flow = new List<FlowComputer>();

            try
            {
                List<FlowComputer> vistio = null;
                if (date == "0" || date == "-1")
                {
                    date = date == "0" ? "0" : date;
                    endTime = DateTime.Now.AddDays(double.Parse(date));
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) == 0 && m.WebInfo.Id == site && m.SearchTerms != ""
                        ).ToList();
                }
                else
                {
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) <= 0 && m.WebInfo.Id == site && m.SearchTerms != ""
                        ).ToList();
                }
                //数量&占比计算
                for (int i = 0; i < vistio.Count; i++)
                {
                    if (flow.Count == 0)
                    {
                        FlowComputer model = new FlowComputer();
                        model.SearchTerms = vistio[i].SearchTerms;
                        model.Id = vistio.Count(m => m.SearchTerms == vistio[i].SearchTerms);
                        decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                        if (rateResult >= 1) { rateResult = 1; }
                        model.VisitPage = (rateResult * 100).ToString().Length >= 5 ?
                            (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                        flow.Add(model);
                    }
                    else
                    {
                        if (flow.Count <= 10)
                        {
                            for (int j = 0; j < flow.Count; j++)
                            {
                                if (flow[j].SearchTerms.Trim() != vistio[i].SearchTerms.Trim())
                                {
                                    FlowComputer model = new FlowComputer();
                                    model.SearchTerms = vistio[i].SearchTerms;
                                    model.Id = vistio.Count(m => m.SearchTerms == vistio[i].SearchTerms);
                                    decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                                    if (rateResult >= 1) { rateResult = 1; }
                                    model.VisitPage = (rateResult * 100).ToString().Length >= 5 ?
                                        (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                                    flow.Add(model);
                                }
                            }
                        }
                    }
                }
                //去除冗余数据
                for (int i = 0; i < flow.Count(); i++)
                {
                    for (int j = flow.Count() - 1; j > i; j--)
                    {
                        if (flow[i].SearchTerms == flow[j].SearchTerms)
                        {
                            flow.RemoveAt(j);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return flow = null;
                throw ex;
            }
            return flow.OrderByDescending(m => m.Id).ToList();
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
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) == 0 && m.WebInfo.Id == site
                        ).ToList();
                }
                else
                {
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) <= 0 && m.WebInfo.Id == site
                        ).ToList();
                }
                //数量&占比计算
                for (int i = 0; i < vistio.Count; i++)
                {
                    if (flow.Count == 0)
                    {
                        FlowComputer model = new FlowComputer();
                        model.VisitSE = vistio[i].VisitSE;
                        model.Id = vistio.Count(m => m.VisitSE == vistio[i].VisitSE);
                        decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                        if (rateResult >= 1) { rateResult = 1; }
                        model.SearchTerms = (rateResult * 100).ToString().Length >= 5 ?
                            (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                        flow.Add(model);
                    }
                    else
                    {
                        if (flow.Count <= 10)
                        {
                            for (int j = 0; j < flow.Count; j++)
                            {
                                if (flow[j].VisitSE.Trim() != vistio[i].VisitSE.Trim())
                                {
                                    FlowComputer model = new FlowComputer();
                                    model.VisitSE = vistio[i].VisitSE;
                                    model.Id = vistio.Count(m => m.VisitSE == vistio[i].VisitSE);
                                    decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                                    if (rateResult >= 1) { rateResult = 1; }
                                    model.SearchTerms = (rateResult * 100).ToString().Length >= 5 ?
                                        (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                                    flow.Add(model);
                                }
                            }
                        }
                    }
                }
                //去除冗余数据
                for (int i = 0; i < flow.Count(); i++)
                {
                    for (int j = flow.Count() - 1; j > i; j--)
                    {
                        if (flow[i].VisitSE == flow[j].VisitSE)
                        {
                            flow.RemoveAt(j);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return flow = null;
                throw ex;
            }
            return flow.OrderByDescending(m => m.Id).ToList();
        }
        /// <summary>
        /// Top10入口页面
        /// </summary>
        /// <param name="endTime"></param>
        /// <param name="date"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public List<VisitorInfo> EntryPageByDay(DateTime endTime, string date, int site)
        {
            List<VisitorInfo> flow = new List<VisitorInfo>();
            try
            {
                List<VisitorInfo> vistio = null;
                if (date == "0" || date == "-1")
                {
                    date = date == "0" ? "0" : date;
                    endTime = DateTime.Now.AddDays(double.Parse(date));
                    vistio = work.CreateRepository<VisitorInfo>().GetList(
                        m => DbFunctions.DiffDays(m.AccessTime, endTime) == 0 && m.WebInfo.Id == site
                        ).ToList();
                }
                else
                {
                    vistio = work.CreateRepository<VisitorInfo>().GetList(
                        m => DbFunctions.DiffDays(m.AccessTime, endTime) <= 0 && m.WebInfo.Id == site
                        ).ToList();
                }
                //数量&占比计算
                for (int i = 0; i < vistio.Count; i++)
                {
                    if (flow.Count == 0)
                    {
                        VisitorInfo model = new VisitorInfo();
                        model.VisitPage = vistio[i].VisitPage;
                        model.Id = vistio.Count(m => m.VisitPage == vistio[i].VisitPage);
                        decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                        if (rateResult >= 1) { rateResult = 1; }
                        model.VisitSE = (rateResult * 100).ToString().Length >= 5 ?
                            (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                        flow.Add(model);
                    }
                    else
                    {
                        if (flow.Count <= 10)
                        {
                            for (int j = 0; j < flow.Count; j++)
                            {
                                if (flow[j].VisitPage.Trim() != vistio[i].VisitPage.Trim())
                                {
                                    VisitorInfo model = new VisitorInfo();
                                    model.VisitPage = vistio[i].VisitPage;
                                    model.Id = vistio.Count(m => m.VisitPage == vistio[i].VisitPage);
                                    decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                                    if (rateResult >= 1) { rateResult = 1; }
                                    model.VisitSE = (rateResult * 100).ToString().Length >= 5 ?
                                        (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                                    flow.Add(model);
                                }
                            }
                        }
                    }
                }
                //去除冗余数据
                for (int i = 0; i < flow.Count(); i++)
                {
                    for (int j = flow.Count() - 1; j > i; j--)
                    {
                        if (flow[i].VisitPage == flow[j].VisitPage)
                        {
                            flow.RemoveAt(j);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return flow = null;
                throw ex;
            }
            return flow.OrderByDescending(m => m.Id).ToList();
        }
        /// <summary>
        /// 受访页面(数量,占比)
        /// </summary>
        /// <param name="endTime"></param>
        /// <param name="date"></param>
        /// <param name="site"></param>
        /// <returns></returns>
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
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) == 0 && m.WebInfo.Id == site
                        ).ToList();
                }
                else
                {
                    vistio = work.CreateRepository<FlowComputer>().GetList(
                        m => DbFunctions.DiffDays(m.CurrentTime, endTime) <= 0 && m.WebInfo.Id == site
                        ).ToList();
                }
                //数量&占比计算
                for (int i = 0; i < vistio.Count; i++)
                {
                    if (flow.Count == 0)
                    {
                        FlowComputer model = new FlowComputer();
                        model.VisitPage = vistio[i].VisitPage;
                        model.Id = vistio.Count(m => m.VisitPage == vistio[i].VisitPage);
                        decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                        if (rateResult >= 1) { rateResult = 1; }
                        model.VisitSE = (rateResult * 100).ToString().Length >= 5 ?
                            (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                        flow.Add(model);
                    }
                    else
                    {
                        if (flow.Count <= 10)
                        {
                            for (int j = 0; j < flow.Count; j++)
                            {
                                if (flow[j].VisitPage.Trim() != vistio[i].VisitPage.Trim())
                                {
                                    FlowComputer model = new FlowComputer();
                                    model.VisitPage = vistio[i].VisitPage;
                                    model.Id = vistio.Count(m => m.VisitPage == vistio[i].VisitPage);
                                    decimal rateResult = Math.Round((decimal)model.Id / vistio.Count, 4);
                                    if (rateResult >= 1) { rateResult = 1; }
                                    model.VisitSE = (rateResult * 100).ToString().Length >= 5 ?
                                        (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                                    flow.Add(model);
                                }
                            }
                        }
                    }
                }
                //去除冗余数据
                for (int i = 0; i < flow.Count(); i++)
                {
                    for (int j = flow.Count() - 1; j > i; j--)
                    {
                        if (flow[i].VisitPage == flow[j].VisitPage)
                        {
                            flow.RemoveAt(j);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return flow = null;
                throw ex;
            }
            return flow.OrderByDescending(m => m.Id).ToList();
        }
        /// <summary>
        /// 新老访客(浏览量、访客数、跳出率、平均访问时长、平均访问页数)
        /// </summary>
        /// <param name="endTime"></param>
        /// <param name="date"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public List<WebInfo> NewOldVisit(DateTime endTime, string date, int site)
        {
            List<WebInfo> list = new List<WebInfo>();
            if (date == "0" || date == "-1")
            {
                //按IP地址分组
                //List<FlowComputer> vistio = null;
                var oldlist = db.VisitorInfo.Where(v => DbFunctions.DiffDays(v.AccessTime, endTime) == 0).GroupBy(p => p.IpAddress).Select(p => new { count = p.Count(), key = p.Key });
                double old = 0;
                double Xin = 0;
                foreach (var item in oldlist)
                {
                    int str1 = item.count;
                    if (str1 > 1)
                    {
                        //获取老访客数量
                        old += old + 1;
                    }
                    else
                    {
                        //获取新访客数量
                        Xin += Xin + 1;
                    }
                }
                //老访客比例
                string oldRatio = "";
                //新访客比例
                string newRatio = "";
                if (old + Xin > 0)
                {
                    oldRatio = (old / (old + Xin) * 100).ToString("0.00");
                    newRatio = (Xin / (old + Xin) * 100).ToString("0.00");
                }
                else
                {
                    oldRatio = "0.00";
                    newRatio = "0.00";
                }
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
                var oldtoday = GetVistiorByDay(endTime, webList.Id);
                //PV
                double oPv = double.Parse(oldtoday.WebPv.ToString()) * (old / (old + Xin));
                oldtoday.WebPv = int.Parse(oPv.ToString("0"));
                oldtoday.WebKey = oldRatio;
                //UV
                double oUv = double.Parse(oldtoday.WebUv.ToString()) * (old / (old + Xin));
                oldtoday.WebUv = int.Parse(oUv.ToString("0"));
                //BR
                double oBr = double.Parse(oldtoday.BounceRate.ToString().Replace("%", "").Trim()) * (old / (old + Xin));
                oldtoday.BounceRate = oBr.ToString("0.00") + "%";
                //TS
                string time = oldtoday.WebTS;
                int hour = int.Parse(time.Split(':')[0].ToString());
                int minute = int.Parse(time.Split(':')[1].ToString());
                int second = int.Parse(time.Split(':')[2].ToString());
                time = (hour * 3600 + minute * 60 + second).ToString();
                double nTs = double.Parse(time) * (old / (old + Xin));
                TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(nTs));
                oldtoday.WebTS = ts.ToString();
                //平均访问页数
                var page = 0.00;
                if (oUv != 0)
                {
                    page = db.FlowComputer.Where(v => System.Data.Entity.Core.Objects.EntityFunctions.DiffDays(v.CurrentTime, endTime) == 0).GroupBy(p => p.Id).Select(p => p.Count()).DefaultIfEmpty().Sum() * (old / ((Xin + old))) / oUv;
                    oldtoday.WebDomain = page.ToString("0.00");
                }
                else
                {
                    oldtoday.WebDomain = "0";
                }
                list.Add(oldtoday);

                var newtoday = GetVistiorByDay(endTime, webList.Id);
                newtoday.WebKey = newRatio;
                //PV
                double nPv = double.Parse(newtoday.WebPv.ToString()) * (Xin / (old + Xin));
                newtoday.WebPv = int.Parse(nPv.ToString("0"));
                //UV
                double nUv = double.Parse(newtoday.WebUv.ToString()) * (Xin / (old + Xin));
                newtoday.WebUv = int.Parse(nUv.ToString("0"));
                //BR
                double nBr = double.Parse(newtoday.BounceRate.ToString().Replace("%", "").Trim()) * (Xin / (old + Xin));
                newtoday.BounceRate = nBr.ToString() + "%";
                //TS平均访问时长
                time = newtoday.WebTS;
                hour = int.Parse(time.Split(':')[0].ToString());
                minute = int.Parse(time.Split(':')[1].ToString());
                second = int.Parse(time.Split(':')[2].ToString());
                time = (hour * 3600 + minute * 60 + second).ToString();
                nTs = double.Parse(time) * (Xin / (old + Xin));
                ts = new TimeSpan(0, 0, Convert.ToInt32(nTs));
                newtoday.WebTS = ts.ToString();
                //平均访问页数
                //double nPage = double.Parse(newtoday.WebConversion.ToString()) * (Xin / (old + Xin));
                if (nUv != 0)
                {
                    page = db.FlowComputer.Where(v => DbFunctions.DiffDays(v.CurrentTime, endTime) == 0).GroupBy(p => p.Id).Select(p => p.Count()).DefaultIfEmpty().Sum() * (Xin / ((Xin + old))) / nUv;
                    newtoday.WebDomain = page.ToString("0.00");
                }
                else
                {
                    newtoday.WebDomain = "0";
                }
                list.Add(newtoday);
            }
            else
            {

                var oldlist = db.VisitorInfo.Where(v => DbFunctions.DiffDays(v.AccessTime, endTime) <= 0).GroupBy(p => p.IpAddress).Select(p => new { count = p.Count() });
                double old = 0;
                double Xin = 0;
                foreach (var item in oldlist)
                {
                    int str1 = item.count;
                    if (str1 > 1)
                    {
                        //获取老访客数量
                        old += old + 1;
                    }
                    else
                    {
                        //获取新访客数量
                        Xin += Xin + 1;
                    }
                }
                //老访客比例
                string oldRatio = "";
                //新访客比例
                string newRatio = "";
                if (old + Xin > 0)
                {
                    oldRatio = (old / (old + Xin) * 100).ToString("0.00");
                    newRatio = (Xin / (old + Xin) * 100).ToString("0.00");
                }
                else
                {
                    oldRatio = "0.00";
                    newRatio = "0.00";
                }
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
                var oldtoday = GetVistiorByEveryDay(endTime, webList.Id);
                oldtoday.WebKey = oldRatio;
                //PV
                double oPv = double.Parse(oldtoday.WebPv.ToString()) * (old / (old + Xin));
                oldtoday.WebPv = int.Parse(oPv.ToString("0"));
                //UV
                double oUv = double.Parse(oldtoday.WebUv.ToString()) * (old / (old + Xin));
                oldtoday.WebUv = int.Parse(oUv.ToString("0"));
                //BR
                double oBr = double.Parse(oldtoday.BounceRate.ToString().Replace("%", "").Trim()) * (old / (old + Xin));
                oldtoday.BounceRate = oBr.ToString("0.00") + "%";
                //TS
                string time = oldtoday.WebTS;
                int hour = int.Parse(time.Split(':')[0].ToString());
                int minute = int.Parse(time.Split(':')[1].ToString());
                int second = int.Parse(time.Split(':')[2].ToString());
                time = (hour * 3600 + minute * 60 + second).ToString();
                double nTs = double.Parse(time) * (old / (old + Xin));
                TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(nTs));
                oldtoday.WebTS = ts.ToString();
                //平均访问页数
                var page = 0.00;
                if (oUv != 0)
                {
                    page = db.FlowComputer.Where(v => System.Data.Entity.Core.Objects.EntityFunctions.DiffDays(v.CurrentTime, endTime) <= 0).GroupBy(p => p.Id).Select(p => p.Count()).DefaultIfEmpty().Sum() * (old / ((Xin + old))) / oUv;
                    oldtoday.WebDomain = page.ToString("0.00");
                }
                else
                {
                    oldtoday.WebDomain = "0.00";
                }
                list.Add(oldtoday);

                var newtoday = GetVistiorByEveryDay(endTime, webList.Id);
                newtoday.WebKey = newRatio;
                //PV
                double nPv = double.Parse(newtoday.WebPv.ToString()) * (Xin / (old + Xin));
                newtoday.WebPv = int.Parse(nPv.ToString("0"));
                //UV
                double nUv = double.Parse(newtoday.WebUv.ToString()) * (Xin / (old + Xin));
                newtoday.WebUv = int.Parse(nUv.ToString("0"));
                //BR
                double nBr = double.Parse(newtoday.BounceRate.ToString().Replace("%", "").Trim()) * (Xin / (old + Xin));
                newtoday.BounceRate = nBr.ToString("0.00") + "%";
                //TS平均访问时长
                time = newtoday.WebTS;
                hour = int.Parse(time.Split(':')[0].ToString());
                minute = int.Parse(time.Split(':')[1].ToString());
                second = int.Parse(time.Split(':')[2].ToString());
                time = (hour * 3600 + minute * 60 + second).ToString();
                nTs = double.Parse(time) * (Xin / (old + Xin));
                ts = new TimeSpan(0, 0, Convert.ToInt32(nTs));
                newtoday.WebTS = ts.ToString();
                //平均访问页数
                //double nPage = double.Parse(newtoday.WebConversion.ToString()) * (Xin / (old + Xin));
                if (nUv != 0)
                {
                    page = db.FlowComputer.Where(v => DbFunctions.DiffDays(v.CurrentTime, endTime) <= 0).GroupBy(p => p.Id).Select(p => p.Count()).DefaultIfEmpty().Sum() * (Xin / ((Xin + old))) / nUv;
                    newtoday.WebDomain = page.ToString("0.00");
                }
                else
                {
                    newtoday.WebDomain = "0.00";
                }
                list.Add(newtoday);
            }
            //根据新老访客查询浏览量
            //根据新老访客查询访客数
            //根据新老访客查询跳出率
            //根据新老访客查询平均访问时长
            //根据新老访客查询平均访问页数
            return list;
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
        public WebInfo GetVistiorByEveryDay(DateTime startTime, int site)
        {
            WebInfo web = new WebInfo();
            try
            {
                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessTime, startTime) <= 0 && m.WebInfo.Id == site).ToList();
                //获取pv量,根据条件时间&网站id
                web.WebPv = work.CreateRepository<FlowComputer>().GetList(m => DbFunctions.DiffDays(m.CurrentTime, startTime) <= 0 && m.WebInfo.Id == site).Count();
                //获取uv值,根据条件时间&网站id
                web.WebUv = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessTime, startTime) <= 0 && m.WebInfo.Id == site).Count();
                //获取ip数 去重
                List<VisitorInfo> webuv = work.CreateRepository<VisitorInfo>().GetList(m => DbFunctions.DiffDays(m.AccessTime, startTime) <= 0 && m.WebInfo.Id == site).ToList();
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
                int rate = work.CreateRepository<VisitorInfo>().GetCount(m => m.PageNumber == 0 && DbFunctions.DiffDays(m.AccessTime, startTime) <= 0 && m.WebInfo.Id == site);
                int sumPv = work.CreateRepository<WebInfo>().GetList(m => m.UserInfo.Id == site).FirstOrDefault().WebPv;
                decimal rateResult = Math.Round((decimal)rate / web.WebPv, 4);
                if (rateResult >= 1) { rateResult = 1; }
                web.BounceRate = (rateResult * 100).ToString().Length >= 5 ? (rateResult * 100).ToString().Substring(0, 4) + "%" : (rateResult * 100).ToString() + "%";
                //计算转换次数,根据条件时间&网站id
                web.WebConversion = 0;
                //计算平均时长,根据条件时间&网站id
                int sumDuration = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffDays(m.AccessTime, startTime) <= 0 && m.WebInfo.Id == site);
                double duration = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffDays(m.AccessTime, startTime) <= 0 && m.WebInfo.Id == site);
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
        /// 根据历史巅峰计算统计流量
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public WebInfo GetVistiorByMax(int site)
        {
            WebInfo web = new WebInfo();
            try
            {
                var whereMax = work.CreateRepository<FlowComputer>().GetList(
                    m => m.WebInfo.Id == site).OrderBy(m => m.CurrentTime).FirstOrDefault();
                List<VisitorInfo> vistio = work.CreateRepository<VisitorInfo>().GetList(
                    m => m.WebInfo.Id == site && DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0
                    ).ToList();
                //获取pv量,根据条件时间&网站id
                web.WebPv = work.CreateRepository<FlowComputer>().GetList(
                    m => m.WebInfo.Id == site).OrderBy(m => m.CurrentTime
                    ).Count();
                web.WebDomain = "" + whereMax.CurrentTime.ToShortDateString();
                //获取uv值,根据条件时间&网站id
                web.WebUv = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site
                    ).Count();
                //获取ip数 去重
                List<VisitorInfo> webuv = work.CreateRepository<VisitorInfo>().GetList(
                    m => DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site
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
                    m => m.PageNumber == 0 && DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site);
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
                    m => DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site);
                double duration = work.CreateRepository<VisitorInfo>().GetCount(
                    m => DbFunctions.DiffDays(m.AccessTime, whereMax.CurrentTime) == 0 && m.WebInfo.Id == site);
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
        public ActionResult AnalysisInit(int siteId)
        {
            #region  获取今天的浏览量、访客数、跳出率、平均访问时长
            //获取今天的浏览量、访客数、跳出率、平均访问时长
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            int year = int.Parse(today.Split('-')[0].ToString());
            int month = int.Parse(today.Split('-')[1].ToString());
            int day = int.Parse(today.Split('-')[2].ToString());
            DateTime? startTime = new DateTime(year, month, day);
            //浏览量
            double model = work.CreateRepository<FlowComputer>().GetCount(m => DbFunctions.DiffDays(m.CurrentTime, startTime) == 0);
            if (model == 0)
            {
                return View();
            }
            //访客数
            double model1 = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffDays(m.AccessTime, startTime) == 0);
            //跳出率
            var list = work.CreateRepository<VisitorInfo>().GetList(v => DbFunctions.DiffDays(v.AccessTime, startTime) == 0);
            double pagenumber = work.CreateRepository<VisitorInfo>().GetCount(m => m.PageNumber == 0 && DbFunctions.DiffDays(m.AccessTime, startTime) == 0);
            double number = work.CreateRepository<VisitorInfo>().GetCount(m => m.PageNumber > 0 && DbFunctions.DiffDays(m.AccessTime, startTime) == 0);
            string br = (pagenumber / (number + pagenumber) * 100).ToString("0.00") + "%";
            //平均访问时长
            var a = 0.00;
            foreach (var item in list)
            {
                a += item.Duration;
            }
            double avgts = a / model1;
            TimeSpan ts = new TimeSpan(0, 0, Convert.ToInt32(avgts));
            string str = ts.ToString();
            //获取前一天天的浏览量、访客数、跳出率、平均访问时长
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            int yyear = int.Parse(yesterday.Split('-')[0].ToString());
            int ymonth = int.Parse(yesterday.Split('-')[1].ToString());
            int yday = int.Parse(yesterday.Split('-')[2].ToString());
            DateTime? ystartTime = new DateTime(yyear, ymonth, yday);
            double ymodel = work.CreateRepository<FlowComputer>().GetCount(m => DbFunctions.DiffDays(m.CurrentTime, ystartTime) == 0);
            double ymodel1 = work.CreateRepository<VisitorInfo>().GetCount(m => DbFunctions.DiffDays(m.AccessTime, ystartTime) == 0);
            var ylist = work.CreateRepository<VisitorInfo>().GetList(v => DbFunctions.DiffDays(v.AccessTime, ystartTime) == 0);
            double ypagenumber = work.CreateRepository<VisitorInfo>().GetCount(m => m.PageNumber == 0 && DbFunctions.DiffDays(m.AccessTime, ystartTime) == 0);
            double ynumber = work.CreateRepository<VisitorInfo>().GetCount(m => m.PageNumber > 0 && DbFunctions.DiffDays(m.AccessTime, ystartTime) == 0);
            var ya = 0.00;
            foreach (var item in ylist)
            {
                ya += item.Duration;
            }
            double yavgts = ya / ymodel1;
            TimeSpan yts = new TimeSpan(0, 0, Convert.ToInt32(yavgts));
            string ystr = yts.ToString();
            //pv相比昨天
            string cwpv = ((model - ymodel) / model * 100).ToString("0.00") + "%";
            //uv相比昨天
            string cwuv = ((model1 - ymodel1) / model1 * 100).ToString("0.00") + "%";
            //br相比昨天
            string cwbr = ((pagenumber / (number + pagenumber) - ypagenumber / (ynumber + ypagenumber)) / pagenumber / (number + pagenumber)).ToString("0.00") + "%";
            //平均访问时长相比昨天
            string cwa = ((a - ya) / a * 100).ToString("0.00") + "%";
            #endregion

            #region  获取新访客老访客占比
            //按IP地址分组
            var oldlist = db.VisitorInfo.Where(v => DbFunctions.DiffDays(v.AccessTime, startTime) == 0).GroupBy(p => p.IpAddress).Select(p => new { count = p.Count() });
            double old = 0;
            double Xin = 0;
            foreach (var item in oldlist)
            {
                int str1 = item.count;
                if (str1 > 1)
                {
                    //获取老访客数量
                    old += old + 1;
                }
                else
                {
                    //获取新访客数量
                    Xin += Xin + 1;
                }
            }

            //var xin = db.VisitorInfo.Where();

            ////老访客比例
            //string oldRatio = (old / (old + Xin) * 100).ToString("0.00") + "%";
            ////新访客比例
            //string newRatio = (Xin / (old + Xin) * 100).ToString("0.00") + "%";
            #endregion

            #region 访问城市
            //var cityList = db.VisitorInfo.GroupBy(p => p.IpAddress).Select(p => new { count = p.Count(), city = p.Address });
            var q = from p in db.VisitorInfo
                    group p by p.Address into g
                    select new
                    {
                        g.Key,
                        NumProducts = g.Count()
                    };
            //string city = "";
            //int cityNumber = 0;
            //foreach (var item in q)
            //{
            //    city = item.Key.Replace("市", "").Trim();
            //    cityNumber = item.NumProducts;
            //}
            #endregion

            #region  浏览量趋势


            #endregion

            #region  访问用户趋势
            #endregion

            #region  平均访问时长
            #endregion

            #region  跳出率趋势
            #endregion

            #region  来源类型
            #endregion

            #region  搜索词
            #endregion

            #region  来源类型
            #endregion

            return Json(new { pv = model, uv = model1, ts = str, br = br, cwpv = cwpv, cwa = cwa, cwuv = cwuv, cwbr = cwbr, old = old, xin = Xin, cityList = q }, JsonRequestBehavior.AllowGet);
        }
    }
}