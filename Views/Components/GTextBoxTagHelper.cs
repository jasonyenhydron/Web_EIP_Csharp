using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * GTextBoxTagHelper — 對應 jeasyui TextBox (標籤 + 輸入框)
 * 用法：
 *   <g-textbox label="員工姓名" name="emp_name" type="text"
 *              placeholder="輸入姓名" required="true" alpine-model="form.empName"/>
 *
 * type : text | number | date | email | password | tel | textarea
 * col-span : 1~4 (配合外層 grid)
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-textbox")]
    public class GTextBoxTagHelper : TagHelper
    {
        public string Label       { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Id          { get; set; } = "";
        public string Type        { get; set; } = "text";   // text|number|date|email|password|tel|textarea
        public string Placeholder { get; set; } = "";
        public string Value       { get; set; } = "";
        public string AlpineModel { get; set; } = "";       // x-model="..."，Razor 中 @ 問題，改用此屬性
        public bool   Required    { get; set; } = false;
        public bool   Readonly    { get; set; } = false;
        public bool   Disabled    { get; set; } = false;
        public string Help        { get; set; } = "";
        public string Min         { get; set; } = "";
        public string Max         { get; set; } = "";
        public string Maxlength   { get; set; } = "";
        public int    ColSpan     { get; set; } = 1;
        public int    Rows        { get; set; } = 3;         // textarea 用
        public string Class       { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gt_{Guid.NewGuid():N}" : Id;
            var required = Required ? """<span class="text-red-500 ml-0.5 font-bold">*</span>""" : "";
            var helpHtml = !string.IsNullOrEmpty(Help)
                ? $"""<p class="text-xs text-slate-400 mt-1">{Help}</p>"""
                : "";
            var colClass  = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var disAttr   = Disabled ? " disabled" : "";
            var rdoAttr   = Readonly ? " readonly" : "";
            var reqAttr   = Required ? " required" : "";
            var xmodel    = !string.IsNullOrEmpty(AlpineModel) ? $""" x-model="{AlpineModel}" """ : "";
            var minAttr   = !string.IsNullOrEmpty(Min) ? $""" min="{Min}" """ : "";
            var maxAttr   = !string.IsNullOrEmpty(Max) ? $""" max="{Max}" """ : "";
            var maxlenAttr= !string.IsNullOrEmpty(Maxlength) ? $""" maxlength="{Maxlength}" """ : "";
            var extraCls  = Readonly ? " bg-slate-50 text-slate-500 cursor-not-allowed" : "";

            var inputHtml = Type switch
            {
                "textarea" => $"""
                               <textarea id="{inputId}" name="{Name}" rows="{Rows}"
                                   placeholder="{Placeholder}"{disAttr}{rdoAttr}{reqAttr}{xmodel}
                                   class="g-input w-full resize-y{extraCls} {Class}">{Value}</textarea>
                               """,
                _ => $"""
                      <input type="{Type}" id="{inputId}" name="{Name}"
                          placeholder="{Placeholder}" value="{Value}"
                          {disAttr}{rdoAttr}{reqAttr}{xmodel}{minAttr}{maxAttr}{maxlenAttr}
                          class="g-input w-full{extraCls} {Class}">
                      """
            };

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass}");
            output.Content.SetHtmlContent($"""
                <label for="{inputId}" class="block text-xs font-semibold text-slate-600">{Label}{required}</label>
                {inputHtml}
                {helpHtml}
            """);
        }
    }
}
