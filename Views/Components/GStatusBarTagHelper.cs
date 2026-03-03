using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// g-status-bar嚗?撘??函????辣
    /// 瘛梯????航撓?交???渲????賂?隞?Oracle Forms ????    ///
    /// 雿輻?孵?嚗?    ///   <g-status-bar msg-id="statusBarMsg" record-info="1/1" default-msg="Ready." />
    ///
    /// JS ?湔閮嚗?    ///   document.getElementById('statusBarMsg').value = '鞈?撌脣摮?;
    /// </summary>
    [HtmlTargetElement("g-status-bar")]
    public class GStatusBarTagHelper : TagHelper
    {
        /// <summary>閮 input ??HTML id嚗? JS ?湔嚗?/summary>
        public string MsgId { get; set; } = "statusBarMsg";

        /// <summary>?喳閮?閮??嚗?憒?"1/1" ??"0 蝑?</summary>
        public string RecordInfo { get; set; } = string.Empty;

        /// <summary>閮甈?閮剜?摮?/summary>
        public string DefaultMsg { get; set; } = "Ready.";

        /// <summary>Alpine.js ??閮?閮銵券?撘????隞?record-info ???潘?</summary>
        public string AlpineRecordInfo { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class",
                "bg-slate-800 rounded-lg p-3 text-white shadow-inner " +
                "flex items-center justify-between font-mono text-sm");

            // ?? ?喳閮?鞈? ??
            string infoHtml;
            if (!string.IsNullOrEmpty(AlpineRecordInfo))
            {
                infoHtml = $"<div class=\"text-xs text-slate-500 ml-4 shrink-0\" x-text=\"{HtmlAttr(AlpineRecordInfo)}\"></div>";
            }
            else if (!string.IsNullOrEmpty(RecordInfo))
            {
                infoHtml = $"<div class=\"text-xs text-slate-500 ml-4 shrink-0\">閮?: {HtmlEncode(RecordInfo)}</div>";
            }
            else
            {
                infoHtml = "<div class=\"text-xs text-slate-400 ml-4 shrink-0\">閮?: -</div>";
            }

            string html = $@"
<div class=""flex items-center gap-3 w-full"">
    <span class=""text-emerald-400 font-bold shrink-0"">閮 &gt;</span>
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

