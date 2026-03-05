// 功能：網站基礎頁面控制器，提供 Index、Privacy、Error 預設頁面。
// 輸入：輸入 HTTP GET 請求與目前 RequestId。
// 輸出：輸出 Index/Privacy/Error 視圖回應。
// 依賴：ErrorViewModel、ResponseCache、ASP.NET Core MVC。

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web_EIP_Csharp.Models;

namespace Web_EIP_Csharp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

