using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;

namespace Web_EIP_Csharp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = username;
            ViewBag.UserName = HttpContext.Session.GetString("user_name");
            ViewBag.TodayDate = DateTime.Today.ToString("yyyy-MM-dd");

            return View();
        }
    }
}

