using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * EipPanelTagHelper — 對應 jeasyui Panel
 * 用法：
 *   <eip-panel title="基本資訊" icon="info" collapsible="true" collapsed="false">
 *       ...content...
 *   </eip-panel>
 * icon: info | edit | calendar | user | filter | clock | setting | code
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("eip-panel")]
    public class EipPanelTagHelper : TagHelper
    {
        /// <summary>面板標題</summary>
        public string Title { get; set; } = "";

        /// <summary>標題 icon 名稱：info|edit|calendar|user|filter|clock|setting|code</summary>
        public string Icon { get; set; } = "";

        /// <summary>是否可收合</summary>
        public bool Collapsible { get; set; } = false;

        /// <summary>預設收合</summary>
        public bool Collapsed { get; set; } = false;

        /// <summary>額外 CSS class（作用於整個 panel）</summary>
        public string Class { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent();
            var panelId   = $"panel_{Guid.NewGuid():N}";
            var collapsed = Collapsed ? "true" : "false";
            var iconSvg   = GetIconSvg(Icon);
            var colBtn    = Collapsible
                ? $"""<button type="button" onclick="eipPanelToggle('{panelId}')" title="收合/展開" class="ml-auto text-slate-400 hover:text-slate-600 transition-colors p-1 rounded-lg hover:bg-slate-100"><svg id="{panelId}-arrow" class="w-4 h-4 transition-transform duration-200{(Collapsed ? " rotate-180" : "")}" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7"/></svg></button>"""
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden {Class}");
            output.Content.SetHtmlContent($"""
                <div class="flex items-center gap-2 px-4 py-3 bg-gradient-to-r from-slate-50 to-white border-b border-slate-200">
                    {iconSvg}
                    <span class="text-sm font-bold text-slate-700 flex-1">{Title}</span>
                    {colBtn}
                </div>
                <div id="{panelId}" class="p-4{(Collapsed ? " hidden" : "")}">
                    {content}
                </div>
            """);
        }

        private static string GetIconSvg(string icon) => icon switch
        {
            "info"     => """<svg class="w-4 h-4 text-blue-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>""",
            "edit"     => """<svg class="w-4 h-4 text-amber-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>""",
            "calendar" => """<svg class="w-4 h-4 text-green-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>""",
            "user"     => """<svg class="w-4 h-4 text-indigo-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>""",
            "filter"   => """<svg class="w-4 h-4 text-purple-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"/></svg>""",
            "clock"    => """<svg class="w-4 h-4 text-orange-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>""",
            "setting"  => """<svg class="w-4 h-4 text-slate-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/></svg>""",
            "code"     => """<svg class="w-4 h-4 text-cyan-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4"/></svg>""",
            _          => ""
        };
    }
}
