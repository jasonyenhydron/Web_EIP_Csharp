// ?嚗董??交?嗅嚗?鞎祉?亥?霅??澈???瑼Ｘ??箸? Session 皜???
// 頛詨嚗撓??tns?sername?assword??蝟餉??交??澆??嚗誑???Session ???
// 頛詨嚗撓?箇?仿??Ｕ?交???憭望?閮??箏?撠?蝯???
// 靘陷嚗bHelper?ttpContext.Session?ogin.cshtml?SP.NET Core MVC??

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Web_EIP_Csharp.Helpers;

namespace Web_EIP_Csharp.Controllers
{
    public class AccountController : Controller
    {
        private static readonly List<string> TNS_LIST = new() { "MIS", "TEST" };

        private static string BuildDbConnectionString(string tns) =>
            DbHelper.BuildConnectionString(tns);

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.TnsList = TNS_LIST;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string tns, string username, string password,
                                               string use_defaults, string language,
                                               string date_format, string test_mode, string work_dir)
        {
            ViewBag.TnsList = TNS_LIST;

            if (string.IsNullOrEmpty(tns) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "請輸入完整登入資訊 (Incomplete login information)";
                return View();
            }

            try
            {
                var conn = BuildDbConnectionString(tns);
                var valid = await DbHelper.ExecuteScalarAsync(
                    conn,
                    CommandType.Text,
                    "SELECT IDM.f_idm_check_mis_password(:account, :pwd) FROM DUAL",
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("account", username.ToUpperInvariant()),
                        DbHelper.CreateParameter("pwd", password)
                    });

                if (!string.Equals(valid?.ToString(), "Y", StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.Error = "登入失敗: 帳號或密碼錯誤 (Invalid username or password)";
                    ViewBag.Username = username;
                    ViewBag.Tns = tns;
                    return View();
                }

                var userNameObj = await DbHelper.ExecuteScalarAsync(
                    conn,
                    CommandType.Text,
                    "SELECT USER_NAME FROM IDM_USER WHERE USER_NO = :user_no",
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("user_no", username.ToUpperInvariant())
                    });

                var userName = userNameObj?.ToString();

                HttpContext.Session.SetString("username", username);
                HttpContext.Session.SetString("password", password);
                HttpContext.Session.SetString("tns", tns);
                HttpContext.Session.SetString("user_name", string.IsNullOrWhiteSpace(userName) ? username : userName);

                return RedirectToAction("Dashboard", "Dashboard");
            }
            catch (Exception ex)
            {
                var message = ex.Message ?? string.Empty;
                if (message.Contains("ORA-01017", StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.Error = "資料庫連線失敗：ORA-01017（資料庫帳號/密碼錯誤）。請檢查 appsettings.json 的 ConnectionStrings:oracleConn_MIS / oracleConn_TEST。";
                }
                else
                {
                    ViewBag.Error = $"資料庫連線失敗: {message}";
                }
                ViewBag.Username = username;
                ViewBag.Tns = tns;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}




