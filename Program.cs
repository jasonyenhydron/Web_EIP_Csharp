using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;
using Web_EIP_Csharp.Helpers;

var builder = WebApplication.CreateBuilder(args);

DbHelper.Configure(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Add Session Services
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

// Configure the HTTP request pipeline.
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Similar to FastAPI static cache-control
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

// Use Session middleware! This must be BEFORE UseAuthorization
app.UseSession();

app.UseAuthorization();

// Map default route to Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

