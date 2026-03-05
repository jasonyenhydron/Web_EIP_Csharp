using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// Status badge with ping indicator and label text.
    /// Supports static text or Alpine.js expressions.
    /// </summary>
    [HtmlTargetElement("g-status-badge")]
    public class GStatusBadgeTagHelper : TagHelper
    {
        /// <summary>Status code text.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Status label text.</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Alpine expression for status code.</summary>
        public string AlpineCode { get; set; } = string.Empty;

        /// <summary>Alpine expression for status label.</summary>
        public string AlpineLabel { get; set; } = string.Empty;

        /// <summary>Badge color: amber | green | blue | red | slate.</summary>
        public string Color { get; set; } = "amber";

        /// <summary>Disable ping animation when true.</summary>
        public bool NoPing { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "inline-flex items-center gap-2");

            // Color palette mapping
            (string pingColor, string dotColor, string textColor, string bgColor, string borderColor) = Color?.ToLower() switch
            {
                "green" => ("uk-background-primary",  "uk-background-primary",  "text-green-700",  "uk-background-primary",  "border-green-200"),
                "blue"  => ("uk-background-primary",   "uk-background-primary",   "text-blue-700",   "uk-background-primary",   "border-blue-200"),
                "red"   => ("uk-background-muted",    "uk-background-muted",    "text-red-700",    "uk-background-muted",    "border-red-200"),
                "slate" => ("uk-background-muted",  "uk-background-muted",  "text-slate-700",  "uk-background-muted", "border-slate-200"),
                _       => ("uk-background-muted",  "uk-background-muted",  "text-amber-600",  "uk-background-muted",  "border-amber-200"),
            };

            string pingHtml = NoPing
                ? string.Empty
                : $@"<span class=""relative flex h-2.5 w-2.5"">
                    <span class=""animate-ping absolute inline-flex h-full w-full rounded-full {pingColor} opacity-75""></span>
                    <span class=""relative inline-flex rounded-full h-2.5 w-2.5 {dotColor}""></span>
                </span>";

            string codeText, labelText;
            if (!string.IsNullOrEmpty(AlpineCode))
            {
                codeText  = $"<span x-text=\"{HtmlAttr(AlpineCode)}\"></span>";
                labelText = !string.IsNullOrEmpty(AlpineLabel)
                    ? $"<span x-text=\"{HtmlAttr(AlpineLabel)}\"></span>"
                    : string.Empty;
            }
            else
            {
                codeText  = !string.IsNullOrEmpty(Code) ? HtmlEncode(Code) : string.Empty;
                labelText = !string.IsNullOrEmpty(Label) ? HtmlEncode(Label) : string.Empty;
            }

            string badgeContent = string.IsNullOrEmpty(codeText)
                ? labelText
                : $"{codeText} {labelText}".Trim();

            string badgeHtml = $@"<span class=""text-xs font-bold {textColor} {bgColor} px-2.5 py-1 rounded-full border {borderColor}"">{badgeContent}</span>";

            output.Content.SetHtmlContent(pingHtml + badgeHtml);
        }

        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string? s)   => s?.Replace("\"", "&quot;") ?? string.Empty;
    }
}




