using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// g-lov-input：通用 LOV（List of Values）選擇輸入組合元件
    /// 產生：hidden id 欄 + code 顯示欄 + name 顯示欄，點擊任一顯示欄呼叫 lov-fn 函式。
    ///
    /// 使用方式（員工）：
    ///   <g-lov-input label="申請員工" required="true"
    ///                hidden-id="employeeId"    hidden-value="@ViewBag.NumericUserId"
    ///                code-id="employeeNo"      code-value="@ViewBag.UserId"    code-placeholder="員工編號"
    ///                name-id="employeeName"    name-value="@ViewBag.UserName"  name-placeholder="姓名"
    ///                lov-fn="openEmployeeLov('employeeId','employeeNo','employeeName')"
    ///                col-span="3" />
    ///
    /// 使用方式（假別，只有 display + button，無 name 欄）：
    ///   <g-lov-input label="假別" required="true"
    ///                hidden-id="leaveId"
    ///                name-id="leaveTypeDisplay"  name-placeholder="請選擇假別"
    ///                lov-fn="openLeaveTypeLov()"  show-button="true"
    ///                col-span="2" />
    /// </summary>
    [HtmlTargetElement("g-lov-input")]
    public class GLovInputTagHelper : TagHelper
    {
        // ── 標籤設定 ──
        /// <summary>欄位 Label 文字</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>是否必填（顯示紅色星號）</summary>
        public bool Required { get; set; } = false;

        /// <summary>Grid 欄位佔用數（md:col-span-N）</summary>
        public int ColSpan { get; set; } = 1;

        // ── Hidden 欄位 ──
        /// <summary>隱藏欄 ID（存 numeric ID，如 employee_id）</summary>
        public string HiddenId { get; set; } = string.Empty;

        /// <summary>隱藏欄預設值</summary>
        public string HiddenValue { get; set; } = string.Empty;

        // ── Code 欄位（左側窄欄，員工編號 / 假別代碼等）──
        /// <summary>Code 欄 ID；留空則不顯示 code 欄</summary>
        public string CodeId { get; set; } = string.Empty;

        /// <summary>Code 欄預設值</summary>
        public string CodeValue { get; set; } = string.Empty;

        /// <summary>Code 欄 placeholder</summary>
        public string CodePlaceholder { get; set; } = "代碼";

        /// <summary>Code 欄寬度 Tailwind class（預設 w-1/3）</summary>
        public string CodeWidth { get; set; } = "w-1/3";

        // ── Name 欄位（右側寬欄，姓名 / 名稱等）──
        /// <summary>Name 欄 ID；留空則不顯示 name 欄</summary>
        public string NameId { get; set; } = string.Empty;

        /// <summary>Name 欄預設值</summary>
        public string NameValue { get; set; } = string.Empty;

        /// <summary>Name 欄 placeholder</summary>
        public string NamePlaceholder { get; set; } = "名稱";

        // ── x-model 綁定 (Alpine.js) ──
        /// <summary>Code 欄 x-model 屬性值（留空則不加）</summary>
        public string XModelCode { get; set; } = string.Empty;

        /// <summary>Name 欄 x-model 屬性值（留空則不加）</summary>
        public string XModelName { get; set; } = string.Empty;

        /// <summary>Hidden 欄 x-model 屬性值（留空則不加）</summary>
        public string XModelHidden { get; set; } = string.Empty;

        // ── 行為 ──
        /// <summary>LOV 開啟 JS 表達式（整個 onclick 內容）</summary>
        public string LovFn { get; set; } = string.Empty;

        /// <summary>是否在右側顯示省略號按鈕（…），預設 false</summary>
        public bool ShowButton { get; set; } = false;

        /// <summary>是否唯讀（不允許手動輸入，預設 true）</summary>
        public bool Readonly { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            int colSpan = ColSpan < 1 ? 1 : ColSpan;
            string colClass = colSpan > 1 ? $"md:col-span-{colSpan}" : string.Empty;

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass}".Trim());

            // ── Label ──
            string required = Required
                ? " <span class=\"text-red-500 ml-0.5\">*</span>"
                : string.Empty;
            string labelHtml = $"<label class=\"text-xs font-semibold text-slate-600\">{HtmlEncode(Label)}{required}</label>";

            // ── Hidden field ──
            string hiddenHtml = string.Empty;
            if (!string.IsNullOrEmpty(HiddenId))
            {
                string xm = !string.IsNullOrEmpty(XModelHidden) ? $" x-model=\"{XModelHidden}\"" : string.Empty;
                hiddenHtml = $"<input type=\"hidden\" id=\"{HtmlId(HiddenId)}\" name=\"{HtmlEncode(HiddenId)}\"{xm} value=\"{HtmlEncode(HiddenValue)}\">";
            }

            // ── Input wrapper ──
            string onclick = !string.IsNullOrEmpty(LovFn) ? $" onclick=\"{HtmlAttr(LovFn)}\"" : string.Empty;

            // Code 欄（左側）
            string codeHtml = string.Empty;
            if (!string.IsNullOrEmpty(CodeId))
            {
                string rdAttr   = Readonly ? " readonly" : string.Empty;
                string xmAttr   = !string.IsNullOrEmpty(XModelCode) ? $" x-model=\"{XModelCode}\"" : string.Empty;
                bool   hasName  = !string.IsNullOrEmpty(NameId) || ShowButton;
                string rounded  = hasName ? "rounded-r-none" : "rounded-lg";
                codeHtml = $@"<input type=""text"" id=""{HtmlId(CodeId)}"" name=""{HtmlEncode(CodeId)}""
                       value=""{HtmlEncode(CodeValue)}""
                       placeholder=""{HtmlEncode(CodePlaceholder)}""{rdAttr}{onclick}{xmAttr}
                       class=""block {CodeWidth} px-3 py-2 border border-slate-300 {rounded} text-sm
                              text-blue-700 font-bold bg-blue-50 hover:bg-blue-100 cursor-pointer
                              focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors"">";
            }

            // Name 欄（右側）
            string nameHtml = string.Empty;
            if (!string.IsNullOrEmpty(NameId))
            {
                string rdAttr   = " readonly";
                string xmAttr   = !string.IsNullOrEmpty(XModelName) ? $" x-model=\"{XModelName}\"" : string.Empty;
                string hasCode  = !string.IsNullOrEmpty(CodeId) ? "rounded-l-none border-l-0" : "rounded-lg";
                string hasBtn   = ShowButton ? "rounded-r-none" : string.Empty;
                nameHtml = $@"<input type=""text"" id=""{HtmlId(NameId)}"" name=""{HtmlEncode(NameId)}""
                       value=""{HtmlEncode(NameValue)}""
                       placeholder=""{HtmlEncode(NamePlaceholder)}""{rdAttr}{onclick}{xmAttr}
                       class=""block flex-1 px-3 py-2 border border-slate-300 {hasCode} {hasBtn} text-sm
                              bg-slate-50 cursor-pointer hover:bg-slate-100
                              focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition-colors"">";
            }

            // 省略號按鈕
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

        private static string HtmlId(string s)    => System.Net.WebUtility.HtmlEncode(s);
        private static string HtmlEncode(string s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string s)   => s?.Replace("\"", "&quot;") ?? string.Empty;
    }
}
