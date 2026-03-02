using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * EipTabsTagHelper — 對應 jeasyui Tabs
 * 父元件：<eip-tabs id="myTabs" active-tab="0">
 * 子元件：<eip-tab title="Tab一" icon="info">...content...</eip-tab>
 *
 * 使用 Alpine.js 管理 active tab 狀態（不需額外 JavaScript）
 */
namespace Web_EIP_Csharp.Views.Components
{
    // ---- 子元件 Context（由子傳至父）----
    public class EipTabContext
    {
        public List<(string Title, string Icon, string Content)> Tabs { get; } = new();
    }

    // =============================================
    // 子元件 <eip-tab>
    // =============================================
    [HtmlTargetElement("eip-tab", ParentTag = "eip-tabs")]
    public class EipTabTagHelper : TagHelper
    {
        /// <summary>Tab 標籤文字</summary>
        public string Title { get; set; } = "Tab";

        /// <summary>Tab icon：info|edit|calendar|user|filter|clock|setting|code</summary>
        public string Icon { get; set; } = "";

        [HtmlAttributeNotBound]
        [ViewContext]
        public Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; } = default!;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var tabCtx = context.Items[typeof(EipTabContext)] as EipTabContext;
            var content = (await output.GetChildContentAsync()).GetContent();
            tabCtx?.Tabs.Add((Title, Icon, content));

            // 子元件不直接渲染自己
            output.SuppressOutput();
        }
    }

    // =============================================
    // 父元件 <eip-tabs>
    // =============================================
    [HtmlTargetElement("eip-tabs")]
    [RestrictChildren("eip-tab")]
    public class EipTabsTagHelper : TagHelper
    {
        /// <summary>預設顯示第幾個 Tab（0-based）</summary>
        public int ActiveTab { get; set; } = 0;

        /// <summary>額外 CSS class</summary>
        public string Class { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var tabCtx = new EipTabContext();
            context.Items[typeof(EipTabContext)] = tabCtx;

            // 執行子元件以收集 tab 資訊
            await output.GetChildContentAsync();

            var tabs    = tabCtx.Tabs;
            var alpineId = $"tabs_{Guid.NewGuid():N}";

            // ---- Tab 按鈕列表 ----
            var headers = new System.Text.StringBuilder();
            for (int i = 0; i < tabs.Count; i++)
            {
                var (title, icon, _) = tabs[i];
                var iconHtml = GetTabIcon(icon);
                headers.Append($"""
                    <button type="button"
                        @@click="active={i}"
                        :class="active==={i}
                            ? 'border-blue-600 text-blue-700 font-bold bg-white shadow-sm'
                            : 'border-transparent text-slate-500 hover:text-slate-700 hover:bg-slate-50'"
                        class="flex items-center gap-1.5 px-4 py-2.5 text-sm border-b-2 -mb-px transition-all whitespace-nowrap rounded-t-lg">
                        {iconHtml}
                        {title}
                    </button>
                """);
            }

            // ---- Tab 內容 ----
            var panels = new System.Text.StringBuilder();
            for (int i = 0; i < tabs.Count; i++)
            {
                var (_, _, content) = tabs[i];
                panels.Append($"""
                    <div x-show="active==={i}" x-cloak class="p-4">
                        {content}
                    </div>
                """);
            }

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden {Class}");
            output.Attributes.SetAttribute("x-data", $"{{ active: {ActiveTab} }}");
            output.Content.SetHtmlContent($"""
                <!-- Tabs Header -->
                <div class="flex flex-wrap gap-0.5 border-b border-slate-200 bg-slate-50/70 px-3 pt-2 overflow-x-auto">
                    {headers}
                </div>
                <!-- Tabs Body -->
                <div class="overflow-auto">
                    {panels}
                </div>
            """);
        }

        private static string GetTabIcon(string icon) => icon switch
        {
            "info"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>""",
            "edit"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>""",
            "calendar" => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>""",
            "user"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>""",
            "filter"   => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"/></svg>""",
            "clock"    => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>""",
            "setting"  => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/></svg>""",
            "code"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4"/></svg>""",
            _          => ""
        };
    }
}
