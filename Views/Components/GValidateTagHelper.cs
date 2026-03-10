using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web_EIP_Csharp.Views.Components
{
    // ════════════════════════════════════════════════════════════════
    //  列舉
    // ════════════════════════════════════════════════════════════════

    /// <summary>重複檢查模式。</summary>
    public enum DuplicateCheckMode
    {
        /// <summary>在目前 BindingSource 內比對（適合 Master/Detail 的 Detail 表）。</summary>
        ByLocal,
        /// <summary>存檔前向後端執行 SELECT COUNT(*) 比對重複鍵值。</summary>
        ByWhere
    }

    /// <summary>合法性檢測模式。</summary>
    public enum ValidateMode
    {
        /// <summary>一次顯示一則警告（預設）。</summary>
        One,
        /// <summary>一次顯示全部警告。</summary>
        All
    }

    // ════════════════════════════════════════════════════════════════
    //  欄位規則模型
    // ════════════════════════════════════════════════════════════════

    /// <summary>單一欄位的預設值與合法性檢驗規則。</summary>
    public class GValidateColumn
    {
        /// <summary>要處理的欄位名稱，預設或檢查皆適用。</summary>
        public string FieldName { get; set; } = "";

        // ── 合法性檢查 ───────────────────────────────────────────────
        /// <summary>True 時檢查空值（含空字串、數值 0）。</summary>
        public bool CheckNull { get; set; } = false;

        /// <summary>合法性檢查類型（如 required、range、regex …）。</summary>
        public string ValidateType { get; set; } = "";

        /// <summary>自訂檢驗方法名稱，搭配 RemoteMethod 決定 C# 或 JS。</summary>
        public string CheckMethod { get; set; } = "";

        /// <summary>True = C# 方法；False = JS 方法。需搭配 CheckMethod 使用。</summary>
        public bool RemoteMethod { get; set; } = false;

        /// <summary>檢驗不通過時顯示給使用者的自訂提示語。</summary>
        public string ValidateMessage { get; set; } = "";

        /// <summary>範圍檢查起始值（單獨設定可達到「必須大於等於」效果）。</summary>
        public string RangeFrom { get; set; } = "";

        /// <summary>範圍檢查結束值（單獨設定可達到「必須小於等於」效果）。</summary>
        public string RangeTo { get; set; } = "";

        /// <summary>跨欄位比較目標欄位名稱。</summary>
        public string CompareField { get; set; } = "";

        /// <summary>跨欄位比較模式，如 after-field / before-field / gte-field / lte-field。</summary>
        public string CompareMode { get; set; } = "";

        /// <summary>
        /// 搭配 ValidateColor / ValidateChar，
        /// 指定對應的 Label 名稱（非 Grid 欄位必填）。
        /// </summary>
        public string ValidateLabelLink { get; set; } = "";

        // ── 預設值 ───────────────────────────────────────────────────
        /// <summary>欄位預設值，可填常數或函數；與 CarryOn 只能擇一。</summary>
        public string DefaultValue { get; set; } = "";

        /// <summary>
        /// True 時此欄位從最近一筆資料帶入，與 DefaultValue 互斥。
        /// 需父層 CarryOn = true 且 DefaultActive = true 才有效。
        /// </summary>
        public bool CarryOn { get; set; } = false;

        // ── 相容既有 runtime ────────────────────────────────────────
        /// <summary>自訂 validate 函式名稱，供前端 runtime 使用。</summary>
        public string Validate { get; set; } = "";

        /// <summary>驗證失敗訊息別名，供前端 runtime 使用。</summary>
        public string WarningMsg { get; set; } = "";
    }

    // ════════════════════════════════════════════════════════════════
    //  主 TagHelper：<g-validate>
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// &lt;g-validate&gt; — Client 端表單在 submit / button click 時，
    /// 對使用者輸入資料進行預設值填充與合法性檢查的統一驗證元件。
    /// </summary>
    [HtmlTargetElement("g-validate")]
    [RestrictChildren("validate-column")]
    public class GValidateTagHelper : TagHelper
    {
        /// <summary>元件名稱。</summary>
        public string Name { get; set; } = "";

        /// <summary>綁定的目標元件 ID。</summary>
        public string BindingObjectId { get; set; } = "";

        /// <summary>
        /// True 時記錄最近一筆新增／修改資料，下次新增時自動帶入部分欄位。
        /// 需搭配 DefaultActive = true；第一次新增時無效。
        /// </summary>
        public bool CarryOn { get; set; } = false;

        /// <summary>True 時在提交前檢查 Key 欄位是否為空。</summary>
        public bool CheckKeyFieldEmpty { get; set; } = false;

        /// <summary>True 時啟用欄位預設值功能；False 時所有欄位的預設設定全部無效。</summary>
        public bool DefaultActive { get; set; } = false;

        /// <summary>True 時啟用合法性檢查功能（預設 true）。</summary>
        public bool ValidActive { get; set; } = true;

        /// <summary>True 時在存檔前執行重複資料檢查。</summary>
        public bool DuplicateCheck { get; set; } = false;

        /// <summary>重複檢查模式：ByLocal 或 ByWhere。</summary>
        public DuplicateCheckMode DuplicateCheckMode { get; set; } = DuplicateCheckMode.ByLocal;

        /// <summary>合法性檢測模式：One（預設，一次一則）或 All（一次全部）。</summary>
        public ValidateMode ValidateMode { get; set; } = ValidateMode.One;

        /// <summary>True 時焦點離開欄位即觸發驗證。</summary>
        public bool LeaveValidation { get; set; } = false;

        /// <summary>驗證不通過時欄位標題變更的顏色（預設紅色 #dc2626）。</summary>
        public string ValidateColor { get; set; } = "#dc2626";

        /// <summary>驗證不通過時欄位標題前自動加上的字元（預設 *）。</summary>
        public string ValidateChar { get; set; } = "*";

        public override int Order => int.MinValue;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var columns = new List<GValidateColumn>();
            context.Items[typeof(GValidateTagHelper)] = columns;

            await output.GetChildContentAsync();

            output.TagName = null;
            output.Content.SetHtmlContent(BuildScriptBlock(this, columns));
        }

        private static string BuildScriptBlock(GValidateTagHelper th, List<GValidateColumn> columns)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                WriteIndented = false,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            var payload = new
            {
                name = OrNull(th.Name),
                bindingObjectId = th.BindingObjectId,
                carryOn = th.CarryOn ? (bool?)true : null,
                checkKeyFieldEmpty = th.CheckKeyFieldEmpty ? (bool?)true : null,
                defaultActive = th.DefaultActive ? (bool?)true : null,
                validActive = th.ValidActive,
                duplicateCheck = th.DuplicateCheck ? (bool?)true : null,
                duplicateCheckMode = th.DuplicateCheck ? th.DuplicateCheckMode.ToString() : null,
                validateMode = th.ValidateMode != ValidateMode.One ? th.ValidateMode.ToString() : null,
                leaveValidation = th.LeaveValidation ? (bool?)true : null,
                validateColor = th.ValidateColor != "#dc2626" ? th.ValidateColor : null,
                validateChar = th.ValidateChar != "*" ? th.ValidateChar : null,
                columns = columns.Select(c => new
                {
                    fieldName = c.FieldName,
                    checkNull = c.CheckNull ? (bool?)true : null,
                    remoteMethod = c.RemoteMethod ? (bool?)true : null,
                    carryOn = c.CarryOn ? (bool?)true : null,
                    validateType = OrNull(c.ValidateType),
                    validate = OrNull(c.Validate),
                    checkMethod = OrNull(c.CheckMethod),
                    validateMessage = OrNull(c.ValidateMessage),
                    warningMsg = OrNull(c.WarningMsg),
                    rangeFrom = OrNull(c.RangeFrom),
                    rangeTo = OrNull(c.RangeTo),
                    compareField = OrNull(c.CompareField),
                    compareMode = OrNull(c.CompareMode),
                    validateLabelLink = OrNull(c.ValidateLabelLink),
                    defaultValue = OrNull(c.DefaultValue),
                })
            };

            var json = JsonSerializer.Serialize(payload, options);
            var encoded = System.Net.WebUtility.HtmlEncode(th.BindingObjectId);

            return $"""<script type="application/json" data-g-validate="{encoded}">{json}</script>""";
        }

        private static string? OrNull(string s) =>
            string.IsNullOrWhiteSpace(s) ? null : s;
    }

    // ════════════════════════════════════════════════════════════════
    //  子標籤 TagHelper：<validate-column>
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// &lt;validate-column&gt; — 宣告單一欄位的預設值與合法性檢驗規則，
    /// 只能出現在 &lt;g-validate&gt; 內部。
    /// </summary>
    [HtmlTargetElement("validate-column", ParentTag = "g-validate")]
    public class GValidateColumnTagHelper : TagHelper
    {
        public string FieldName { get; set; } = "";
        public bool CheckNull { get; set; } = false;
        public string ValidateType { get; set; } = "";
        [HtmlAttributeName("validate")]
        public string Validate { get; set; } = "";
        public string CheckMethod { get; set; } = "";
        public string ValidateMessage { get; set; } = "";
        public string WarningMsg { get; set; } = "";
        public string RangeFrom { get; set; } = "";
        public string RangeTo { get; set; } = "";
        public string CompareField { get; set; } = "";
        public string CompareMode { get; set; } = "";
        public bool RemoteMethod { get; set; } = false;
        public string ValidateLabelLink { get; set; } = "";
        public string DefaultValue { get; set; } = "";
        public bool CarryOn { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.SuppressOutput();

            if (context.Items.TryGetValue(typeof(GValidateTagHelper), out var obj)
                && obj is List<GValidateColumn> columns)
            {
                columns.Add(new GValidateColumn
                {
                    FieldName = FieldName,
                    CheckNull = CheckNull,
                    ValidateType = ValidateType,
                    Validate = Validate,
                    CheckMethod = CheckMethod,
                    ValidateMessage = ValidateMessage,
                    WarningMsg = WarningMsg,
                    RangeFrom = RangeFrom,
                    RangeTo = RangeTo,
                    CompareField = CompareField,
                    CompareMode = CompareMode,
                    RemoteMethod = RemoteMethod,
                    ValidateLabelLink = ValidateLabelLink,
                    DefaultValue = DefaultValue,
                    CarryOn = CarryOn,
                });
            }
        }
    }
}
