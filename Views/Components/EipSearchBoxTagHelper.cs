using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * EipSearchBoxTagHelper — 對應 jeasyui ComboBox / SearchBox
 * 用法：
 *   <eip-search-box id="empSearch" name="employee_no" placeholder="輸入員工編號..."
 *                   api-url="/api/lov/hrm/employees"
 *                   display-field="employee_no" value-field="employee_id"
 *                   label-fields="employee_no,employee_name"
 *                   target-id="hidEmployeeId"/>
 *
 * api-url     : 查詢 API，需接受 ?query= 參數
 * display-field: 輸入框顯示的欄位名（input 的 value）
 * value-field  : 實際值欄位（寫入 target-id 的 hidden input）
 * label-fields : 下拉選項中顯示的欄位（逗號分隔）；第一個為主要、後面用小字顯示
 * target-id   : 儲存實際值的 hidden input ID
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("eip-search-box")]
    public class EipSearchBoxTagHelper : TagHelper
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Placeholder { get; set; } = "輸入關鍵字...";
        public string ApiUrl      { get; set; } = "";
        public string DisplayField { get; set; } = "";
        public string ValueField   { get; set; } = "";
        public string LabelFields  { get; set; } = "";  // "field1,field2"
        public string TargetId     { get; set; } = "";  // hidden input id
        public string Class        { get; set; } = "";
        public bool   Disabled     { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId    = string.IsNullOrEmpty(Id) ? $"sb_{Guid.NewGuid():N}" : Id;
            var fields    = LabelFields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(f => f.Trim()).ToList();
            // 產生選項 label 的 JS 片段
            var labelJs   = fields.Count switch
            {
                0 => "item[Object.keys(item)[0]]",
                1 => $"(item['{fields[0]}'] ?? '')",
                _ => $"(item['{fields[0]}'] ?? '') + ' <span class=\"text-slate-400 text-xs ml-2\">' + (item['{fields[1]}'] ?? '') + '</span>'"
            };
            var displayJs = string.IsNullOrEmpty(DisplayField) ? "item[Object.keys(item)[0]]" : $"item['{DisplayField}']";
            var valueJs   = string.IsNullOrEmpty(ValueField)   ? displayJs : $"item['{ValueField}']";
            var listId    = $"{compId}_list";
            var disAttr   = Disabled ? "disabled" : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"relative {Class}");
            output.Content.SetHtmlContent($"""
                <input type="text" id="{compId}" name="{Name}" placeholder="{Placeholder}" {disAttr}
                       autocomplete="off"
                       class="w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 bg-white pr-8 transition-all"
                       oninput="eipSearchBox_onInput('{compId}', '{listId}', '{ApiUrl}', '{TargetId}')">
                <span class="absolute inset-y-0 right-2 flex items-center text-slate-400 pointer-events-none">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
                    </svg>
                </span>
                <ul id="{listId}" role="listbox"
                    class="absolute z-[150] w-full mt-1 bg-white rounded-xl shadow-2xl border border-slate-200 hidden overflow-hidden max-h-52 overflow-y-auto">
                </ul>
                <script>
                (function(){{
                    const input  = document.getElementById('{compId}');
                    const list   = document.getElementById('{listId}');
                    const target = document.getElementById('{TargetId}');
                    let debounce;
                    window.eipSearchBox_onInput = window.eipSearchBox_onInput || {{}};
                    input.addEventListener('input', function(){{
                        clearTimeout(debounce);
                        const q = this.value.trim();
                        if(q.length < 1){{ list.classList.add('hidden'); return; }}
                        debounce = setTimeout(async () => {{
                            const res  = await fetch(`{ApiUrl}?query=${{encodeURIComponent(q)}}`);
                            const json = await res.json();
                            const data = json.data ?? json;
                            list.innerHTML = '';
                            if(!data.length){{
                                list.innerHTML = '<li class="px-4 py-3 text-sm text-slate-400">無符合資料</li>';
                            }} else {{
                                data.forEach(item => {{
                                    const li = document.createElement('li');
                                    li.className = 'px-4 py-2.5 text-sm cursor-pointer hover:bg-blue-50 hover:text-blue-700 transition-colors flex justify-between';
                                    li.innerHTML = `{labelJs.Replace("'", "\\'")}`;
                                    li.addEventListener('click', () => {{
                                        input.value = {displayJs};
                                        if(target) target.value = {valueJs};
                                        list.classList.add('hidden');
                                        input.dispatchEvent(new Event('change'));
                                    }});
                                    list.appendChild(li);
                                }});
                            }}
                            list.classList.remove('hidden');
                        }}, 300);
                    }});
                    document.addEventListener('click', e => {{ if(!input.contains(e.target) && !list.contains(e.target)) list.classList.add('hidden'); }});
                }})();
                </script>
            """);
        }
    }
}
