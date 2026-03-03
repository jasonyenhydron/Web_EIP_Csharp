using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-lov-input")]
    public class GLovInputTagHelper : TagHelper
    {
        public string Label { get; set; } = string.Empty;
        public bool Required { get; set; } = false;
        public int ColSpan { get; set; } = 1;

        public string HiddenId { get; set; } = string.Empty;
        public string HiddenValue { get; set; } = string.Empty;
        public string CodeId { get; set; } = string.Empty;
        public string CodeValue { get; set; } = string.Empty;
        public string CodePlaceholder { get; set; } = "請輸入代碼...";
        public string CodeWidth { get; set; } = "w-1/3";
        public string NameId { get; set; } = string.Empty;
        public string NameValue { get; set; } = string.Empty;
        public string NamePlaceholder { get; set; } = "請選擇資料";

        [HtmlAttributeName("x-model-code")]
        public string XModelCode { get; set; } = string.Empty;
        [HtmlAttributeName("x-model-name")]
        public string XModelName { get; set; } = string.Empty;
        [HtmlAttributeName("x-model-hidden")]
        public string XModelHidden { get; set; } = string.Empty;

        // Legacy mode: existing inline JS function call.
        [HtmlAttributeName("lov-fn")]
        public string LovFn { get; set; } = string.Empty;

        // Declarative mode (no extra JS function needed).
        [HtmlAttributeName("lov-title")]
        public string LovTitle { get; set; } = string.Empty;
        [HtmlAttributeName("lov-api")]
        public string LovApi { get; set; } = string.Empty;
        [HtmlAttributeName("lov-columns")]
        public string LovColumns { get; set; } = string.Empty; // e.g. "蝺刻?,?迂,ID"
        [HtmlAttributeName("lov-fields")]
        public string LovFields { get; set; } = string.Empty;  // e.g. "employee_no,employee_name,employee_id"
        [HtmlAttributeName("lov-key-hidden")]
        public string LovKeyHidden { get; set; } = string.Empty;
        [HtmlAttributeName("lov-key-code")]
        public string LovKeyCode { get; set; } = string.Empty;
        [HtmlAttributeName("lov-key-name")]
        public string LovKeyName { get; set; } = string.Empty;
        [HtmlAttributeName("lov-display-format")]
        public string LovDisplayFormat { get; set; } = string.Empty; // e.g. "{leave_id} - {leave_name}"
        [HtmlAttributeName("lov-on-confirm")]
        public string LovOnConfirm { get; set; } = string.Empty; // optional callback function name
        [HtmlAttributeName("lov-buffer-view")]
        public bool LovBufferView { get; set; } = true;
        [HtmlAttributeName("lov-page-size")]
        public int LovPageSize { get; set; } = 50;
        [HtmlAttributeName("lov-sort-enabled")]
        public bool LovSortEnabled { get; set; } = false;

        public bool ShowButton { get; set; } = false;
        public bool Readonly { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            int colSpan = ColSpan < 1 ? 1 : ColSpan;
            string colClass = colSpan > 1 ? $"md:col-span-{colSpan}" : string.Empty;

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass}".Trim());

            string required = Required ? " <span class=\"text-red-500 ml-0.5\">*</span>" : string.Empty;
            string labelHtml = $"<label class=\"text-xs font-semibold text-slate-600\">{HtmlEncode(Label)}{required}</label>";

            string hiddenHtml = string.Empty;
            if (!string.IsNullOrEmpty(HiddenId))
            {
                string xm = !string.IsNullOrEmpty(XModelHidden) ? $" x-model=\"{XModelHidden}\"" : string.Empty;
                hiddenHtml = $"<input type=\"hidden\" id=\"{HtmlId(HiddenId)}\" name=\"{HtmlEncode(HiddenId)}\"{xm} value=\"{HtmlEncode(HiddenValue)}\">";
            }

            string onclick = BuildOnClick();
            onclick = string.IsNullOrEmpty(onclick) ? string.Empty : $" onclick=\"{HtmlAttr(onclick)}\"";

            string codeHtml = string.Empty;
            if (!string.IsNullOrEmpty(CodeId))
            {
                string rdAttr = Readonly ? " readonly" : string.Empty;
                string xmAttr = !string.IsNullOrEmpty(XModelCode) ? $" x-model=\"{XModelCode}\"" : string.Empty;
                bool hasName = !string.IsNullOrEmpty(NameId) || ShowButton;
                string effectiveCodeWidth = hasName ? CodeWidth : "w-full";
                string rounded = hasName ? "rounded-r-none" : "rounded-lg";
                string codeInputStyle = Readonly
                    ? @"text-blue-700 font-bold bg-blue-50 hover:bg-blue-100 cursor-pointer"
                    : @"text-slate-700 bg-white";
                codeHtml = $@"<input type=""text"" id=""{HtmlId(CodeId)}"" name=""{HtmlEncode(CodeId)}""
                       value=""{HtmlEncode(CodeValue)}""
                       placeholder=""{HtmlEncode(CodePlaceholder)}""{rdAttr}{onclick}{xmAttr}
                       class=""block {effectiveCodeWidth} px-3 py-2 border border-slate-300 {rounded} text-sm
                              {codeInputStyle}
                              focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors"">";
            }

            string nameHtml = string.Empty;
            if (!string.IsNullOrEmpty(NameId))
            {
                string rdAttr = " readonly";
                string xmAttr = !string.IsNullOrEmpty(XModelName) ? $" x-model=\"{XModelName}\"" : string.Empty;
                string hasCode = !string.IsNullOrEmpty(CodeId) ? "rounded-l-none border-l-0" : "rounded-lg";
                string hasBtn = ShowButton ? "rounded-r-none" : string.Empty;
                nameHtml = $@"<input type=""text"" id=""{HtmlId(NameId)}"" name=""{HtmlEncode(NameId)}""
                       value=""{HtmlEncode(NameValue)}""
                       placeholder=""{HtmlEncode(NamePlaceholder)}""{rdAttr}{onclick}{xmAttr}
                       class=""block flex-1 px-3 py-2 border border-slate-300 {hasCode} {hasBtn} text-sm
                              bg-slate-50 cursor-pointer hover:bg-slate-100
                              focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors"">";
            }

            string btnHtml = string.Empty;
            if (ShowButton)
            {
                btnHtml = $@"<button type=""button""{onclick}
                        class=""px-3 border border-l-0 border-slate-300 rounded-r-lg
                               bg-slate-100 hover:bg-slate-200 text-slate-600 transition-colors"">
                    <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                              d=""M5 12h.01M12 12h.01M19 12h.01
                                 M6 12a1 1 0 11-2 0 1 1 0 012 0z
                                 m7 0a1 1 0 11-2 0 1 1 0 012 0z
                                 m7 0a1 1 0 11-2 0 1 1 0 012 0z""/>
                    </svg>
                </button>";
            }

            string inputsHtml = $"<div class=\"flex\">{hiddenHtml}{codeHtml}{nameHtml}{btnHtml}</div>";
            output.Content.SetHtmlContent(labelHtml + inputsHtml);
        }

        private string BuildOnClick()
        {
            if (!string.IsNullOrWhiteSpace(LovFn))
            {
                return LovFn;
            }

            if (string.IsNullOrWhiteSpace(LovApi) ||
                string.IsNullOrWhiteSpace(LovColumns) ||
                string.IsNullOrWhiteSpace(LovFields))
            {
                return string.Empty;
            }

            var cols = SplitCsv(LovColumns);
            var fields = SplitCsv(LovFields);
            if (cols.Count == 0 || fields.Count == 0) return string.Empty;

            var map = new StringBuilder();
            map.Append("{");
            bool first = true;

            if (!string.IsNullOrWhiteSpace(LovKeyHidden) && !string.IsNullOrWhiteSpace(HiddenId))
            {
                map.Append($"'{EscapeJs(LovKeyHidden)}':'{EscapeJs(HiddenId)}'");
                first = false;
            }
            if (!string.IsNullOrWhiteSpace(LovKeyCode) && !string.IsNullOrWhiteSpace(CodeId))
            {
                if (!first) map.Append(",");
                map.Append($"'{EscapeJs(LovKeyCode)}':'{EscapeJs(CodeId)}'");
                first = false;
            }
            if (!string.IsNullOrWhiteSpace(LovKeyName) && !string.IsNullOrWhiteSpace(NameId))
            {
                if (!first) map.Append(",");
                map.Append($"'{EscapeJs(LovKeyName)}':'{EscapeJs(NameId)}'");
                first = false;
            }
            var formattedTargetId = !string.IsNullOrWhiteSpace(NameId) ? NameId : CodeId;
            if (!string.IsNullOrWhiteSpace(LovDisplayFormat) && !string.IsNullOrWhiteSpace(formattedTargetId))
            {
                if (!first) map.Append(",");
                map.Append($"'FORMATTED_DISPLAY':'{EscapeJs(formattedTargetId)}'");
            }
            map.Append("}");

            var title = string.IsNullOrWhiteSpace(LovTitle) ? Label : LovTitle;
            var colsJs = "[" + string.Join(",", cols.Select(c => $"'{EscapeJs(c)}'")) + "]";
            var fieldsJs = "[" + string.Join(",", fields.Select(f => $"'{EscapeJs(f)}'")) + "]";
            var formatFn = BuildFormatFunction(LovDisplayFormat);
            var callback = string.IsNullOrWhiteSpace(LovOnConfirm) ? "null" : LovOnConfirm;
            var pageSize = LovPageSize <= 0 ? 50 : LovPageSize;
            var options = $"{{ bufferView: {(LovBufferView ? "true" : "false")}, pageSize: {pageSize}, sortEnabled: {(LovSortEnabled ? "true" : "false")} }}";

            return $"openGenericLov('{EscapeJs(title)}','{EscapeJs(LovApi)}',{colsJs},{fieldsJs},{map},{formatFn},{callback},{options})";
        }

        private static string BuildFormatFunction(string format)
        {
            if (string.IsNullOrWhiteSpace(format)) return "null";

            var sb = new StringBuilder();
            sb.Append("function(d){ return `");

            var s = format;
            int i = 0;
            while (i < s.Length)
            {
                if (s[i] == '{')
                {
                    int end = s.IndexOf('}', i + 1);
                    if (end > i + 1)
                    {
                        var key = s.Substring(i + 1, end - i - 1).Trim();
                        sb.Append("${d.").Append(EscapeTemplateKey(key)).Append(" ?? ''}");
                        i = end + 1;
                        continue;
                    }
                }
                if (s[i] == '`') sb.Append("\\`");
                else sb.Append(s[i]);
                i++;
            }

            sb.Append("`; }");
            return sb.ToString();
        }

        private static List<string> SplitCsv(string s)
            => s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        private static string EscapeJs(string s)
            => (s ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");

        private static string EscapeTemplateKey(string s)
            => string.IsNullOrWhiteSpace(s) ? "" : s.Replace("`", "").Replace("{", "").Replace("}", "").Replace(" ", "");

        private static string HtmlId(string s) => System.Net.WebUtility.HtmlEncode(s);
        private static string HtmlEncode(string s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string s) => s?.Replace("\"", "&quot;") ?? string.Empty;
    }
}

