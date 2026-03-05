// 功能：儀表板控制器，負責 Index/Dashboard/Board/SigonCenter 頁面顯示與登入狀態檢查。
// 輸入：輸入頁面路由請求與 Session 使用者身分資訊。
// 輸出：輸出儀表板相關視圖回應，或未登入時導向 Account/Login。
// 依賴：HttpContext.Session、Dashboard 視圖檔、ASP.NET Core MVC。

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;

namespace Web_EIP_Csharp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult Dashboard()
        {
            return RenderDashboard("Dashboard");
        }

        public IActionResult Board()
        {
            return RenderDashboard("~/Views/Dashboard/Board.cshtml");
        }

        public IActionResult SigonCenter()
        {
            return RenderDashboard("~/Views/Dashboard/sigoncenter.cshtml");
        }

        private IActionResult RenderDashboard(string viewName)
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = username;
            ViewBag.UserName = HttpContext.Session.GetString("user_name");
            ViewBag.TodayDate = DateTime.Today.ToString("yyyy-MM-dd");

            return View(viewName);
        }
    }
}

