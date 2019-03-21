using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GcSite.ModuleSys.Controllers
{
    public class ToolbarController : Controller
    {
        // GET: Toolbar
        /// <summary>
        /// 情况
        /// </summary>
        /// <returns></returns>
        public ActionResult Case()
        {
            return View();
        }
        /// <summary>
        /// 帮助
        /// </summary>
        /// <returns></returns>
        public ActionResult Helper()
        {
            return View();
        }
        /// <summary>
        /// 管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Management()
        {
            return View();
        }
        /// <summary>
        /// 产品
        /// </summary>
        /// <returns></returns>
        public ActionResult Products()
        {
            return View();
        }
    }
}