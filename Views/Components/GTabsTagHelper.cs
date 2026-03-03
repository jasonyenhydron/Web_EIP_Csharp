using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * GTabsTagHelper / GTabTagHelper ??Â∞çÊ? jeasyui Tabs
 * ?®Ê?Ôº? *   <g-tabs active-tab="0">
 *       <g-tab title="Ê∏ÖÂñÆ" icon="list">...content...</g-tab>
 *       <g-tab title="Á∂≠Ë≠∑" icon="edit">...content...</g-tab>
 *   </g-tabs>
 */
namespace Web_EIP_Csharp.Views.Components
{
    public class GTabContext
    {
        public List<(string Title, string Icon, string Content)> Tabs { get; } = new();
    }

    // ---- Â≠êÂ?‰ª?<g-tab> ----
    [HtmlTargetElement("g-tab", ParentTag = "g-tabs")]
    public class GTabTagHelper : TagHelper
    {
        public string Title { get; set; } = "Tab";
        public string Icon  { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var tabCtx  = context.Items[typeof(GTabContext)] as GTabContext;
            var content = (await output.GetChildContentAsync()).GetContent();
            tabCtx?.Tabs.Add((Title, Icon, content));
            output.SuppressOutput();
        }
    }

    // ---- ?∂Â?‰ª?<g-tabs> ----
    [HtmlTargetElement("g-tabs")]
    [RestrictChildren("g-tab")]
    public class GTabsTagHelper : TagHelper
    {
        public int    ActiveTab { get; set; } = 0;
        public string Class     { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var tabCtx = new GTabContext();
            context.Items[typeof(GTabContext)] = tabCtx;
            await output.GetChildContentAsync();

            var tabs    = tabCtx.Tabs;
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
                        {iconHtml}{title}
                    </button>
                """);
            }

            var panels = new System.Text.StringBuilder();
            for (int i = 0; i < tabs.Count; i++)
            {
                panels.Append($"""
                    <div x-show="active==={i}" x-cloak class="p-4">
                        {tabs[i].Content}
                    </div>
                """);
            }

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden {Class}");
            output.Attributes.SetAttribute("x-data", $"{{ active: {ActiveTab} }}");
            output.Content.SetHtmlContent($"""
                <div class="flex flex-wrap gap-0.5 border-b border-slate-200 bg-slate-50/70 px-3 pt-2 overflow-x-auto">
                    {headers}
                </div>
                <div>{panels}</div>
            """);
        }

        private static string GetTabIcon(string icon) => icon switch
        {
            "info"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>""",
            "edit"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/></svg>""",
            "list"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 10h16M4 14h16M4 18h16"/></svg>""",
            "calendar" => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>""",
            "user"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>""",
            "setting"  => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/></svg>""",
            "code"     => """<svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4"/></svg>""",
            _          => ""
        };
    }
}

