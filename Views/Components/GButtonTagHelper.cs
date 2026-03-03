using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * GButtonTagHelper ??å°æ? jeasyui LinkButton
 * ?¨æ?ï¼?g-button text="?²å?" type="primary" icon="save" onclick="save()"/>
 *       <g-button text="è¿”å?" type="ghost" icon="close" href="/mis/programs"/>
 * type : primary | secondary | danger | warning | success | info | ghost
 * icon : save | trash | edit | search | plus | close | check | refresh |
 *        upload | download | print | eye | list | play | filter
 * size : sm | md | lg
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-button")]
    public class GButtonTagHelper : TagHelper
    {
        public string Text     { get; set; } = "";
        public string Type     { get; set; } = "primary";
        public string Icon     { get; set; } = "";
        public string Class    { get; set; } = "";
        public string ExtraClass { get; set; } = "";
        public string Onclick  { get; set; } = "";
        /// <summary>è¨­å?å¾Œè¼¸??&lt;a&gt; æ¨™ç±¤ï¼ˆé€???‰é?ï¼‰ï??¦å?è¼¸å‡º &lt;button&gt;</summary>
        public string Href     { get; set; } = "";
        /// <summary>a æ¨™ç±¤??target å±¬æ€§ï?å¦?"_blank"</summary>
        public string Target   { get; set; } = "";
        public string Id       { get; set; } = "";
        public string Size     { get; set; } = "md";
        public string Title    { get; set; } = "";
        public bool   Submit   { get; set; } = false;
        public bool   Disabled { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var colorClass = Type switch
            {
                "primary"   => "bg-blue-600 hover:bg-blue-700 text-white border-blue-700 shadow-blue-200",
                "secondary" => "bg-slate-100 hover:bg-slate-200 text-slate-700 border-slate-300",
                "danger"    => "bg-red-600 hover:bg-red-700 text-white border-red-700 shadow-red-200",
                "warning"   => "bg-amber-500 hover:bg-amber-600 text-white border-amber-600 shadow-amber-200",
                "success"   => "bg-green-600 hover:bg-green-700 text-white border-green-700 shadow-green-200",
                "info"      => "bg-cyan-600 hover:bg-cyan-700 text-white border-cyan-700 shadow-cyan-200",
                "ghost"     => "bg-transparent hover:bg-slate-100 text-slate-600 border-slate-300",
                _           => "bg-blue-600 hover:bg-blue-700 text-white border-blue-700"
            };
            var sizeClass = Size switch
            {
                "sm" => "px-2.5 py-1 text-xs gap-1",
                "lg" => "px-6 py-3 text-base gap-2",
                _    => "px-4 py-2 text-sm gap-1.5"
            };
            var disabledC = Disabled ? "opacity-50 cursor-not-allowed pointer-events-none" : "hover:scale-[1.02] active:scale-95";
            var iconHtml  = GetIconSvg(Icon);

            // ??href ?‚è¼¸??<a>ï¼Œå¦?‡è¼¸??<button>
            bool isLink = !string.IsNullOrEmpty(Href);
            output.TagName = isLink ? "a" : "button";

            if (isLink)
            {
                output.Attributes.SetAttribute("href", Href);
                if (!string.IsNullOrEmpty(Target)) output.Attributes.SetAttribute("target", Target);
            }
            else
            {
                output.Attributes.SetAttribute("type", Submit ? "submit" : "button");
                if (Disabled) output.Attributes.SetAttribute("disabled", "disabled");
            }

            var defaultClass = $"inline-flex items-center font-semibold rounded-lg border shadow-sm transition-all duration-150 {colorClass} {sizeClass} {disabledC}";
            var finalClass = TagHelperClassResolver.Resolve(defaultClass, Class, ExtraClass);
            output.Attributes.SetAttribute("class", finalClass);

            if (!string.IsNullOrEmpty(Id))      output.Attributes.SetAttribute("id", Id);
            if (!string.IsNullOrEmpty(Onclick)) output.Attributes.SetAttribute("onclick", Onclick);
            if (!string.IsNullOrEmpty(Title))   output.Attributes.SetAttribute("title", Title);

            output.Content.SetHtmlContent($"{iconHtml}{Text}");
        }

        public static string GetIconSvg(string icon, string cls = "w-4 h-4 shrink-0") => icon switch
        {
            "save"     => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4""/></svg>",
            "trash"    => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16""/></svg>",
            "edit"     => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z""/></svg>",
            "search"   => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/></svg>",
            "plus"     => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 4v16m8-8H4""/></svg>",
            "close"    => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/></svg>",
            "check"    => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 13l4 4L19 7""/></svg>",
            "refresh"  => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15""/></svg>",
            "upload"   => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12""/></svg>",
            "download" => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4""/></svg>",
            "print"    => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z""/></svg>",
            "eye"      => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 12a3 3 0 11-6 0 3 3 0 016 0z""/><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z""/></svg>",
            "list"     => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 6h16M4 10h16M4 14h16M4 18h16""/></svg>",
            "play"     => $@"<svg class=""{cls}"" fill=""currentColor"" viewBox=""0 0 24 24""><path d=""M8 5v14l11-7z""/></svg>",
            "filter"   => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z""/></svg>",
            "back"     => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M10 19l-7-7m0 0l7-7m-7 7h18""/></svg>",
            "forward"  => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M14 5l7 7m0 0l-7 7m7-7H3""/></svg>",
            "export"   => $@"<svg class=""{cls}"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z""/></svg>",
            _          => ""
        };
    }
}

