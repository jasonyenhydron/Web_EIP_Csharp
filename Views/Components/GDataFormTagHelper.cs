using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-dataform")]
    public class GDataFormTagHelper : TagHelper
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Model { get; set; } = "null";
        public string Columns { get; set; } = "";
        public int HorizontalColumnsCount { get; set; } = 2;
        public bool AlwaysReadOnly { get; set; } = true;
        public string Class { get; set; } = "";
        public string ExtraClass { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var formId = string.IsNullOrEmpty(Id) ? $"gdf_{Guid.NewGuid():N}" : Id;
            var cols = ParseColumns(Columns);
            var gridCols = HorizontalColumnsCount <= 1 ? 1 : HorizontalColumnsCount;

            var fieldsHtml = new StringBuilder();
            foreach (var col in cols)
            {
                var field = JsEscape(col.Field);
                var label = System.Net.WebUtility.HtmlEncode(col.Label);
                fieldsHtml.Append($@"
                <div class=""flex flex-col gap-1"">
                    <label class=""text-xs font-semibold text-slate-500"">{label}</label>
                    <div class=""min-h-9 px-3 py-2 rounded-lg border border-slate-200 uk-background-muted text-sm text-slate-700 break-all""
                         x-text=""({Model} && {Model}['{field}'] != null) ? {Model}['{field}'] : ''""></div>
                </div>");
            }

            var titleHtml = string.IsNullOrEmpty(Title)
                ? ""
                : $@"<div class=""px-4 py-3 border-b border-slate-200 uk-background-muted text-sm font-bold text-slate-700"">{System.Net.WebUtility.HtmlEncode(Title)}</div>";

            output.TagName = "div";
            output.Attributes.SetAttribute("id", formId);
            var defaultClass = "uk-background-default rounded-xl border border-slate-200 shadow-sm";
            var finalClass = TagHelperClassResolver.Resolve(defaultClass, Class, ExtraClass);
            output.Attributes.SetAttribute("class", finalClass);
            output.Content.SetHtmlContent($@"
                {titleHtml}
                <div class=""p-4 grid gap-4"" style=""grid-template-columns: repeat({gridCols}, minmax(0, 1fr));"">
                    {fieldsHtml}
                </div>");
        }

        private static List<(string Field, string Label)> ParseColumns(string columns)
        {
            var result = new List<(string Field, string Label)>();
            if (string.IsNullOrWhiteSpace(columns)) return result;

            foreach (var item in columns.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split(':', StringSplitOptions.None);
                var field = parts.ElementAtOrDefault(0)?.Trim() ?? "";
                var label = parts.ElementAtOrDefault(1)?.Trim() ?? field;
                if (!string.IsNullOrEmpty(field))
                {
                    result.Add((field, label));
                }
            }
            return result;
        }

        private static string JsEscape(string input)
            => (input ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
    }
}


