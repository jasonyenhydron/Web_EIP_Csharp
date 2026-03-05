using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// Accordion container and panel tags.
    /// Example:
    /// <g-accordion>
    ///   <g-accordion-panel title="Section A" icon="info" active="true">...content...</g-accordion-panel>
    ///   <g-accordion-panel title="Section B" icon="setting">...content...</g-accordion-panel>
    /// </g-accordion>
    /// </summary>
    public class GAccordionContext
    {
        public List<(string Title, string Icon, bool Active, string Content)> Panels { get; } = new();
    }

    [HtmlTargetElement("g-accordion-panel", ParentTag = "g-accordion")]
    public class GAccordionPanelTagHelper : TagHelper
    {
        public string Title  { get; set; } = "";
        public string Icon   { get; set; } = "";
        public bool   Active { get; set; } = false;

        public override async Task ProcessAsync(TagHelperContext ctx, TagHelperOutput output)
        {
            var acc = ctx.Items[typeof(GAccordionContext)] as GAccordionContext;
            if (acc != null)
                acc.Panels.Add((Title, Icon, Active, (await output.GetChildContentAsync()).GetContent()));
            output.SuppressOutput();
        }
    }

    [HtmlTargetElement("g-accordion")]
    [RestrictChildren("g-accordion-panel")]
    public class GAccordionTagHelper : TagHelper
    {
        public bool   Exclusive { get; set; } = true;
        public string Class     { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var acc = new GAccordionContext();
            context.Items[typeof(GAccordionContext)] = acc;
            await output.GetChildContentAsync();

            var accId = $"gacc_{Guid.NewGuid():N}";
            var sb    = new System.Text.StringBuilder();

            for (int i = 0; i < acc.Panels.Count; i++)
            {
                var (title, icon, active, content) = acc.Panels[i];
                var panelId = $"{accId}_p{i}";
                var isOpen  = active || (i == 0 && !acc.Panels.Any(p => p.Active));
                var iconHtml = GPanelTagHelper_GetIcon(icon);

                sb.Append($@"
                <div class=""border border-slate-200 rounded-xl overflow-hidden {(i > 0 ? "mt-1" : "")}"">
                    <button type=""button""
                            onclick=""gAccordionToggle('{accId}','{panelId}',{(Exclusive ? "true" : "false")})""
                            class=""w-full flex items-center gap-2 px-4 py-3 bg-slate-100 transition-colors text-left"">
                        {iconHtml}
                        <span class=""text-sm font-bold text-slate-700 flex-1"">{title}</span>
                        <svg id=""{panelId}-arrow"" class=""w-4 h-4 text-slate-400 transition-transform duration-200{(isOpen ? "" : " rotate-180")}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 15l7-7 7 7""/>
                        </svg>
                    </button>
                    <div id=""{panelId}"" class=""overflow-hidden transition-all duration-300{(isOpen ? "" : " hidden")}"">
                        <div class=""p-4 border-t border-slate-100"">{content}</div>
                    </div>
                </div>");
            }

            // Toggle behavior script
            sb.Append($@"
            <script>
            function gAccordionToggle(accId, panelId, exclusive) {{
                const panel = document.getElementById(panelId);
                const arrow = document.getElementById(panelId + '-arrow');
                if (exclusive) {{
                    document.querySelectorAll('[id^=""' + accId + '_p""]').forEach(p => {{
                        if (p.id !== panelId) {{ p.classList.add('hidden'); }}
                        const a = document.getElementById(p.id + '-arrow');
                        if (a && p.id !== panelId) a.classList.add('rotate-180');
                    }});
                }}
                panel.classList.toggle('hidden');
                arrow.classList.toggle('rotate-180');
            }}
            </script>");

            output.TagName = "div";
            output.Attributes.SetAttribute("id", accId);
            output.Attributes.SetAttribute("class", $"space-y-0 {Class}");
            output.Content.SetHtmlContent(sb.ToString());
        }

        // Shared icon mapping
        private static string GPanelTagHelper_GetIcon(string icon) => icon switch
        {
            "info"     => @"<svg class=""w-4 h-4 text-blue-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""/></svg>",
            "setting"  => @"<svg class=""w-4 h-4 text-slate-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z""/><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 12a3 3 0 11-6 0 3 3 0 016 0z""/></svg>",
            "user"     => @"<svg class=""w-4 h-4 text-indigo-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z""/></svg>",
            "filter"   => @"<svg class=""w-4 h-4 text-purple-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z""/></svg>",
            _          => ""
        };
    }
}


