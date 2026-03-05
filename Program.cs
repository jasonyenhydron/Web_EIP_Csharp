// 功能：應用程式啟動與路由管線設定。
// 輸入：appsettings 設定、環境變數、HTTP 請求。
// 輸出：WebApplication 執行管線與路由設定。
// 依賴：ASP.NET Core Hosting、Middleware、Session、ExceptionHandler。

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;
using Web_EIP_Csharp.Helpers;

var builder = WebApplication.CreateBuilder(args);

DbHelper.Configure(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    var timeoutHours = builder.Configuration.GetValue<int>("SessionTimeoutHours", 2);
    options.Cookie.Name = ".WebEIP.Session";
    options.IdleTimeout = System.TimeSpan.FromHours(timeoutHours);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var ex = feature?.Error;

        static (int? lineNumber, string fileName) ParseLine(string? stack)
        {
            if (string.IsNullOrWhiteSpace(stack)) return (null, "");
            var m = Regex.Match(stack, @" in (?<file>.*):line (?<line>\d+)");
            if (!m.Success) return (null, "");
            var file = m.Groups["file"].Value ?? "";
            var lineText = m.Groups["line"].Value ?? "";
            if (!int.TryParse(lineText, out var line)) return (null, file);
            return (line, file);
        }

        var (lineNumber, fileName) = ParseLine(ex?.StackTrace);

        var isApi = context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                    || context.Request.Headers.Accept.Any(h => (h?.Contains("application/json", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
                    || string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (isApi)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new
            {
                status = "error",
                message = ex?.Message ?? "Unhandled exception",
                lineNumber,
                fileName,
                detail = app.Environment.IsDevelopment() ? ex?.ToString() : ""
            });
            return;
        }

        context.Response.Redirect("/Home/Error");
    });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "files",
    pattern: "Files/{action=Index}/{id?}",
    defaults: new { controller = "Files" });

app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/{action=Index}/{id?}",
    defaults: new { controller = "Dashboard" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

