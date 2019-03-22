using GcSite.ModuleSys.DAL;
using GcSite.ModuleSys.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace GcSite.ModuleSys.Controllers
{
    public class UserController : Controller
    {
        WorkOfUnit work = new WorkOfUnit();
        GcSiteDb db = new GcSiteDb();
        // GET: User
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string name, string pwd, string code)
        {
            string userPwd = MD5Encrypt(pwd.Trim());
            pwd = userPwd;
            //判断验证码是否正确
            if (code.ToLower() == Session["code"].ToString().ToLower())
            {
                //判断用户是否存在
                var model = work.CreateRepository<UserInfo>().GetList(p => p.UserName == name && p.UserPwd == pwd);
                //model = db.UserInfo.Where(p => p.UserName == name && p.UserPwd == pwd);
                if (model.Count() > 0)
                {
                    string id = "";
                    foreach (var item in model)
                    {
                        id = item.Id.ToString();
                    }
                    //给用户设置票证
                    //FormsAuthentication.SetAuthCookie(name.ToString(), false);
                    return Json(new { success = 1, id = id });
                }
                else
                {
                    return Json(new { success = 2 });
                }
            }
            else
            {
                return Json(new { success = 3 });
            }
        }
        /// <summary>
        /// 验证码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetCodeImg(double? id)
        {
            string code = new ValidataCode().GetCode(4);
            Session["code"] = code;
            Session.Timeout = 10;
            byte[] img = new ValidataCode().CreateValidateGraphic(code);
            return File(img, @"img/jpeg");
        }
        public ActionResult LoginVerify(string name, string pwd, string code)
        {
            var a = 0;
            if (true)
            {

            }
            return Json(a, JsonRequestBehavior.AllowGet);
        }

        #region MD5 加密

        /// <summary>
        /// MD5 加密静态方法
        /// </summary>
        /// <param name="EncryptString">待加密的密文</param>
        /// <returns>returns</returns>
        public static string MD5Encrypt(string EncryptString)
        {
            if (string.IsNullOrEmpty(EncryptString)) { throw (new Exception("密文不得为空")); }
            MD5 m_ClassMD5 = new MD5CryptoServiceProvider();
            string m_strEncrypt = "";
            try
            {
                m_strEncrypt = BitConverter.ToString(m_ClassMD5.ComputeHash(Encoding.Default.GetBytes(EncryptString))).Replace("-", "");
            }
            catch (ArgumentException ex) { throw ex; }
            catch (CryptographicException ex) { throw ex; }
            catch (Exception ex) { throw ex; }
            finally { m_ClassMD5.Clear(); }
            return m_strEncrypt;
        }
        #endregion
    }
}