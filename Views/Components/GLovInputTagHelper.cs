using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.RegularExpressions;
using Web_EIP_Csharp.Models.Lov;

namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-lov-input")]
    public class GLovInputTagHelper : TagHelper
    {
        private const string RuntimeInjectedKey = "__g_lov_runtime_injected";

        [Microsoft.AspNetCore.Mvc.ViewFeatures.ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }
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
        [HtmlAttributeName("id-prefix")]
        public string IdPrefix { get; set; } = string.Empty;

        // Legacy mode: existing inline JS function call.
        [HtmlAttributeName("lov-fn")]
        public string LovFn { get; set; } = string.Empty;
        [HtmlAttributeName("config")]
        public LovInputConfig? Config { get; set; }

        // Declarative mode (no extra JS function needed).
        [HtmlAttributeName("lov-title")]
        public string LovTitle { get; set; } = string.Empty;
        [HtmlAttributeName("lov-api")]
        public string LovApi { get; set; } = string.Empty;
        [HtmlAttributeName("lov-controller")]
        public string LovController { get; set; } = string.Empty;
        [HtmlAttributeName("lov-action")]
        public string LovAction { get; set; } = string.Empty;
        [HtmlAttributeName("lov-api-params")]
        public string LovApiParams { get; set; } = string.Empty;
        [HtmlAttributeName("lov-columns")]
        public string LovColumns { get; set; } = string.Empty; // e.g. "Code,Name,ID"
        [HtmlAttributeName("lov-fields")]
        public string LovFields { get; set; } = string.Empty;  // e.g. "employee_no,employee_name,employee_id"
        [HtmlAttributeName("lov-key-hidden")]
        public string LovKeyHidden { get; set; } = string.Empty;
        [HtmlAttributeName("lov-key-code")]
        public string LovKeyCode { get; set; } = string.Empty;
        [HtmlAttributeName("lov-key-name")]
        public string LovKeyName { get; set; } = string.Empty;
        [HtmlAttributeName("lov-return-value-field")]
        public string LovReturnValueField { get; set; } = string.Empty;
        [HtmlAttributeName("lov-return-display-field")]
        public string LovReturnDisplayField { get; set; } = string.Empty;
        [HtmlAttributeName("lov-display-format")]
        public string LovDisplayFormat { get; set; } = string.Empty; // e.g. "{leave_id} - {leave_name}"
        [HtmlAttributeName("lov-on-confirm")]
        public string LovOnConfirm { get; set; } = string.Empty; // optional callback function name
        [HtmlAttributeName("on-select")]
        public string OnSelect { get; set; } = string.Empty; // optional callback while row selected
        [HtmlAttributeName("column-matches")]
        public string ColumnMatches { get; set; } = string.Empty; // JSON array
        [HtmlAttributeName("filter-items")]
        public string FilterItems { get; set; } = string.Empty; // JSON array
        [HtmlAttributeName("lov-buffer-view")]
        public bool? LovBufferView { get; set; }
        [HtmlAttributeName("lov-page-size")]
        public int? LovPageSize { get; set; }
        [HtmlAttributeName("lov-sort-enabled")]
        public bool? LovSortEnabled { get; set; }
        [HtmlAttributeName("lov-request-mode")]
        public string LovRequestMode { get; set; } = "auto"; // auto | htmx | fetch
        [HtmlAttributeName("lov-name")]
        public string LovName { get; set; } = string.Empty; // frontend registry key
        [HtmlAttributeName("selectonly")]
        public bool SelectOnly { get; set; } = true;

        public bool ShowButton { get; set; } = true;
        public bool Readonly { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ApplyConfig();
            ApplyAutoBindingIds(context);
            ApplyAutoValues(context);
            EnsureRuntimeInjected(output);

            int colSpan = ColSpan < 1 ? 1 : ColSpan;
            string colClass = colSpan > 1 ? $"md:col-span-{colSpan}" : string.Empty;

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass}".Trim());
            output.Attributes.SetAttribute("data-glov-input", "1");
            if (!string.IsNullOrWhiteSpace(LovName))
            {
                output.Attributes.SetAttribute("data-lov-name", LovName.Trim());
            }

            string required = Required ? " <span class=\"text-red-500 ml-0.5\">*</span>" : string.Empty;
            string labelHtml = $"<label class=\"text-xs font-semibold text-slate-600\">{HtmlEncode(Label)}{required}</label>";

            string hiddenHtml = string.Empty;
            if (!string.IsNullOrEmpty(HiddenId))
            {
                string xm = !string.IsNullOrEmpty(XModelHidden) ? $" x-model=\"{XModelHidden}\"" : string.Empty;
                hiddenHtml = $"<input type=\"hidden\" id=\"{HtmlId(HiddenId)}\" name=\"{HtmlEncode(HiddenId)}\" data-lov-slot=\"hidden\"{xm} value=\"{HtmlEncode(HiddenValue)}\">";
            }

            string openCmd = BuildOnClick();
            string buttonOnclick = string.IsNullOrEmpty(openCmd) ? string.Empty : $" onclick=\"{HtmlAttr(openCmd)}\"";
            string inputOnInput = string.Empty;
            string inputOnKeydown = string.Empty;
            if (!SelectOnly)
            {
                inputOnKeydown = " onkeydown=\"if(event.key==='Enter'){event.preventDefault();window.gLov&&window.gLov.typeSearchInput&&window.gLov.typeSearchInput(this,true);} \"";
            }

            string codeHtml = string.Empty;
            if (!string.IsNullOrEmpty(CodeId))
            {
                // selectonly=false means user can type keyword in code textbox.
                string rdAttr = SelectOnly ? " readonly" : string.Empty;
                string xmAttr = !string.IsNullOrEmpty(XModelCode) ? $" x-model=\"{XModelCode}\"" : string.Empty;
                bool hasName = !string.IsNullOrEmpty(NameId) || ShowButton;
                string effectiveCodeWidth = hasName ? CodeWidth : "w-full";
                string rounded = hasName ? "rounded-l-lg rounded-r-none" : "rounded-lg";
                string codeInputStyle = SelectOnly
                    ? @"text-white font-bold bg-blue-600 cursor-pointer"
                    : @"text-slate-800 font-normal bg-white cursor-text";
                codeHtml = $@"<input type=""text"" id=""{HtmlId(CodeId)}"" name=""{HtmlEncode(CodeId)}"" data-lov-slot=""code""
                       value=""{HtmlEncode(CodeValue)}""
                       placeholder=""{HtmlEncode(CodePlaceholder)}""{rdAttr}{inputOnInput}{inputOnKeydown}{xmAttr}
                       class=""block {effectiveCodeWidth} px-3 py-2 border border-slate-300 {rounded} text-sm
                              {codeInputStyle}
                              focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors"">";
            }

            string nameHtml = string.Empty;
            if (!string.IsNullOrEmpty(NameId))
            {
                string rdAttr = " readonly";
                string xmAttr = !string.IsNullOrEmpty(XModelName) ? $" x-model=\"{XModelName}\"" : string.Empty;
                string hasCode = !string.IsNullOrEmpty(CodeId) ? "rounded-none border-l-0" : "rounded-lg";
                string hasBtn = ShowButton ? "rounded-r-none" : string.Empty;
                string nameInputStyle = "bg-slate-100 cursor-not-allowed text-slate-500";
                nameHtml = $@"<input type=""text"" id=""{HtmlId(NameId)}"" name=""{HtmlEncode(NameId)}"" data-lov-slot=""name""
                       value=""{HtmlEncode(NameValue)}""
                       placeholder=""{HtmlEncode(NamePlaceholder)}""{rdAttr}{xmAttr}
                       class=""block flex-1 px-3 py-2 border border-slate-300 {hasCode} {hasBtn} text-sm
                              {nameInputStyle}
                              focus:outline-none transition-colors"">";
            }

            string btnHtml = string.Empty;
            if (ShowButton)
            {
                btnHtml = $@"<button type=""button"" data-lov-open-btn=""1""{buttonOnclick}
                        class=""px-3 border border-l-0 border-slate-300 rounded-r-lg
                               bg-slate-100 text-slate-600 transition-colors"">
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

        private void ApplyAutoBindingIds(TagHelperContext context)
        {
            var hasAnyBindingId =
                !string.IsNullOrWhiteSpace(HiddenId) ||
                !string.IsNullOrWhiteSpace(CodeId) ||
                !string.IsNullOrWhiteSpace(NameId);
            if (hasAnyBindingId) return;

            var prefix = (IdPrefix ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = BuildIdPrefixFromLovNameOrApi();
            }
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = BuildIdPrefixFromLabel(Label);
            }
            if (string.IsNullOrWhiteSpace(prefix))
            {
                var uid = (context.UniqueId ?? "lov").Replace("-", "").Replace(":", "");
                prefix = $"lov{uid}";
            }

            HiddenId = $"{prefix}Id";
            CodeId = $"{prefix}Code";
            NameId = $"{prefix}Name";
        }

        private string BuildIdPrefixFromLovNameOrApi()
        {
            var source = (LovName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(source))
            {
                source = (LovApi ?? string.Empty).Trim();
            }
            if (string.IsNullOrWhiteSpace(source)) return string.Empty;

            source = Regex.Replace(source, "[^A-Za-z0-9]+", " ").Trim();
            if (string.IsNullOrWhiteSpace(source)) return string.Empty;
            var parts = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.Append(parts[0].ToLowerInvariant());
            for (var i = 1; i < parts.Length; i++)
            {
                var p = parts[i];
                sb.Append(char.ToUpperInvariant(p[0]));
                if (p.Length > 1) sb.Append(p.Substring(1).ToLowerInvariant());
            }
            return sb.ToString();
        }

        private void ApplyAutoValues(TagHelperContext context)
        {
            var isEmployeeLov =
                (LovApi ?? string.Empty).Contains("/employees", StringComparison.OrdinalIgnoreCase) ||
                (LovName ?? string.Empty).Contains("employee", StringComparison.OrdinalIgnoreCase);
            if (!isEmployeeLov) return;
            if (ViewContext?.ViewData == null) return;

            var hasHiddenValueAttr = context.AllAttributes.ContainsName("hidden-value");
            var hasCodeValueAttr = context.AllAttributes.ContainsName("code-value");
            var hasNameValueAttr = context.AllAttributes.ContainsName("name-value");

            if (!hasHiddenValueAttr && string.IsNullOrWhiteSpace(HiddenValue) &&
                ViewContext.ViewData.TryGetValue("NumericUserId", out var numericUserIdObj) &&
                numericUserIdObj != null)
            {
                HiddenValue = numericUserIdObj.ToString() ?? string.Empty;
            }

            if (!hasCodeValueAttr && string.IsNullOrWhiteSpace(CodeValue) &&
                ViewContext.ViewData.TryGetValue("UserId", out var userIdObj) &&
                userIdObj != null)
            {
                CodeValue = userIdObj.ToString() ?? string.Empty;
            }

            if (!hasNameValueAttr && string.IsNullOrWhiteSpace(NameValue) &&
                ViewContext.ViewData.TryGetValue("UserName", out var userNameObj) &&
                userNameObj != null)
            {
                NameValue = userNameObj.ToString() ?? string.Empty;
            }
        }

        private static string BuildIdPrefixFromLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label)) return string.Empty;
            var chars = label.Where(ch => ch <= 127 && char.IsLetterOrDigit(ch)).ToArray();
            if (chars.Length == 0) return string.Empty;

            var prefix = new string(chars);
            if (!char.IsLetter(prefix[0]))
            {
                prefix = "lov" + prefix;
            }
            return prefix;
        }
        private void EnsureRuntimeInjected(TagHelperOutput output)
        {
            var httpContext = ViewContext?.HttpContext;
            if (httpContext == null) return;
            if (httpContext.Items.ContainsKey(RuntimeInjectedKey)) return;

            httpContext.Items[RuntimeInjectedKey] = true;
            output.PostElement.AppendHtml(@"
<div id=""gLovHost""></div>
<script src=""/js/g-lov-modal-runtime.js""></script>");
        }
        private string BuildOnClick()
        {
            if (!string.IsNullOrWhiteSpace(LovFn))
            {
                return LovFn;
            }

            var api = BuildLovApiUrl();
            var formatFn = BuildFormatFunction(LovDisplayFormat);
            var callback = string.IsNullOrWhiteSpace(LovOnConfirm) ? "null" : LovOnConfirm;
            var onSelectJs = string.IsNullOrWhiteSpace(OnSelect) ? "null" : OnSelect;
            var columnMatchesJs = ToJsObjectOrArrayOrNull(ColumnMatches);
            var filterItemsJs = ToJsObjectOrArrayOrNull(FilterItems);
            var pageSize = (LovPageSize ?? 50) <= 0 ? 50 : (LovPageSize ?? 50);
            var bufferView = LovBufferView ?? true;
            var sortEnabled = LovSortEnabled ?? false;
            var requestMode = string.IsNullOrWhiteSpace(LovRequestMode) ? "auto" : LovRequestMode.Trim().ToLowerInvariant();
            if (requestMode != "htmx" && requestMode != "fetch") requestMode = "auto";
            var sourceInputId = EscapeJs(CodeId);
            var options = $"{{ bufferView: {(bufferView ? "true" : "false")}, pageSize: {pageSize}, sortEnabled: {(sortEnabled ? "true" : "false")}, requestMode: '{requestMode}', selectOnly: {(SelectOnly ? "true" : "false")}, sourceInputId: '{sourceInputId}' }}";

            if (string.IsNullOrWhiteSpace(api) ||
                string.IsNullOrWhiteSpace(LovColumns) ||
                string.IsNullOrWhiteSpace(LovFields))
            {
                if (string.IsNullOrWhiteSpace(LovName))
                {
                    return string.Empty;
                }
            }

            var map = new StringBuilder();
            map.Append("{");
            bool first = true;

            var keyHidden = !string.IsNullOrWhiteSpace(LovKeyHidden) ? LovKeyHidden : LovReturnValueField;
            var keyCode = !string.IsNullOrWhiteSpace(LovKeyCode) ? LovKeyCode : LovReturnValueField;
            var keyName = !string.IsNullOrWhiteSpace(LovKeyName) ? LovKeyName : LovReturnDisplayField;

            if (!string.IsNullOrWhiteSpace(keyHidden) && !string.IsNullOrWhiteSpace(HiddenId))
            {
                map.Append($"'{EscapeJs(keyHidden)}':'{EscapeJs(HiddenId)}'");
                first = false;
            }
            if (!string.IsNullOrWhiteSpace(keyCode) && !string.IsNullOrWhiteSpace(CodeId))
            {
                if (!first) map.Append(",");
                map.Append($"'{EscapeJs(keyCode)}':'{EscapeJs(CodeId)}'");
                first = false;
            }
            if (!string.IsNullOrWhiteSpace(keyName) && !string.IsNullOrWhiteSpace(NameId))
            {
                if (!first) map.Append(",");
                map.Append($"'{EscapeJs(keyName)}':'{EscapeJs(NameId)}'");
                first = false;
            }
            var formattedTargetId = !string.IsNullOrWhiteSpace(NameId) ? NameId : CodeId;
            if (!string.IsNullOrWhiteSpace(LovDisplayFormat) && !string.IsNullOrWhiteSpace(formattedTargetId))
            {
                if (!first) map.Append(",");
                map.Append($"'FORMATTED_DISPLAY':'{EscapeJs(formattedTargetId)}'");
            }
            map.Append("}");

            if (!string.IsNullOrWhiteSpace(LovName))
            {
                return $"gLov.openByName('{EscapeJs(LovName)}',{{ map:{map}, formatDisplay:{formatFn}, onConfirm:{callback}, onSelect:{onSelectJs}, columnMatches:{columnMatchesJs}, filterItems:{filterItemsJs}, options:{options} }})";
            }

            var cols = SplitCsv(LovColumns);
            var fields = SplitCsv(LovFields);
            if (cols.Count == 0 || fields.Count == 0) return string.Empty;
            var title = string.IsNullOrWhiteSpace(LovTitle) ? Label : LovTitle;
            var colsJs = "[" + string.Join(",", cols.Select(c => $"'{EscapeJs(c)}'")) + "]";
            var fieldsJs = "[" + string.Join(",", fields.Select(f => $"'{EscapeJs(f)}'")) + "]";
            return $"gLov.open({{ title:'{EscapeJs(title)}', api:'{EscapeJs(api)}', columns:{colsJs}, fields:{fieldsJs}, map:{map}, formatDisplay:{formatFn}, onConfirm:{callback}, onSelect:{onSelectJs}, columnMatches:{columnMatchesJs}, filterItems:{filterItemsJs}, options:{options} }})";
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

        private void ApplyConfig()
        {
            if (Config == null) return;

            if (string.IsNullOrWhiteSpace(LovTitle)) LovTitle = Config.Title ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LovApi)) LovApi = Config.Api ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LovColumns)) LovColumns = Config.Columns ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LovFields)) LovFields = Config.Fields ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LovKeyHidden)) LovKeyHidden = Config.KeyHidden ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LovKeyCode)) LovKeyCode = Config.KeyCode ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LovKeyName)) LovKeyName = Config.KeyName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LovDisplayFormat)) LovDisplayFormat = Config.DisplayFormat ?? string.Empty;
            if (string.IsNullOrWhiteSpace(LovOnConfirm)) LovOnConfirm = Config.OnConfirm ?? string.Empty;
            if (!LovBufferView.HasValue && Config.BufferView.HasValue) LovBufferView = Config.BufferView.Value;
            if (!LovPageSize.HasValue && Config.PageSize.HasValue) LovPageSize = Config.PageSize.Value;
            if (!LovSortEnabled.HasValue && Config.SortEnabled.HasValue) LovSortEnabled = Config.SortEnabled.Value;
        }

        private static List<string> SplitCsv(string s)
            => s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        private string BuildLovApiUrl()
        {
            var api = (LovApi ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(api)) return api;

            var controller = (LovController ?? string.Empty).Trim().Trim('/');
            var action = (LovAction ?? string.Empty).Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action)) return string.Empty;

            var path = $"/{controller}/{action}";
            var q = (LovApiParams ?? string.Empty).Trim().TrimStart('?', '&');
            return string.IsNullOrWhiteSpace(q) ? path : $"{path}?{q}";
        }

        private static string ToJsObjectOrArrayOrNull(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "null";
            var t = raw.Trim();
            if ((t.StartsWith("[") && t.EndsWith("]")) || (t.StartsWith("{") && t.EndsWith("}")))
            {
                return t;
            }
            return "null";
        }

        private static string EscapeJs(string s)
            => (s ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");

        private static string EscapeTemplateKey(string s)
            => string.IsNullOrWhiteSpace(s) ? "" : s.Replace("`", "").Replace("{", "").Replace("}", "").Replace(" ", "");

        private static string HtmlId(string s) => System.Net.WebUtility.HtmlEncode(s);
        private static string HtmlEncode(string s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string s) => s?.Replace("\"", "&quot;") ?? string.Empty;
    }
}
