using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * EipButtonTagHelper — 對應 jeasyui LinkButton
 * 用法：
 *   <eip-button text="儲存" type="primary" icon="save" onclick="saveData()" id="btnSave"/>
 *   <eip-button text="刪除" type="danger"  icon="trash" onclick="confirmDelete()"/>
 *   <eip-button text="取消" type="secondary"/>
 *   <eip-button text="查詢" type="info"    icon="search" submit="true"/>
 *
 * type: primary | secondary | danger | warning | success | info | ghost
 * icon: save | trash | edit | search | plus | close | check | refresh | upload | download | print
 * size: sm | md | lg
 * submit: true → type="submit"，預設 button
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("eip-button")]
    public class EipButtonTagHelper : TagHelper
    {
        /// <summary>按鈕文字</summary>
        public string Text { get; set; } = "";

        /// <summary>樣式類型：primary|secondary|danger|warning|success|info|ghost</summary>
        public string Type { get; set; } = "primary";

        /// <summary>icon 名稱</summary>
        public string Icon { get; set; } = "";

        /// <summary>額外 CSS class</summary>
        public string Class { get; set; } = "";

        /// <summary>onclick 事件 JS</summary>
        public string Onclick { get; set; } = "";

        /// <summary>是否為 submit 按鈕</summary>
        public bool Submit { get; set; } = false;

        /// <summary>是否 disabled</summary>
        public bool Disabled { get; set; } = false;

        /// <summary>按鈕 ID</summary>
        public string Id { get; set; } = "";

        /// <summary>大小：sm | md | lg</summary>
        public string Size { get; set; } = "md";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var colorClass = Type switch
            {
                "primary"   => "bg-blue-600 hover:bg-blue-700 text-white border-blue-600 shadow-blue-500/30",
                "secondary" => "bg-slate-100 hover:bg-slate-200 text-slate-700 border-slate-200",
                "danger"    => "bg-red-600 hover:bg-red-700 text-white border-red-600 shadow-red-500/30",
                "warning"   => "bg-amber-500 hover:bg-amber-600 text-white border-amber-500 shadow-amber-500/30",
                "success"   => "bg-green-600 hover:bg-green-700 text-white border-green-600 shadow-green-500/30",
                "info"      => "bg-cyan-600 hover:bg-cyan-700 text-white border-cyan-600 shadow-cyan-500/30",
                "ghost"     => "bg-transparent hover:bg-slate-100 text-slate-600 border-slate-300",
                _           => "bg-blue-600 hover:bg-blue-700 text-white border-blue-600"
            };
            var sizeClass = Size switch
            {
                "sm" => "px-3 py-1.5 text-xs gap-1",
                "lg" => "px-6 py-3 text-base gap-2",
                _    => "px-4 py-2 text-sm gap-1.5"
            };
            var iconHtml  = GetIconSvg(Icon);
            var btnType   = Submit ? "submit" : "button";
            var disabledA = Disabled ? "disabled" : "";
            var disabledC = Disabled ? "opacity-50 cursor-not-allowed" : "hover:scale-[1.02] active:scale-95";
            var idAttr    = string.IsNullOrEmpty(Id) ? "" : $"""id="{Id}" """;
            var onclickA  = string.IsNullOrEmpty(Onclick) ? "" : $"""onclick="{Onclick}" """;

            output.TagName = "button";
            output.Attributes.SetAttribute("type", btnType);
            output.Attributes.SetAttribute("class",
                $"inline-flex items-center font-semibold rounded-lg border shadow-sm transition-all duration-150 {colorClass} {sizeClass} {disabledC} {Class}");
            if (!string.IsNullOrEmpty(Id))      output.Attributes.SetAttribute("id", Id);
            if (!string.IsNullOrEmpty(Onclick)) output.Attributes.SetAttribute("onclick", Onclick);
            if (Disabled)                        output.Attributes.SetAttribute("disabled", "disabled");

            output.Content.SetHtmlContent($"{iconHtml}{Text}");
        }

        private static string GetIconSvg(string icon) => icon switch
        {
            "save"     => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4"/></svg>""",
            "trash"    => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>""",
            "edit"     => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>""",
            "search"   => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>""",
            "plus"     => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>""",
            "close"    => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>""",
            "check"    => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/></svg>""",
            "refresh"  => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/></svg>""",
            "upload"   => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"/></svg>""",
            "download" => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/></svg>""",
            "print"    => """<svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z"/></svg>""",
            _          => ""
        };
    }
}
