using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// g-status-bar：程式底部狀態列元件
    /// 深色背景、訊息輸入欄、右側記錄計數，仿 Oracle Forms 操作列。
    ///
    /// 使用方式：
    ///   <g-status-bar msg-id="statusBarMsg" record-info="1/1" default-msg="Ready." />
    ///
    /// JS 更新訊息：
    ///   document.getElementById('statusBarMsg').value = '資料已儲存';
    /// </summary>
    [HtmlTargetElement("g-status-bar")]
    public class GStatusBarTagHelper : TagHelper
    {
        /// <summary>訊息 input 的 HTML id（供 JS 更新）</summary>
        public string MsgId { get; set; } = "statusBarMsg";

        /// <summary>右側記錄計數文字，例如 "1/1" 或 "0 筆"</summary>
        public string RecordInfo { get; set; } = string.Empty;

        /// <summary>訊息欄預設文字</summary>
        public string DefaultMsg { get; set; } = "Ready.";

        /// <summary>Alpine.js 動態記錄計數表達式（啟用時取代 record-info 靜態值）</summary>
        public string AlpineRecordInfo { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class",
                "bg-slate-800 rounded-lg p-3 text-white shadow-inner " +
                "flex items-center justify-between font-mono text-sm");

            // ── 右側記錄資訊 ──
            string infoHtml;
            if (!string.IsNullOrEmpty(AlpineRecordInfo))
            {
                // Alpine.js 動態表達式
                infoHtml = $"<div class=\"text-xs text-slate-500 ml-4 shrink-0\" x-text=\"{HtmlAttr(AlpineRecordInfo)}\"></div>";
            }
            else if (!string.IsNullOrEmpty(RecordInfo))
            {
                infoHtml = $"<div class=\"text-xs text-slate-500 ml-4 shrink-0\">記錄: {HtmlEncode(RecordInfo)}</div>";
            }
            else
            {
                infoHtml = "<div class=\"text-xs text-slate-400 ml-4 shrink-0\">記錄: -</div>";
            }

            string html = $@"
<div class=""flex items-center gap-3 w-full"">
    <span class=""text-emerald-400 font-bold shrink-0"">訊息 &gt;</span>
    <input type=""text""
           id=""{HtmlEncode(MsgId)}""
           class=""bg-slate-900 border border-slate-700 rounded px-3 py-1 flex-1
                  text-slate-300 focus:outline-none focus:border-indigo-500 shadow-inner""
           value=""{HtmlEncode(DefaultMsg)}"" readonly>
</div>
{infoHtml}";

            output.Content.SetHtmlContent(html);
        }

        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string? s)   => s?.Replace("\"", "&quot;") ?? string.Empty;
    }
}
