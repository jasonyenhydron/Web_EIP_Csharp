using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// 自訂 Tag Helper：<g-dataform>
    /// 用途：根據指定欄位清單，自動渲染一個唯讀的資料展示表單。
    /// 支援 Alpine.js 的 x-text 動態綁定，欄位值來自指定的 JS 模型物件。
    /// 
    /// 使用範例：
    /// <g-dataform
    ///     id="myForm"
    ///     title="基本資料"
    ///     model="formData"
    ///     columns="Name:姓名,Age:年齡,Email:電子郵件"
    ///     horizontal-columns-count="3"
    ///     class=""
    ///     extra-class="mt-4" />
    /// </summary>
    [HtmlTargetElement("g-dataform")]
    public class GDataFormTagHelper : TagHelper
    {
        /// <summary>
        /// 表單容器的 HTML id。
        /// 若未指定，自動產生 gdf_{Guid} 格式的唯一 id。
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// 表單標題文字，顯示於表單頂部。
        /// 若為空則不渲染標題列。
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// Alpine.js 資料模型的變數名稱（JS 表達式字串）。
        /// 例如：model="formData" 會產生 x-text="formData['fieldName']"。
        /// 預設為 "null"，欄位將顯示空白。
        /// </summary>
        public string Model { get; set; } = "null";

        /// <summary>
        /// 欄位定義字串，以逗號分隔多個欄位。
        /// 格式：field1:Label1,field2:Label2,...
        /// 若省略 Label，則以 field 名稱作為顯示標籤。
        /// 例如："Name:姓名,Age:年齡,Email"
        /// </summary>
        public string Columns { get; set; } = "";

        /// <summary>
        /// 水平方向每列顯示的欄位數（Grid columns）。
        /// 最小值為 1，預設為 2。
        /// </summary>
        public int HorizontalColumnsCount { get; set; } = 2;

        /// <summary>
        /// 覆蓋預設 CSS class（會取代預設樣式）。
        /// 若為空，使用預設樣式：bg-white rounded-xl border border-slate-200 shadow-sm。
        /// </summary>
        public string Class { get; set; } = "";

        /// <summary>
        /// 附加的額外 CSS class（附加在預設或覆蓋樣式之後）。
        /// </summary>
        public string ExtraClass { get; set; } = "";

        /// <summary>
        /// Tag Helper 主處理方法，產生最終 HTML 輸出。
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // 若未提供 id，自動產生唯一識別碼
            var formId = string.IsNullOrWhiteSpace(Id)
                ? $"gdf_{Guid.NewGuid():N}"
                : Id;

            // 確保 grid 欄數至少為 1
            var gridCols = Math.Max(1, HorizontalColumnsCount);

            // 解析欄位定義字串
            var cols = ParseColumns(Columns);

            // 設定輸出的根元素為 <div>
            output.TagName = "div";
            output.Attributes.SetAttribute("id", formId);

            // 解析最終 class（支援覆蓋或附加）
            output.Attributes.SetAttribute("class", TagHelperClassResolver.Resolve(
                "bg-white rounded-xl border border-slate-200 shadow-sm",
                Class,
                ExtraClass));

            // 設定內部 HTML 內容
            output.Content.SetHtmlContent(BuildContent(cols, gridCols));
        }

        /// <summary>
        /// 建構表單內部的完整 HTML 字串。
        /// 包含：選擇性標題列 + Grid 欄位區塊。
        /// </summary>
        /// <param name="cols">已解析的欄位清單（Field, Label）</param>
        /// <param name="gridCols">Grid 欄數</param>
        /// <returns>完整 HTML 字串</returns>
        private string BuildContent(List<(string Field, string Label)> cols, int gridCols)
        {
            var sb = new StringBuilder();

            // 渲染標題列（僅在 Title 有值時輸出）
            if (!string.IsNullOrWhiteSpace(Title))
            {
                sb.Append($"<div class=\"px-4 py-3 border-b border-slate-200 bg-slate-100 text-sm font-bold text-slate-700\">" +
                          $"{System.Net.WebUtility.HtmlEncode(Title)}</div>");
            }

            // 開始 Grid 容器
            sb.Append($"<div class=\"p-4 grid gap-4\" style=\"grid-template-columns: repeat({gridCols}, minmax(0, 1fr));\">");

            // 逐一渲染每個欄位
            foreach (var (field, label) in cols)
            {
                // 對 JS 屬性名稱進行跳脫，防止 XSS 或語法錯誤
                var escapedField = JsEscape(field);

                // 對 Label 進行 HTML 編碼，防止 XSS
                var encodedLabel = System.Net.WebUtility.HtmlEncode(label);

                // 使用 Alpine.js x-text 動態綁定欄位值
                // 若 Model 或欄位值為 null，顯示空字串
                sb.Append($"""
                    <div class="flex flex-col gap-1">
                        <label class="text-xs font-semibold text-slate-500">{encodedLabel}</label>
                        <div class="min-h-9 px-3 py-2 rounded-lg border border-slate-200 bg-slate-100 text-sm text-slate-700 break-all"
                             x-text="({Model} && {Model}['{escapedField}'] != null) ? {Model}['{escapedField}'] : ''"></div>
                    </div>
                    """);
            }

            // 關閉 Grid 容器
            sb.Append("</div>");

            return sb.ToString();
        }

        /// <summary>
        /// 解析欄位定義字串，轉換為 (Field, Label) 元組清單。
        /// 格式：「field1:Label1,field2:Label2」
        /// - 以逗號分隔多個欄位
        /// - 以冒號分隔 field 名稱與顯示標籤
        /// - 若未提供 Label 或 Label 為空，使用 field 名稱作為 Label
        /// - 忽略空白的 field 項目
        /// </summary>
        /// <param name="columns">欄位定義字串</param>
        /// <returns>欄位清單</returns>
        private static List<(string Field, string Label)> ParseColumns(string columns)
        {
            var result = new List<(string Field, string Label)>();
            if (string.IsNullOrWhiteSpace(columns)) return result;

            foreach (var item in columns.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                // 限制只分割第一個冒號，避免 Label 含有冒號時被錯誤截斷
                var parts = item.Split(':', 2, StringSplitOptions.None);

                var field = parts[0].Trim();

                // 跳過空白欄位名稱
                if (string.IsNullOrEmpty(field)) continue;

                // 若 Label 未提供或為空，fallback 使用 field 名稱
                var label = parts.Length > 1 ? parts[1].Trim() : field;
                result.Add((field, string.IsNullOrEmpty(label) ? field : label));
            }

            return result;
        }

        /// <summary>
        /// 對 JavaScript 字串內容進行跳脫處理。
        /// 主要防止在 Alpine.js x-text 的單引號字串中發生語法錯誤或注入攻擊。
        /// 處理：反斜線 → \\，單引號 → \'
        /// </summary>
        /// <param name="input">原始字串</param>
        /// <returns>跳脫後的安全字串</returns>
        private static string JsEscape(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return input
                .Replace("\\", "\\\\")  // 反斜線必須先處理，避免後續替換被重複跳脫
                .Replace("'", "\\'");   // 單引號跳脫，保護 JS 字串邊界
        }
    }
}
