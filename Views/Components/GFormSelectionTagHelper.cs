using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// g-checkbox — 對應 jEasyUI Checkbox（checkgroup 內的單個核取方塊）
    /// <g-checkbox name="agree" label="我同意條款" value="1" checked="true" alpine-model="form.agree"/>
    /// </summary>
    [HtmlTargetElement("g-checkbox")]
    public class GCheckBoxTagHelper : TagHelper
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public string Value       { get; set; } = "1";
        public bool   Checked     { get; set; } = false;
        public string AlpineModel { get; set; } = "";
        public bool   Disabled    { get; set; } = false;
        public string Class       { get; set; } = "";
        public string Onchange    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gchk_{Guid.NewGuid():N}" : Id;
            var chkAttr  = Checked ? " checked" : "";
            var disAttr  = Disabled ? " disabled" : "";
            var xmodel   = !string.IsNullOrEmpty(AlpineModel) ? $@" x-model=""{AlpineModel}""" : "";
            var onChange  = !string.IsNullOrEmpty(Onchange) ? $@" onchange=""{Onchange}""" : "";

            output.TagName = "label";
            output.Attributes.SetAttribute("class", $"inline-flex items-center gap-2 cursor-pointer select-none {(Disabled ? "opacity-50 cursor-not-allowed" : "")} {Class}");
            output.Content.SetHtmlContent($@"
                <input type=""checkbox"" id=""{inputId}"" name=""{Name}"" value=""{Value}"" {chkAttr} {disAttr} {xmodel} {onChange}
                       class=""w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-400 transition-colors cursor-pointer"">
                <span class=""text-sm text-slate-700"">{Label}</span>
            ");
        }
    }

    /// <summary>
    /// g-radio-group — 對應 jEasyUI RadioGroup（單選群）
    /// <g-radio-group name="gender" label="性別" options="M:男,F:女" value="M" alpine-model="form.gender"/>
    /// layout: inline(預設) | stack
    /// </summary>
    [HtmlTargetElement("g-radio-group")]
    public class GRadioGroupTagHelper : TagHelper
    {
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public string Options     { get; set; } = "";  // "value:label,value:label,..."
        public string Value       { get; set; } = "";
        public string Layout      { get; set; } = "inline";  // inline | stack
        public string AlpineModel { get; set; } = "";
        public bool   Required    { get; set; } = false;
        public bool   Disabled    { get; set; } = false;
        public int    ColSpan     { get; set; } = 1;
        public string Class       { get; set; } = "";
        public string Onchange    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var required  = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var disAttr  = Disabled ? " disabled" : "";
            var onChange  = !string.IsNullOrEmpty(Onchange) ? $@" onchange=""{Onchange}""" : "";
            var layoutCls = Layout == "stack" ? "flex-col" : "flex-row flex-wrap";
            var sb = new System.Text.StringBuilder();

            foreach (var item in Options.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts  = item.Trim().Split(':', 2);
                var v      = parts[0].Trim();
                var t      = parts.Length > 1 ? parts[1].Trim() : v;
                var optId  = $"grg_{Guid.NewGuid():N}";
                var chkAttr = v == Value ? " checked" : "";
                var xmodel  = !string.IsNullOrEmpty(AlpineModel) ? $@" x-model=""{AlpineModel}""" : "";
                sb.Append($@"
                <label class=""inline-flex items-center gap-1.5 cursor-pointer"">
                    <input type=""radio"" id=""{optId}"" name=""{Name}"" value=""{v}"" {chkAttr} {disAttr} {xmodel} {onChange}
                           class=""w-4 h-4 text-blue-600 border-slate-300 focus:ring-blue-400 cursor-pointer"">
                    <span class=""text-sm text-slate-700"">{t}</span>
                </label>");
            }

            var labelHtml = !string.IsNullOrEmpty(Label)
                ? $@"<label class=""block text-xs font-semibold text-slate-600"">{Label}{required}</label>"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1.5 {colClass} {Class}");
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <div class=""flex {layoutCls} gap-4"">
                    {sb}
                </div>
            ");
        }
    }

    /// <summary>
    /// g-check-group — 對應 jEasyUI CheckGroup（多選群）
    /// <g-check-group name="roles[]" label="角色" options="1:管理員,2:使用者,3:訪客"/>
    /// </summary>
    [HtmlTargetElement("g-check-group")]
    public class GCheckGroupTagHelper : TagHelper
    {
        public string Name     { get; set; } = "";
        public string Label    { get; set; } = "";
        public string Options  { get; set; } = "";   // "value:label,..."
        public string Values   { get; set; } = "";   // 預選的 value（逗號分隔）
        public string Layout   { get; set; } = "inline";
        public bool   Disabled { get; set; } = false;
        public int    ColSpan  { get; set; } = 1;
        public string Class    { get; set; } = "";
        public string Onchange { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var colClass  = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var selected  = new HashSet<string>(Values.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()));
            var disAttr   = Disabled ? " disabled" : "";
            var onChange  = !string.IsNullOrEmpty(Onchange) ? $@" onchange=""{Onchange}""" : "";
            var layoutCls = Layout == "stack" ? "flex-col" : "flex-row flex-wrap";
            var sb = new System.Text.StringBuilder();

            foreach (var item in Options.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts   = item.Trim().Split(':', 2);
                var v       = parts[0].Trim();
                var t       = parts.Length > 1 ? parts[1].Trim() : v;
                var optId   = $"gckg_{Guid.NewGuid():N}";
                var chkAttr = selected.Contains(v) ? " checked" : "";
                sb.Append($@"
                <label class=""inline-flex items-center gap-1.5 cursor-pointer"">
                    <input type=""checkbox"" id=""{optId}"" name=""{Name}"" value=""{v}"" {chkAttr} {disAttr} {onChange}
                           class=""w-4 h-4 rounded text-blue-600 border-slate-300 focus:ring-blue-400 cursor-pointer"">
                    <span class=""text-sm text-slate-700"">{t}</span>
                </label>");
            }

            var labelHtml = !string.IsNullOrEmpty(Label)
                ? $@"<label class=""block text-xs font-semibold text-slate-600"">{Label}</label>"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1.5 {colClass} {Class}");
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <div class=""flex {layoutCls} gap-4"">
                    {sb}
                </div>
            ");
        }
    }

    /// <summary>
    /// g-slider — 對應 jEasyUI Slider（拖曳滑桿）
    /// <g-slider name="volume" label="音量" value="50" min="0" max="100" step="1"/>
    /// </summary>
    [HtmlTargetElement("g-slider")]
    public class GSliderTagHelper : TagHelper
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public int    Value       { get; set; } = 0;
        public int    Min         { get; set; } = 0;
        public int    Max         { get; set; } = 100;
        public int    Step        { get; set; } = 1;
        public bool   ShowValue   { get; set; } = true;
        public bool   Disabled    { get; set; } = false;
        public string AlpineModel { get; set; } = "";
        public int    ColSpan     { get; set; } = 1;
        public string Class       { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gsl_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var disAttr  = Disabled ? " disabled" : "";
            var xmodel   = !string.IsNullOrEmpty(AlpineModel) ? $@" x-model.number=""{AlpineModel}""" : "";
            var labelHtml= !string.IsNullOrEmpty(Label)
                ? $@"<label for=""{inputId}"" class=""block text-xs font-semibold text-slate-600"">{Label}</label>"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-2 {colClass} {Class}");
            output.Content.SetHtmlContent($@"
                <div class=""flex items-center justify-between"">
                    {labelHtml}
                    {(ShowValue ? $@"<span id=""{inputId}_val"" class=""text-xs font-bold text-blue-600"">{Value}</span>" : "")}
                </div>
                <input type=""range"" id=""{inputId}"" name=""{Name}"" value=""{Value}""
                       min=""{Min}"" max=""{Max}"" step=""{Step}"" {disAttr} {xmodel}
                       oninput=""document.getElementById('{inputId}_val').textContent=this.value""
                       class=""w-full h-2 bg-slate-200 rounded-full appearance-none cursor-pointer accent-blue-600 disabled:opacity-50"">
                <div class=""flex justify-between text-xs text-slate-400"">
                    <span>{Min}</span><span>{Max}</span>
                </div>
            ");
        }
    }

    /// <summary>
    /// g-rating — 對應 jEasyUI Rate（星級評分）
    /// <g-rating name="score" value="3" max="5" alpine-model="form.score"/>
    /// </summary>
    [HtmlTargetElement("g-rating")]
    public class GRatingTagHelper : TagHelper
    {
        public string Id          { get; set; } = "";
        public string Name        { get; set; } = "";
        public string Label       { get; set; } = "";
        public int    Value       { get; set; } = 0;
        public int    Max         { get; set; } = 5;
        public bool   Disabled    { get; set; } = false;
        public string AlpineModel { get; set; } = "";
        public string Class       { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"grt_{Guid.NewGuid():N}" : Id;
            var xdata    = !string.IsNullOrEmpty(AlpineModel)
                ? $@"x-data=""{{ score: {Value} }}"" x-model=""{AlpineModel}"""
                : $@"x-data=""{{ score: {Value} }}""";

            var labelHtml = !string.IsNullOrEmpty(Label)
                ? $@"<label class=""block text-xs font-semibold text-slate-600 mb-1"">{Label}</label>"
                : "";

            var stars = new System.Text.StringBuilder();
            for (int i = 1; i <= Max; i++)
            {
                stars.Append($@"
                <button type=""button"" {(Disabled ? "disabled" : "")}
                        @click=""score={i}""
                        @mouseover=""hover={i}"" @mouseleave=""hover=0""
                        class=""text-2xl transition-colors {(Disabled ? "cursor-default" : "cursor-pointer")} focus:outline-none"">
                    <span :class=""(hover||score)>={i}?'text-amber-400':'text-slate-300'"">★</span>
                </button>");
            }

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col {Class}");
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <div {xdata} x-data=""Object.assign($data, {{hover:0}})""
                     class=""flex items-center gap-0.5"">
                    <input type=""hidden"" id=""{inputId}"" name=""{Name}"" :value=""score"">
                    {stars}
                    <span class=""ml-2 text-sm text-slate-500"" x-text=""score + ' / {Max}'""></span>
                </div>
            ");
        }
    }

    /// <summary>
    /// g-filebox — 對應 jEasyUI FileBox（檔案上傳）
    /// <g-filebox name="attachment" label="附件" accept=".pdf,.xlsx" multiple="true"/>
    /// </summary>
    [HtmlTargetElement("g-filebox")]
    public class GFileBoxTagHelper : TagHelper
    {
        public string Id       { get; set; } = "";
        public string Name     { get; set; } = "";
        public string Label    { get; set; } = "";
        public string Accept   { get; set; } = "";
        public bool   Multiple { get; set; } = false;
        public bool   Required { get; set; } = false;
        public bool   Disabled { get; set; } = false;
        public int    ColSpan  { get; set; } = 1;
        public string Class    { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var inputId  = string.IsNullOrEmpty(Id) ? $"gfb_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var multiAttr= Multiple ? " multiple" : "";
            var disAttr  = Disabled ? " disabled" : "";
            var reqAttr  = Required ? " required" : "";
            var accAttr  = !string.IsNullOrEmpty(Accept) ? $@" accept=""{Accept}""" : "";
            var required  = Required ? @"<span class=""text-red-500 ml-0.5 font-bold"">*</span>" : "";
            var labelHtml = !string.IsNullOrEmpty(Label)
                ? $@"<label for=""{inputId}"" class=""block text-xs font-semibold text-slate-600 mb-1"">{Label}{required}</label>"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass} {Class}");
            // 先把含 > 的 JS 字串組合好，避免放在 $@"..." 字串中被誤判
            var onchangeJs  = "const lbl=document.getElementById('" + inputId + "_label');"
                            + "if(this.files.length){const c=this.files.length;lbl.textContent=c>1?c+'個檔案':this.files[0].name;}else{lbl.textContent='點擊選擇';}";
            var acceptSpan  = string.IsNullOrEmpty(Accept) ? "" : @"<span class=""text-xs text-slate-400 ml-1"">(" + Accept + ")</span>";
            var multiTxt    = Multiple ? "（可多選）" : "";
            var disLblCls   = Disabled ? "opacity-50 cursor-not-allowed" : "";
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <label for=""{inputId}""
                       class=""flex items-center gap-3 px-3 py-2.5 border-2 border-dashed border-slate-300 rounded-lg cursor-pointer hover:border-blue-400 hover:bg-blue-50 transition-colors group {disLblCls}"">
                    <svg class=""w-5 h-5 text-slate-400 group-hover:text-blue-500 shrink-0"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12""/>
                    </svg>
                    <span class=""text-sm text-slate-500 group-hover:text-blue-600"" id=""{inputId}_label"">
                        點擊選擇{multiTxt}{acceptSpan}
                    </span>
                    <input type=""file"" id=""{inputId}"" name=""{Name}"" {multiAttr} {disAttr} {reqAttr} {accAttr}
                           class=""sr-only""
                           onchange=""{onchangeJs}"">
                </label>
            ");
        }
    }

    /// <summary>
    /// g-tagbox — 對應 jEasyUI TagBox（標籤輸入框）
    /// <g-tagbox name="tags" label="標籤" value="Tag1,Tag2"/>
    /// </summary>
    [HtmlTargetElement("g-tagbox")]
    public class GTagBoxTagHelper : TagHelper
    {
        public string Id    { get; set; } = "";
        public string Name  { get; set; } = "";
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";    // 逗號分隔初始值
        public string Placeholder { get; set; } = "輸入後按 Enter 新增標籤";
        public bool   Disabled    { get; set; } = false;
        public int    ColSpan     { get; set; } = 1;
        public string Class       { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId   = string.IsNullOrEmpty(Id) ? $"gtb_{Guid.NewGuid():N}" : Id;
            var colClass = ColSpan switch { 2 => "col-span-2", 3 => "col-span-3", 4 => "col-span-4", _ => "col-span-1" };
            var initTags = string.IsNullOrWhiteSpace(Value) ? "[]" :
                "[" + string.Join(",", Value.Split(',').Select(t => $"'{t.Trim()}'")) + "]";
            var labelHtml= !string.IsNullOrEmpty(Label)
                ? $@"<label class=""block text-xs font-semibold text-slate-600 mb-1"">{Label}</label>"
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class", $"flex flex-col gap-1 {colClass} {Class}");
            output.Content.SetHtmlContent($@"
                {labelHtml}
                <div id=""{compId}"" x-data=""{{ tags: {initTags}, inp: '' }}""
                     class=""flex flex-wrap gap-1.5 p-2 border border-slate-300 rounded-lg focus-within:ring-2 focus-within:ring-blue-400 bg-white min-h-[40px]"">
                    <template x-for=""(tag, i) in tags"" :key=""i"">
                        <span class=""inline-flex items-center gap-1 px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded-full"">
                            <span x-text=""tag""></span>
                            {(Disabled ? "" : @"<button type=""button"" @click=""tags.splice(i,1)"" class=""hover:text-red-600 transition-colors leading-none"">×</button>")}
                        </span>
                    </template>
                    <input type=""text"" x-model=""inp"" placeholder=""{Placeholder}"" {(Disabled ? "disabled" : "")}
                           @keydown.enter.prevent=""if(inp.trim()&&!tags.includes(inp.trim())){{tags.push(inp.trim());inp='';}}""
                           @keydown.backspace=""if(!inp&&tags.length)tags.pop()""
                           class=""flex-1 min-w-[120px] border-0 outline-none text-sm bg-transparent py-0.5"">
                    <input type=""hidden"" name=""{Name}"" :value=""tags.join(','"">
                </div>
            ");
        }
    }
}
