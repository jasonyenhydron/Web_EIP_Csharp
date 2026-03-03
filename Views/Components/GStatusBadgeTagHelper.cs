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
                "green" => ("bg-green-400",  "bg-green-500",  "text-green-700",  "bg-green-50",  "border-green-200"),
                "blue"  => ("bg-blue-400",   "bg-blue-500",   "text-blue-700",   "bg-blue-50",   "border-blue-200"),
                "red"   => ("bg-red-400",    "bg-red-500",    "text-red-700",    "bg-red-50",    "border-red-200"),
                "slate" => ("bg-slate-400",  "bg-slate-500",  "text-slate-700",  "bg-slate-100", "border-slate-200"),
                _       => ("bg-amber-400",  "bg-amber-500",  "text-amber-600",  "bg-amber-50",  "border-amber-200"),
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



