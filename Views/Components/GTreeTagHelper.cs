using Microsoft.AspNetCore.Razor.TagHelpers;


namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-tree-leaf", ParentTag = "g-tree-node")]
    public class GTreeLeafTagHelper : TagHelper
    {
        public string Label   { get; set; } = "";
        public string Icon    { get; set; } = "file";   // file | circle
        public string Onclick { get; set; } = "";
        public string Class   { get; set; } = "";
        public string Title   { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var onclickAttr = !string.IsNullOrEmpty(Onclick)
                ? $"""onclick="{Onclick}" """
                : "";
            var titleAttr   = !string.IsNullOrEmpty(Title) ? $"""title="{Title}" """ : "";
            var cursor      = !string.IsNullOrEmpty(Onclick) ? "cursor-pointer" : "";
            var iconHtml    = GetLeafIcon(Icon);

            output.TagName = "div";
            output.Attributes.SetAttribute("class",
                $"flex items-center gap-2 px-3 py-1.5 rounded-lg text-sm text-slate-700 bg-blue-600 hover:text-blue-700 transition-colors {cursor} {Class}");
            if (!string.IsNullOrEmpty(Onclick)) output.Attributes.SetAttribute("onclick", Onclick);
            if (!string.IsNullOrEmpty(Title))   output.Attributes.SetAttribute("title", Title);
            output.Content.SetHtmlContent($"{iconHtml}<span class='truncate'>{Label}</span>");
        }

        private static string GetLeafIcon(string icon) => icon switch
        {
            "circle" => """<svg class="w-3.5 h-3.5 text-slate-400 shrink-0" fill="currentColor" viewBox="0 0 24 24"><circle cx="12" cy="12" r="4"/></svg>""",
            _ =>        """<svg class="w-3.5 h-3.5 text-slate-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/></svg>"""
        };
    }
    [HtmlTargetElement("g-tree-node", ParentTag = "g-tree")]
    public class GTreeNodeTagHelper : TagHelper
    {
        public string Label    { get; set; } = "";
        public string Icon     { get; set; } = "folder"; // folder | setting | code | user
        public bool   Expanded { get; set; } = false;
        public string Badge    { get; set; } = "";
        public string Onclick  { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content  = (await output.GetChildContentAsync()).GetContent();
            var nodeId   = $"gtn_{Guid.NewGuid():N}";
            var initOpen = Expanded;
            var iconHtml = GetNodeIcon(Icon);
            var badge    = !string.IsNullOrEmpty(Badge)
                ? $"""<span class="ml-auto text-xs bg-slate-100 text-slate-600 px-1.5 py-0.5 rounded-full font-semibold">{Badge}</span>"""
                : "";
            var hdrClick = !string.IsNullOrEmpty(Onclick)
                ? $"""ondblclick="{Onclick}" """
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "tree-node");
            output.Content.SetHtmlContent($"""
                <div class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-semibold text-slate-700 bg-slate-100 cursor-pointer transition-colors select-none group"
                     onclick="gTreeToggle('{nodeId}')" {hdrClick}>
                    <svg id="{nodeId}-arrow"
                         class="w-3.5 h-3.5 text-slate-400 shrink-0 transition-transform duration-150{(initOpen ? " rotate-90" : "")}"
                         fill="currentColor" viewBox="0 0 16 16">
                        <path d="M6 4l4 4-4 4V4z"/>
                    </svg>
                    {iconHtml}
                    <span class="flex-1 truncate">{Label}</span>
                    {badge}
                </div>
                <div id="{nodeId}" class="pl-4 overflow-hidden transition-all duration-200{(initOpen ? "" : " hidden")}">
                    {content}
                </div>
            """);
        }

        private static string GetNodeIcon(string icon) => icon switch
        {
            "folder"  => """<svg class="w-4 h-4 text-amber-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z"/></svg>""",
            "setting" => """<svg class="w-4 h-4 text-slate-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35..."/></svg>""",
            "code"    => """<svg class="w-4 h-4 text-cyan-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4"/></svg>""",
            "user"    => """<svg class="w-4 h-4 text-indigo-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>""",
            _         => """<svg class="w-4 h-4 text-amber-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z"/></svg>"""
        };
    }
    [HtmlTargetElement("g-tree")]
    [RestrictChildren("g-tree-node")]
    public class GTreeTagHelper : TagHelper
    {
        public string Id    { get; set; } = "";
        public string Class { get; set; } = "";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent();
            var idAttr  = !string.IsNullOrEmpty(Id) ? $"""id="{Id}" """ : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"g-tree overflow-auto {Class}");
            if (!string.IsNullOrEmpty(Id)) output.Attributes.SetAttribute("id", Id);
            output.Content.SetHtmlContent($"""<div class="py-2 space-y-0.5">{content}</div>""");
        }
    }
}


