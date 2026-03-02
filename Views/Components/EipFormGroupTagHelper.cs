using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * EipFormGroupTagHelper — 對應 jeasyui Form field group
 * 用法：
 *   <eip-form-group label="員工姓名" required="true" help="請輸入全名">
 *       <input type="text" name="name" class="eip-input">
 *   </eip-form-group>
 *
 * col-span: 1~4（Grid 欄寬，配合外層 grid cols）
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("eip-form-group")]
    public class EipFormGroupTagHelper : TagHelper
    {
        /// <summary>欄位標籤</summary>
        public string Label { get; set; } = "";

        /// <summary>是否必填（顯示紅色星號）</summary>
        public bool Required { get; set; } = false;

        /// <summary>說明文字（顯示於欄位下方）</summary>
        public string Help { get; set; } = "";

        /// <summary>錯誤訊息（顯示錯誤樣式）</summary>
        public string Error { get; set; } = "";

        /// <summary>Grid 欄寬：1|2|3|4</summary>
        public int ColSpan { get; set; } = 1;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content  = (await output.GetChildContentAsync()).GetContent();
            var required = Required ? """<span class="text-red-500 font-bold ml-0.5">*</span>""" : "";
            var helpHtml = !string.IsNullOrEmpty(Help)
                ? $"""<p class="text-xs text-slate-400 mt-1">{Help}</p>"""
                : "";
            var errHtml  = !string.IsNullOrEmpty(Error)
                ? $"""<p class="text-xs text-red-500 mt-1 flex items-center gap-1"><svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>{Error}</p>"""
                : "";
            var colClass = ColSpan switch
            {
                2 => "col-span-2",
                3 => "col-span-3",
                4 => "col-span-4",
                _ => "col-span-1"
            };

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass}");
            output.Content.SetHtmlContent($"""
                <label class="block text-xs font-semibold text-slate-600">
                    {Label}{required}
                </label>
                {content}
                {helpHtml}
                {errHtml}
            """);
        }
    }
}
