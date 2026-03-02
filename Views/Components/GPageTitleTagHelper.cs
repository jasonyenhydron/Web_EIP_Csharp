using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// g-page-title：主內容區頁面標題列元件
    /// 顯示 SVG icon + h1 標題 + 副標說明文字。
    /// 注意：區別於 g-page-header（g-page-header 用於 Popup 視窗頁首；
    ///       g-page-title 用於主版面 main 區的頁面標題）
    ///
    /// 使用方式：
    ///   <g-page-title title="MIS 程式項目列表"
    ///                 subtitle="查看和管理系統程式項目"
    ///                 icon="document" />
    ///
    /// icon 支援：document, list, user, calendar, chart, cog, check, search, shield, database
    /// </summary>
    [HtmlTargetElement("g-page-title")]
    public class GPageTitleTagHelper : TagHelper
    {
        /// <summary>H1 標題文字</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>副標說明文字（可選）</summary>
        public string Subtitle { get; set; } = string.Empty;

        /// <summary>圖示名稱（與 GPageHeaderTagHelper 一致的圖示庫）</summary>
        public string Icon { get; set; } = "document";

        /// <summary>外部額外 CSS class（用於調整 margin）</summary>
        public string Class { get; set; } = "mb-6";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", Class);

            string iconSvg = GetIconSvg(Icon);

            string subtitleHtml = !string.IsNullOrEmpty(Subtitle)
                ? $"<p class=\"text-slate-600 mt-1\">{HtmlEncode(Subtitle)}</p>"
                : string.Empty;

            output.Content.SetHtmlContent($@"
<h1 class=""text-2xl font-bold text-slate-800 flex items-center gap-2"">
    {iconSvg}
    {HtmlEncode(Title)}
</h1>
{subtitleHtml}");
        }

        private static string GetIconSvg(string icon) => icon?.ToLower() switch
        {
            "list" => @"<svg class=""w-7 h-7 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 6h16M4 10h16M4 14h16M4 18h16""/></svg>",
            "user" => @"<svg class=""w-7 h-7 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z""/></svg>",
            "calendar" => @"<svg class=""w-7 h-7 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z""/></svg>",
            "chart" => @"<svg class=""w-7 h-7 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z""/></svg>",
            "cog" => @"<svg class=""w-7 h-7 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z""/><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 12a3 3 0 11-6 0 3 3 0 016 0z""/></svg>",
            "database" => @"<svg class=""w-7 h-7 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4m0 5c0 2.21-3.582 4-8 4s-8-1.79-8-4""/></svg>",
            "shield" => @"<svg class=""w-7 h-7 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z""/></svg>",
            _ => @"<svg class=""w-7 h-7 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z""/></svg>"
        };

        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}
