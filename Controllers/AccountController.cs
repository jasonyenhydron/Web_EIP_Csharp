using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Web_EIP_Csharp.Helpers;

namespace Web_EIP_Csharp.Controllers
{
    public class AccountController : Controller
    {
        private static readonly List<string> TNS_LIST = new List<string> { "MIS", "TEST" };

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
                ViewBag.Error = "登入資訊不完整 / Incomplete login information";
                return View();
            }

            try
            {
                Console.WriteLine($"Attempting Oracle Connection and Validation: User={username}, TNS={tns}");

                // Validate Connection and User Login using IDM.f_idm_check_mis_password
                bool isValidUser = await OracleDbHelper.ValidateUserLoginAsync(username, password, tns);
                if (!isValidUser)
                {
                    ViewBag.Error = "登入失敗: 帳號或密碼錯誤 (Invalid username or password)";
                    ViewBag.Username = username;
                    ViewBag.Tns = tns;
                    return View();
                }

                // If connection works and user is valid, attempt to fetch user name
                string userName = null;
                try
                {
                    userName = await OracleDbHelper.GetUserNameAsync(username, password, tns);
                    if (!string.IsNullOrEmpty(userName))
                        Console.WriteLine($"Fetched User Name: {userName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching user name: {ex.Message}");
                }

                Console.WriteLine($"Oracle Connection Successful: User={username}");

                // Store credentials in Session
                HttpContext.Session.SetString("username", username);
                HttpContext.Session.SetString("password", password);
                HttpContext.Session.SetString("tns", tns);
                HttpContext.Session.SetString("user_name", userName ?? username); // Fallback to username

                return RedirectToAction("Index", "Dashboard");
            }
            catch (OracleException dbEx)
            {
                Console.WriteLine($"Login Failed (Oracle Error): {dbEx.Message}");
                ViewBag.Error = $"資料庫連線失敗: {dbEx.Message}";
                ViewBag.Username = username;
                ViewBag.Tns = tns;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Failed: {ex.Message}");
                ViewBag.Error = $"Connection Failed: {ex.Message}";
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
