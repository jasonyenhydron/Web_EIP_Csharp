using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-empty-state")]
    public class GEmptyStateTagHelper : TagHelper
    {
        public string Title { get; set; } = "查無資料";
        public string Subtitle { get; set; } = "請調整查詢條件後再試一次";
        public string Icon { get; set; } = "document";
        [HtmlAttributeName("x-show")]
        public string? XShow { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "text-center py-12 text-slate-500");

            if (!string.IsNullOrEmpty(XShow))
                output.Attributes.SetAttribute("x-show", XShow);

            string iconSvg = GetIconSvg(Icon);

            string subtitleHtml = !string.IsNullOrEmpty(Subtitle)
                ? $"<p class=\"text-sm mt-1\">{HtmlEncode(Subtitle)}</p>"
                : string.Empty;

            output.Content.SetHtmlContent($@"
{iconSvg}
<p class=""text-lg font-medium"">{HtmlEncode(Title)}</p>
{subtitleHtml}");
        }

        private static string GetIconSvg(string icon) => icon?.ToLower() switch
        {
            "search" => @"<svg class=""w-12 h-12 mx-auto mb-3 text-slate-300"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/></svg>",
            "inbox" => @"<svg class=""w-12 h-12 mx-auto mb-3 text-slate-300"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4""/></svg>",
            "error" => @"<svg class=""w-12 h-12 mx-auto mb-3 text-slate-300"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""/></svg>",
            _ => @"<svg class=""w-12 h-12 mx-auto mb-3 text-slate-300"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z""/></svg>"
        };

        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    }
}


