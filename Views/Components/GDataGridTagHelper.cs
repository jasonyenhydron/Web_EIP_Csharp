using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * GDataGridTagHelper — 對應 jeasyui DataGrid
 * 自包含 Alpine.js 元件，自動呼叫 API 並渲染表格 + 分頁 + 排序
 *
 * 用法：
 *   <g-datagrid id="programGrid"
 *               api-url="/api/mis/programs/IDMGD01/list"
 *               columns="PROGRAM_NO:程式代號:120,DISPLAY_CODE:顯示:60:center,PURPOSE:用途"
 *               page-size="20"
 *               striped="true"
 *               on-row-click="onRowSelected(row)"/>
 *
 * columns 格式（逗號分隔，每欄用冒號）：
 *   field:標題:寬度px:對齊(left|center|right)
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-datagrid")]
    public class GDataGridTagHelper : TagHelper
    {
        // --- 基本屬性保留原樣，搭配新屬性 ---
        public string Id           { get; set; } = "";
        [HtmlAttributeName("api")]
        public string Api          { get; set; } = "";
        public string ApiUrl       { get; set; } = "";
        public string RemoteName   { get; set; } = ""; // 取代 controller name
        [HtmlAttributeName("member-id")]
        public string DataMember   { get; set; } = ""; // 取代 action name

        public string Columns      { get; set; } = "";
        public int    PageSize     { get; set; } = 20;
        public bool   Striped      { get; set; } = true;

        public bool   ShowRowNum   { get; set; } = false;
        public bool   RowNumbers   { get; set; } = false;

        public string OnRowClick   { get; set; } = "";
        public string OnRowDblClick{ get; set; } = "";
        public string ToolbarHtml  { get; set; } = "";
        public string Class        { get; set; } = "";
        [HtmlAttributeName("clean-style")]
        public bool CleanStyle     { get; set; } = false;
        public string EditMode     { get; set; } = "";

        public string IdField      { get; set; } = "ROWID";
        public string IDField      { get; set; } = "";

        public bool   ShowFilter   { get; set; } = false;
        public bool   QueryAutoColumn { get; set; } = false;
        public bool   TitleFilterEnabled { get; set; } = true;

        //  --- 屬性 ---
        public bool   AllowAdd     { get; set; } = true;
        public bool   AllowDelete  { get; set; } = true;
        public bool   AllowUpdate  { get; set; } = true;
        public bool   AlwaysClose  { get; set; } = true;
        public bool   NotInitGrid  { get; set; } = true;
        public bool   AutoApply    { get; set; } = true;
        public string Title        { get; set; } = "";
        public string HelpLink     { get; set; } = "";
        // Keep legacy typo property for backward compatibility.
        public bool   ColumnsHibeable { get; set; } = false;
        public bool   ColumnsHideable { get; set; } = false;
        public bool   Pagination   { get; set; } = true;
        public string PageList     { get; set; } = "10,20,50,100";
        public string QueryMode    { get; set; } = "Panel";
        public string QueryTitle   { get; set; } = "查詢";
        public int    QueryLeft    { get; set; } = 0;
        public int    QueryTop     { get; set; } = 0;
        public string QueryColumns { get; set; } = "";
        public bool   MultiSelect  { get; set; } = false;
        public string EditDialogID { get; set; } = "";
        public bool   TitleSortEnabled { get; set; } = false;
        public string TitleSortField { get; set; } = "";
        public string SortableColumns { get; set; } = "";
        public bool   MultiSortEnabled { get; set; } = false;

        // --- 未實作/保留屬性 ---
        public bool   BufferView   { get; set; } = false;
        public bool   CheckOnSelect{ get; set; } = true;
        public string CloudReportName{ get; set; } = "";
        public string ReportFileName { get; set; } = "";
        public bool   DuplicateCheck { get; set; } = false;
        public bool   EditOnEnter  { get; set; } = false;
        public string MultiSelectGridID{ get; set; } = "";
        public string ParentObjectID { get; set; } = "";
        public string RelationColumns{ get; set; } = "";
        public bool   RecordLock   { get; set; } = false;
        public string RecordLockMode { get; set; } = "";
        // Keep legacy typo property for backward compatibility.
        public string TotalCpation { get; set; } = "";
        public string TotalCaption { get; set; } = "";
        public bool   UpdateCommandVisible { get; set; } = true;
        public bool   DeleteCommandVisible { get; set; } = true;
        public bool   ViewCommandVisible   { get; set; } = true;

        // --- JS 事件 Callback (Alpine.js integration) ---
        public string OnLoadSuccess{ get; set; } = ""; // 資料載入成功
        public string OnSelect     { get; set; } = ""; // 點擊列
        public string OnInsert     { get; set; } = ""; // 新增列
        public string OnInserted   { get; set; } = ""; // 新增列後
        public string OnUpdate     { get; set; } = ""; // 編輯列
        public string OnUpdated    { get; set; } = ""; // 編輯列後
        public string OnDelete     { get; set; } = ""; // 刪除列
        public string OnDeleted    { get; set; } = ""; // 刪除列後
        public string OnDeleting   { get; set; } = ""; // 刪除列前
        public string OnFilter     { get; set; } = ""; // 篩選
        public string OnView       { get; set; } = ""; // 檢視

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId   = string.IsNullOrEmpty(Id) ? $"grid_{Guid.NewGuid():N}" : Id;
            var fnName   = $"gDataGrid_{compId}";
            var striped  = Striped ? "odd:bg-white even:bg-slate-50/60" : "";
            var rowClick = !string.IsNullOrEmpty(OnRowClick)
                ? $"@click.stop=\"({OnRowClick.Replace("\"","&quot;")})(row)\""
                : "";
            var rowDbl   = !string.IsNullOrEmpty(OnRowDblClick)
                ? $"@dblclick.stop=\"({OnRowDblClick.Replace("\"","&quot;")})(row)\""
                : "";
            var rowCursor = (!string.IsNullOrEmpty(OnRowClick) || !string.IsNullOrEmpty(OnRowDblClick))
                ? "cursor-pointer" : "";

            // 解析 columns
            var cols = ParseColumns(Columns);
            var sortableFieldSet = ParseSortableFields(SortableColumns, cols);
            var sortableFields = cols
                .Where(c => sortableFieldSet.Contains(c.Field))
                .Select(c => c.Field)
                .Distinct()
                .ToList();
            var defaultTitleSortField = !string.IsNullOrEmpty(TitleSortField)
                ? TitleSortField
                : (sortableFields.FirstOrDefault() ?? cols.FirstOrDefault().Field ?? "");
            var thHtml = new System.Text.StringBuilder();
            var filterHtml = new System.Text.StringBuilder();
            var tdHtml = new System.Text.StringBuilder();

            // Handle JQGrid Mappings
            var actualApiUrl = !string.IsNullOrEmpty(ApiUrl) ? ApiUrl : Api;
            if (string.IsNullOrEmpty(actualApiUrl) && !string.IsNullOrEmpty(RemoteName))
            {
                // mvc 模式 由controller action crud select update insert delete 組成url
                var tableParam = !string.IsNullOrEmpty(DataMember) ? $"?DataMember={DataMember}" : "";
                actualApiUrl = $"/{RemoteName}/select{tableParam}".Replace("//", "/");
            }
            var actualShowRowNum = RowNumbers || ShowRowNum;
            var actualShowFilter = QueryAutoColumn || ShowFilter;
            var actualTitleFilterEnabled = actualShowFilter && TitleFilterEnabled;
            var actualIdField = !string.IsNullOrEmpty(IDField) ? IDField : IdField;
            var actualNotInitGrid = AlwaysClose || NotInitGrid;
            _ = ColumnsHibeable || ColumnsHideable; // reserved option, not rendered yet
            var actualTotalCaption = !string.IsNullOrEmpty(TotalCaption) ? TotalCaption : TotalCpation;

            // --- Option: Title ---
            var titleHeaderHtml = "";
            if (!string.IsNullOrEmpty(Title))
            {
                var helpIcon = !string.IsNullOrEmpty(HelpLink) ? $"<a href='{HelpLink}' target='_blank' class='text-slate-400 hover:text-blue-500 transition-colors' title='Help'><svg class='w-5 h-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg></a>" : "";
                var titleSortButtons = TitleSortEnabled
                    ? @"<div class='inline-flex items-center rounded-lg border border-slate-200 bg-white overflow-hidden'>
                            <button type='button' @click='setTitleSort(""asc"")' :class='(sortKey===titleSortField && sortDir===""asc"") ? ""text-blue-600 bg-blue-50"" : ""text-slate-500 hover:bg-slate-50""' class='w-8 h-8 inline-flex items-center justify-center transition-colors' title='升冪'>
                                <svg class='w-4 h-4' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 15l7-7 7 7'/></svg>
                            </button>
                            <button type='button' @click='setTitleSort(""desc"")' :class='(sortKey===titleSortField && sortDir===""desc"") ? ""text-blue-600 bg-blue-50"" : ""text-slate-500 hover:bg-slate-50""' class='w-8 h-8 inline-flex items-center justify-center border-l border-slate-200 transition-colors' title='降冪'>
                                <svg class='w-4 h-4' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'/></svg>
                            </button>
                        </div>"
                    : "";
                titleHeaderHtml = $"<div class='px-4 py-3 bg-slate-100 border-b border-slate-200 flex justify-between items-center'><h3 class='font-bold text-slate-700 text-sm flex items-center gap-2'><svg class='w-4 h-4 text-blue-600' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M4 6h16M4 10h16M4 14h16M4 18h16'/></svg>{Title}</h3><div class='flex items-center gap-2'>{titleSortButtons}{helpIcon}</div></div>";
            }

            if (actualShowRowNum)
            {
                thHtml.Append(@"<th class=""px-3 py-2.5 text-center text-xs font-bold text-slate-500 uppercase tracking-wider bg-slate-100 border-b-2 border-slate-200 w-10 shrink-0"">#</th>");
                if (actualShowFilter)
                {
                    filterHtml.Append(@"<th class=""px-3 py-2 bg-indigo-50/50 border-b-2 border-slate-200""></th>");
                }
                tdHtml.Append(@"<td class=""px-3 py-2 text-center text-xs text-slate-400 border-b border-slate-100"" x-text=""(currentPage-1)*pageSize+rowIdx+1""></td>");
            }

            if (MultiSelect)
            {
                thHtml.Append(@"<th class=""px-3 py-2.5 text-center bg-slate-100 border-b-2 border-slate-200 w-10 shrink-0""><input type=""checkbox"" @change=""toggleAll($event.target.checked)"" class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500 cursor-pointer""></th>");
                if (actualShowFilter)
                {
                    filterHtml.Append(@"<th class=""px-3 py-2 bg-indigo-50/50 border-b-2 border-slate-200""></th>");
                }
                tdHtml.Append($@"<td class=""px-3 py-2 text-center border-b border-slate-100"" @click.stop><input type=""checkbox"" :value=""row['{actualIdField}']"" x-model=""selectedIds"" @change=""onSelectRow(row, $event.target.checked)"" class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500 cursor-pointer""></td>");
            }

            foreach (var col in cols)
            {
                var wStyle  = string.IsNullOrEmpty(col.Width) ? "" : $"width:{col.Width}px;min-width:{col.Width}px;";
                var thAlign = col.Align is "center" or "right" ? $"text-{col.Align}" : "text-left";
                var tdAlign = col.Align is "center" or "right" ? $"text-{col.Align}" : "text-left";
                var isSortable = sortableFieldSet.Contains(col.Field);
                var sortableHeaderClass = isSortable ? "cursor-pointer hover:bg-slate-200" : "cursor-default";

                // --- 1. 表頭 (Header) ---
                var filterIconBtn = actualTitleFilterEnabled
                    ? $@"<button type=""button"" @click.stop=""openHeaderFilter('{col.Field}', $event)"" title=""欄位過濾""
                               class=""ml-1 inline-flex items-center justify-center w-5 h-5 rounded hover:bg-blue-100 text-blue-600 transition-colors"">
                            <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><use href=""#icon-filter""></use></svg>
                       </button>"
                    : "";

                var sortIconBtns = $@"
                            <span x-show=""isSortableField('{col.Field}')"" class=""inline-flex items-center rounded border border-slate-200 bg-white overflow-hidden"">
                                <button type=""button"" @click.stop=""setSort('{col.Field}', 'asc')"" title=""升冪""
                                        :class=""isSorted('{col.Field}', 'asc') ? 'text-blue-600 bg-blue-50' : 'text-slate-400 hover:bg-slate-100'""
                                        class=""w-4 h-4 inline-flex items-center justify-center transition-colors"">
                                    <svg class=""w-3 h-3"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 15l7-7 7 7""/>
                                    </svg>
                                </button>
                                <button type=""button"" @click.stop=""setSort('{col.Field}', 'desc')"" title=""降冪""
                                        :class=""isSorted('{col.Field}', 'desc') ? 'text-blue-600 bg-blue-50' : 'text-slate-400 hover:bg-slate-100'""
                                        class=""w-4 h-4 inline-flex items-center justify-center border-l border-slate-200 transition-colors"">
                                    <svg class=""w-3 h-3"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M19 9l-7 7-7-7""/>
                                    </svg>
                                </button>
                            </span>";

                thHtml.Append($@"<th @dblclick=""toggleSort('{col.Field}')"" style=""{wStyle}""
                        class=""px-3 py-2.5 {thAlign} text-xs font-bold text-slate-500 uppercase tracking-wider bg-slate-100 border-b-2 border-slate-200 {sortableHeaderClass} transition-colors select-none whitespace-nowrap group"">
                        <span class=""inline-flex items-center gap-1"">
                            {col.Title}
                            <span x-show=""multiSortEnabled && getSortOrder('{col.Field}') > 0""
                                  x-text=""`↑${{getSortOrder('{col.Field}')}}`""
                                  class=""text-[10px] text-slate-600 font-semibold""></span>
                            {sortIconBtns}
                            {filterIconBtn}
                        </span>
                    </th>");

                // --- 2. 過濾列 (Filter) ---
                if (ShowFilter)
                {
                    filterHtml.Append($@"<th class=""px-2 py-1.5 bg-indigo-50/50 border-b border-slate-200"">");
                    if (col.FilterType == "text")
                    {
                        filterHtml.Append($@"<input type=""text"" x-model=""filters['{col.Field}']"" @keydown.enter=""applyFilter()"" class=""w-full px-2 py-1 text-xs border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                    }
                    else if (col.FilterType == "select" && !string.IsNullOrEmpty(col.EditorOptions))
                    {
                        filterHtml.Append($@"<select x-model=""filters['{col.Field}']"" @change=""applyFilter()"" class=""w-full px-2 py-1 text-xs border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                        filterHtml.Append(@"<option value=""""></option>");
                        var opts = col.EditorOptions.Split(';');
                        foreach (var opt in opts)
                        {
                            var kv = opt.Split('=');
                            if (kv.Length == 2)
                            {
                                filterHtml.Append($@"<option value=""{kv[0]}"">{kv[1]}</option>");
                            }
                        }
                        filterHtml.Append(@"</select>");
                    }
                    else if (col.FilterType == "lov")
                    {
                         filterHtml.Append($@"
                         <div class=""relative flex items-center"">
                             <input type=""text"" x-model=""filters['{col.Field}']"" @keydown.enter=""applyFilter()"" class=""w-full pl-2 pr-6 py-1 text-xs border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">
                             <button type=""button"" @click=""$dispatch('open-lov', '{col.Field}')"" class=""absolute right-1 text-slate-400 hover:text-blue-600"">
                                 <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/></svg>
                             </button>
                         </div>");
                    }
                    filterHtml.Append("</th>");
                }

                // --- 3. 內容列 (Td) ---
                tdHtml.Append($@"<td class=""px-3 py-2 {tdAlign} text-sm text-slate-700 border-b border-slate-100 whitespace-nowrap"">");

                if (EditMode == "row" && (col.EditorType == "text" || col.EditorType == "select"))
                {
                    // View Mode
                    tdHtml.Append($@"<span x-show=""editingId !== row['{IdField}']"" x-text=""row['{col.Field}'] ?? ''""></span>");

                    // Edit Mode
                    if (col.EditorType == "text")
                    {
                        tdHtml.Append($@"<input x-cloak x-show=""editingId === row['{IdField}']"" type=""text"" x-model=""editRowData['{col.Field}']"" class=""w-full px-2 py-1 text-sm border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                    }
                    else if (col.EditorType == "select" && !string.IsNullOrEmpty(col.EditorOptions))
                    {
                        tdHtml.Append($@"<select x-cloak x-show=""editingId === row['{IdField}']"" x-model=""editRowData['{col.Field}']"" class=""w-full px-2 py-1 text-sm border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                         var opts = col.EditorOptions.Split(';');
                        foreach (var opt in opts)
                        {
                            var kv = opt.Split('=');
                            if (kv.Length == 2)
                            {
                                tdHtml.Append($@"<option value=""{kv[0]}"">{kv[1]}</option>");
                            }
                        }
                        tdHtml.Append(@"</select>");
                    }
                }
                else
                {
                     // Readonly Default
                     tdHtml.Append($@"<span x-text=""row['{col.Field}'] ?? ''""></span>");
                }

                tdHtml.Append("</td>");
            }

            // --- 4. 操作 (Action) 欄位 ---
            if (!string.IsNullOrEmpty(EditMode) && (AllowUpdate || AllowDelete || UpdateCommandVisible || DeleteCommandVisible || ViewCommandVisible))
            {
                thHtml.Append(@"<th class=""px-3 py-2.5 text-center text-xs font-bold text-slate-500 uppercase tracking-wider bg-slate-100 border-b-2 border-slate-200 w-24 shrink-0 sticky right-0"">操作</th>");
                if (actualShowFilter)
                {
                    filterHtml.Append(@"<th class=""px-3 py-2 bg-indigo-50/50 border-b-2 border-slate-200 sticky right-0 text-center"">
                        <button type=""button"" @click=""applyFilter()"" title=""套用過濾""
                                class=""inline-flex items-center justify-center w-7 h-7 rounded-lg border border-blue-200 text-blue-600 bg-blue-50 hover:bg-blue-100 transition-colors"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><use href=""#icon-filter""></use></svg>
                        </button>
                    </th>");
                }

                tdHtml.Append($@"<td class=""px-3 py-2 text-center text-sm border-b border-slate-100 whitespace-nowrap sticky right-0 bg-white group-odd:bg-white group-even:bg-slate-50/60 transition-colors"">
                    <div class=""flex items-center justify-center gap-2"">");

                var btnViewHtml = ViewCommandVisible ? $@"
                                <button type=""button"" @click.stop=""onView(row)"" class=""p-1 text-slate-400 hover:text-blue-500 transition-colors"" title=""檢視"">
                                    <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 12a3 3 0 11-6 0 3 3 0 016 0""/><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z""/></svg>
                                </button>" : "";

                var btnUpdateHtml = (AllowUpdate && UpdateCommandVisible) ? $@"
                                <button type=""button"" @click.stop=""console.log('[GDataGrid] edit button clicked', row); ('{EditMode}'==='row') ? startEdit(row) : editRow(row)"" class=""p-1 text-slate-400 hover:text-amber-500 transition-colors"" title=""編輯"">
                                    <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z""/></svg>
                                </button>" : "";

                var btnDeleteHtml = (AllowDelete && DeleteCommandVisible) ? $@"
                                <button type=""button"" @click.stop=""deleteRow(row)"" class=""p-1 text-slate-400 hover:text-red-500 transition-colors"" title=""刪除"">
                                    <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16""/></svg>
                                </button>" : "";

                if (EditMode == "row")
                {
                    tdHtml.Append($@"
                        <!-- View State Actions -->
                        <template x-if=""editingId !== row['{actualIdField}']"">
                            <div class=""flex gap-1"">{btnViewHtml}{btnUpdateHtml}{btnDeleteHtml}</div>
                        </template>
                        <!-- Edit State Actions -->
                        <template x-if=""editingId === row['{actualIdField}']"">
                            <div class=""flex gap-1"">
                                <button type=""button"" @click.stop=""confirmSave()"" class=""p-1 text-slate-400 hover:text-emerald-500 transition-colors"" title=""儲存"">
                                    <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 13l4 4L19 7""/></svg>
                                </button>
                                <button type=""button"" @click.stop=""cancelEdit()"" class=""p-1 text-slate-400 hover:text-slate-600 transition-colors"" title=""取消"">
                                    <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 12l12 12""/></svg>
                                </button>
                            </div>
                        </template>
                    ");
                }
                else if (EditMode == "form")
                {
                    tdHtml.Append($@"<div class=""flex gap-1"">{btnViewHtml}{btnUpdateHtml}{btnDeleteHtml}</div>");
                }
                tdHtml.Append("</div></td>");
            }

            var theadHtml = new System.Text.StringBuilder();
            theadHtml.Append($@"<tr>{thHtml}</tr>");
            if (ShowFilter)
            {
               theadHtml.Append($@"<tr class=""bg-indigo-50/50"">{filterHtml}</tr>");
            }

            var paginationHtml = "";
            if (Pagination)
            {
                var pageListOpts = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(PageList))
                {
                    foreach(var p in PageList.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var pt = p.Trim();
                        pageListOpts.Append($@"<option value=""{pt}"">{pt} 筆/頁</option>");
                    }
                }

                paginationHtml = $@"
                <!-- Pagination -->
                <div class=""flex flex-wrap items-center justify-between gap-3 px-4 py-2 bg-slate-50/80 border-t border-slate-200 text-sm shrink-0 select-none"">
                    <div class=""flex items-center gap-2"">
                        <select x-model=""pageSize"" @change=""currentPage=1""
                                class=""pl-2 pr-6 py-1 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-blue-400 cursor-pointer"">
                            {pageListOpts}
                        </select>
                    </div>
                    <div class=""flex items-center gap-1"">
                        <button type=""button"" @click=""prevPage()"" :disabled=""currentPage<=1""
                                :class=""currentPage<=1?'opacity-40 cursor-not-allowed':'hover:bg-slate-200'""
                                class=""w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M15 19l-7-7 7-7""/></svg>
                        </button>
                        <div class=""flex items-center gap-1.5 px-2"">
                            <input type=""number"" min=""1"" :max=""totalPages"" :value=""currentPage""
                                   @change=""jumpPage($event.target.value)""
                                   class=""w-14 text-center text-xs border border-slate-300 rounded-lg py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-400"">
                            <span class=""text-slate-400 text-xs"">/</span>
                            <span x-text=""totalPages"" class=""text-xs font-bold text-slate-700""></span>
                            <span class=""text-slate-400 text-xs"">頁</span>
                        </div>
                        <button type=""button"" @click=""nextPage()"" :disabled=""currentPage>=totalPages""
                                :class=""currentPage>=totalPages?'opacity-40 cursor-not-allowed':'hover:bg-slate-200'""
                                class=""w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 5l7 7-7 7""/></svg>
                        </button>
                    </div>
                </div>";
            }

            output.TagName = "div";
            output.Attributes.SetAttribute("id", compId);
            var cleanStyleClass = CleanStyle ? "flex-1 border-0 rounded-none w-full shadow-none" : "";
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden flex flex-col relative {cleanStyleClass} {Class}");
            output.Attributes.SetAttribute("x-data", $"{fnName}()");
            output.Attributes.SetAttribute("x-init", "init()");

            output.Content.SetHtmlContent($@"
                {titleHeaderHtml}
                <!-- Toolbar -->
                <div class=""g-grid-toolbar flex items-center justify-between gap-3 px-4 py-2.5 border-b border-slate-200 bg-slate-50/70 shrink-0 flex-wrap"">
                    <div class=""flex items-center gap-2"">
                        {ToolbarHtml}
                        {((AllowAdd && !string.IsNullOrEmpty(EditMode)) ? $@"
                        <button type=""button"" @click=""addRow()"" class=""flex items-center gap-1 px-2.5 py-1.5 text-xs text-white bg-emerald-500 hover:bg-emerald-600 rounded-lg shadow-sm transition-colors font-medium"">
                            <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 4v16m8-8H4""/></svg>
                            新增
                        </button>
                        " : "")}
                        <button type=""button"" @click=""fetchData()"" title=""重新整理""
                                class=""flex items-center gap-1 px-2.5 py-1.5 text-xs text-slate-600 hover:text-blue-700 hover:bg-blue-50 rounded-lg border border-slate-200 bg-white transition-colors font-medium"">
                            <svg class=""w-3.5 h-3.5"" :class=""loading?'animate-spin':''"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15""/>
                            </svg>
                            重整
                        </button>
                        
                    </div>
                    <span class=""text-xs text-slate-400"">{(string.IsNullOrEmpty(actualTotalCaption) ? "共 " : actualTotalCaption)}<span class=""font-bold text-slate-600"" x-text=""rows.length""></span> 筆</span>
                </div>
                <div x-show=""headerFilter.open""
                     x-transition
                     @click.away=""cancelHeaderFilter()""
                     class=""absolute z-40 bg-white border border-slate-200 rounded-xl shadow-xl w-64""
                     :style=""`left:${{headerFilter.x}}px; top:${{headerFilter.y}}px;`""
                     style=""display:none;"">
                    <div class=""px-3 py-2 border-b border-slate-100 text-sm font-semibold text-slate-700 flex items-center justify-between"">
                        <span x-text=""headerFilter.title || '過濾'""></span>
                        <button type=""button"" @click=""cancelHeaderFilter()"" class=""p-1 text-slate-400 hover:text-slate-600"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/></svg>
                        </button>
                    </div>
                    <template x-if=""headerFilter.mode !== 'datetime'"">
                    <div class=""px-3 pt-2 pb-1.5 border-b border-slate-100"">
                        <div class=""relative mb-2"">
                            <input type=""text""
                                   x-model=""headerFilter.search""
                                   placeholder=""Search""
                                   class=""w-full pl-8 pr-2 py-1.5 text-sm border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">
                            <svg class=""w-4 h-4 text-slate-400 absolute left-2.5 top-1/2 -translate-y-1/2"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/>
                            </svg>
                        </div>
                        <label class=""inline-flex items-center gap-2 text-sm text-slate-700 cursor-pointer"">
                            <input type=""checkbox"" :checked=""isHeaderFilterAllSelected()"" @change=""toggleHeaderFilterAll($event.target.checked)"" class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500"">
                            <span>Select All</span>
                        </label>
                    </div>
                    </template>
                    <template x-if=""headerFilter.mode === 'datetime'"">
                    <div class=""px-3 py-2 border-b border-slate-100 space-y-2"">
                        <div>
                            <label class=""block text-xs text-slate-500 mb-1"">From</label>
                            <input type=""datetime-local"" x-model=""headerFilter.dateFrom""
                                   class=""w-full px-2 py-1.5 text-sm border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">
                        </div>
                        <div>
                            <label class=""block text-xs text-slate-500 mb-1"">To</label>
                            <input type=""datetime-local"" x-model=""headerFilter.dateTo""
                                   class=""w-full px-2 py-1.5 text-sm border border-slate-300 rounded focus:ring-1 focus:ring-blue-500 focus:outline-none"">
                        </div>
                    </div>
                    </template>
                    <div x-show=""headerFilter.mode !== 'datetime'"" class=""max-h-64 overflow-auto px-3 py-2 space-y-1"">
                        <template x-for=""opt in getHeaderFilterVisibleOptions()"" :key=""opt"">
                            <label class=""flex items-center gap-2 text-sm text-slate-700 cursor-pointer"">
                                <input type=""checkbox"" :value=""opt"" x-model=""headerFilter.selected"" class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500"">
                                <span x-text=""opt""></span>
                            </label>
                        </template>
                    </div>
                    <div class=""px-3 py-2 border-t border-slate-100 flex justify-end gap-2"">
                        <button type=""button"" @click=""confirmHeaderFilter()"" class=""px-3 py-1.5 text-xs rounded-md bg-blue-600 text-white hover:bg-blue-700"">OK</button>
                        <button type=""button"" @click=""cancelHeaderFilter()"" class=""px-3 py-1.5 text-xs rounded-md border border-slate-300 text-slate-600 hover:bg-slate-50"">Cancel</button>
                    </div>
                </div>
                <!-- Table -->
                <div class=""overflow-auto flex-1"" style=""min-height:120px;"">
                    <div x-show=""loading"" class=""absolute inset-0 z-20 bg-white/50 backdrop-blur-[1px] flex items-center justify-center text-blue-600 gap-2"">
                        <svg class=""w-8 h-8 animate-spin"" fill=""none"" viewBox=""0 0 24 24"">
                            <circle class=""opacity-25"" cx=""12"" cy=""12"" r=""10"" stroke=""currentColor"" stroke-width=""4""/>
                            <path class=""opacity-75"" fill=""currentColor"" d=""M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z""/>
                        </svg>
                        <span class=""text-sm font-semibold shadow-sm"">載入中...</span>
                    </div>
                    <div x-show=""!loading && rows.length===0"" class=""flex flex-col items-center justify-center py-12 text-slate-400"">
                        <svg class=""w-10 h-10 mb-2 text-slate-300"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""1.5"" d=""M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z""/>
                        </svg>
                        <p class=""text-sm"">無資料</p>
                    </div>
                    <table x-show=""rows.length>0"" class=""min-w-full border-collapse"">
                        <thead class=""sticky top-0 z-10"">{theadHtml}</thead>
                        <tbody>
                            <template x-for=""(row, rowIdx) in pagedRows"" :key=""rowIdx"">
                                <tr class=""group {striped} hover:bg-blue-50/50 transition-colors {rowCursor}""
                                    :class=""selectedRow===row?'bg-blue-100/60 outline outline-1 outline-blue-400':''""
                                    {rowDbl}
                                    @click=""selectedRow=row; {(!string.IsNullOrEmpty(OnRowClick) ? $"({OnRowClick.Replace("\"", "&quot;")})(row);" : "")} {(!string.IsNullOrEmpty(OnSelect) ? $"window['{OnSelect}'] && window['{OnSelect}'](row);" : "")}"">
                                    {tdHtml}
                                </tr>
                            </template>
                        </tbody>
                    </table>
                </div>
                {paginationHtml}
                <!-- Alpine Component Script -->
                <script>
                function {fnName}() {{
                    return {{
                        allRows    : [],
                        rows       : [],
                        loading    : false,
                        sortKey    : '',
                        sortDir    : 'asc',
                        titleSortEnabled: {(TitleSortEnabled ? "true" : "false")},
                        titleSortField: '{defaultTitleSortField}',
                        multiSortEnabled: {(MultiSortEnabled ? "true" : "false")},
                        sortableFields: [{string.Join(",", sortableFields.Select(f => $"'{f}'"))}],
                        sortRules: [],
                        currentPage: 1,
                        pageSize   : {PageSize},
                        selectedRow: null,
                        currentParams: '',
                        selectedIds: [], // 用於 MultiSelect

                        // Editor / Filter states
                        editingId  : null,
                        editRowData: {{}},
                        filters    : {{}},
                        columnFilters: {{}},
                        dateTimeFilters: {{}},
                        headerFilter: {{
                            open: false,
                            field: '',
                            title: '',
                            mode: 'list',
                            options: [],
                            selected: [],
                            search: '',
                            dateFrom: '',
                            dateTo: '',
                            x: 0,
                            y: 0
                        }},

                        get sortedRows() {{
                            const compare = (av, bv, dir) => {{
                                if (av === null || av === undefined) av = '';
                                if (bv === null || bv === undefined) bv = '';
                                if (av === bv) return 0;
                                return av < bv ? -dir : dir;
                            }};
                            if (this.multiSortEnabled && this.sortRules.length > 0) {{
                                return [...this.rows].sort((a, b) => {{
                                    for (const rule of this.sortRules) {{
                                        const dir = rule.dir === 'asc' ? 1 : -1;
                                        const r = compare(a?.[rule.key], b?.[rule.key], dir);
                                        if (r !== 0) return r;
                                    }}
                                    return 0;
                                }});
                            }}
                            if (!this.sortKey) return this.rows;
                            const dir = this.sortDir === 'asc' ? 1 : -1;
                            return [...this.rows].sort((a, b) => compare(a?.[this.sortKey], b?.[this.sortKey], dir));
                        }},
                        get totalPages()  {{ return Math.max(1, Math.ceil(this.sortedRows.length / this.pageSize)); }},
                        get pagedRows()   {{
                            const s = (this.currentPage - 1) * this.pageSize;
                            return this.sortedRows.slice(s, s + Number(this.pageSize));
                        }},

                        async init()     {{
                            this.$el.gDataGrid = this; // Expose API to DOM element
                            if (this.titleSortEnabled && this.titleSortField && !this.sortKey) {{
                                this.setSort(this.titleSortField, 'asc');
                            }}
                            if ({(!actualNotInitGrid).ToString().ToLower()}) {{
                                await this.fetchData();
                            }}
                        }},
                        onSelectRow(row, isChecked) {{
                            {(!string.IsNullOrEmpty(OnSelect) ? $"if (isChecked) window['{OnSelect}'] && window['{OnSelect}'](row);" : "")}
                        }},
                        toggleAll(isChecked) {{
                            if (isChecked) {{
                                this.selectedIds = this.rows.map(r => r['{actualIdField}']);
                            }} else {{
                                this.selectedIds = [];
                            }}
                        }},
                        async fetchData(queryParams = null) {{
                            if (queryParams !== null) {{
                                this.currentParams = typeof queryParams === 'object'
                                    ? new URLSearchParams(queryParams).toString()
                                    : queryParams;
                            }}
                            this.loading = true;
                            // Reset selection/edit state on fetch
                            this.cancelEdit();
                            try {{
                                const urlObj = new URL('{actualApiUrl}', window.location.origin);
                                if (this.currentParams) {{
                                    const params = new URLSearchParams(this.currentParams);
                                    params.forEach((v, k) => urlObj.searchParams.append(k, v));
                                }}
                                const res  = await fetch(urlObj.toString());
                                const json = await res.json();
                                if (res.status === 401) {{ window.location.href = '/Account/Login'; return; }}
                                this.allRows = json.data ?? json;
                                this.rows = [...this.allRows];
                                this.applyColumnFilters(false);
                                this.currentPage = 1;
                                this.selectedRow = null;
                                {(!string.IsNullOrEmpty(OnLoadSuccess) ? $"window['{OnLoadSuccess}'] && window['{OnLoadSuccess}'](this.rows);" : "")}
                            }} catch(e) {{ console.error('GDataGrid fetch error:', e); }}
                            finally    {{ this.loading = false; }}
                        }},

                        // Filter actions
                        applyFilter() {{
                            // clean up empty string properties from filters object
                            const cleanFilters = Object.fromEntries(
                                Object.entries(this.filters).filter(([_, v]) => v !== '' && v !== null && v !== undefined)
                            );
                            this.$dispatch('query', {{ filters: cleanFilters }});
                            {(!string.IsNullOrEmpty(OnFilter) ? $"window['{OnFilter}'] && window['{OnFilter}'](cleanFilters);" : "")}
                            // If user doesn't catch the @query event to do a custom fetch,
                            // we do a default fetch with URLSearchParams.
                            const params = new URLSearchParams(cleanFilters).toString();
                            this.fetchData(params);
                        }},
                        openHeaderFilter(field, event) {{
                            if (!{(actualTitleFilterEnabled ? "true" : "false")}) return;
                            const titleNode = event?.currentTarget?.closest('th')?.querySelector('span');
                            const values = [...new Set((this.allRows || [])
                                .map(r => r?.[field])
                                .filter(v => v !== null && v !== undefined && `${{v}}`.trim() !== '')
                                .map(v => `${{v}}`))];
                            values.sort((a, b) => a.localeCompare(b, 'zh-Hant'));

                            const existing = this.columnFilters[field];
                            this.headerFilter.field = field;
                            this.headerFilter.title = titleNode?.textContent?.trim() || field;
                            this.headerFilter.options = values;
                            this.headerFilter.selected = existing ? [...existing] : [...values];
                            this.headerFilter.search = '';
                            this.headerFilter.mode = this.isDateTimeField(values) ? 'datetime' : 'list';
                            const existingDate = this.dateTimeFilters[field] || {{}};
                            this.headerFilter.dateFrom = this.toDateTimeLocal(existingDate.from);
                            this.headerFilter.dateTo = this.toDateTimeLocal(existingDate.to);

                            const rect = event?.currentTarget?.getBoundingClientRect();
                            const hostRect = this.$el.getBoundingClientRect();
                            const toolbarRect = this.$el.querySelector('.g-grid-toolbar')?.getBoundingClientRect();
                            this.headerFilter.x = Math.max(8, (rect?.left || hostRect.left) - hostRect.left);
                            this.headerFilter.y = Math.max(8, ((toolbarRect?.bottom || rect?.bottom || hostRect.top) - hostRect.top) + 6);
                            this.headerFilter.open = true;
                        }},
                        isDateTimeField(values) {{
                            if (!values || !values.length) return false;
                            const sample = values.slice(0, Math.min(values.length, 6));
                            const okCount = sample.filter(v => !Number.isNaN(Date.parse(v))).length;
                            const hasTime = sample.some(v => /[T\s]\d{{2}}:\d{{2}}/.test(v));
                            return okCount >= Math.ceil(sample.length * 0.8) && hasTime;
                        }},
                        toDateTimeLocal(v) {{
                            if (!v) return '';
                            const d = new Date(v);
                            if (Number.isNaN(d.getTime())) return '';
                            const pad = (n) => String(n).padStart(2, '0');
                            return `${{d.getFullYear()}}-${{pad(d.getMonth()+1)}}-${{pad(d.getDate())}}T${{pad(d.getHours())}}:${{pad(d.getMinutes())}}`;
                        }},
                        isHeaderFilterAllSelected() {{
                            const opts = this.getHeaderFilterVisibleOptions();
                            if (!opts.length) return false;
                            return opts.every(o => this.headerFilter.selected.includes(o));
                        }},
                        toggleHeaderFilterAll(checked) {{
                            const opts = this.getHeaderFilterVisibleOptions();
                            if (checked) {{
                                const merged = new Set([...(this.headerFilter.selected || []), ...opts]);
                                this.headerFilter.selected = [...merged];
                            }} else {{
                                this.headerFilter.selected = (this.headerFilter.selected || []).filter(v => !opts.includes(v));
                            }}
                        }},
                        getHeaderFilterVisibleOptions() {{
                            const q = (this.headerFilter.search || '').trim().toLowerCase();
                            if (!q) return this.headerFilter.options;
                            return this.headerFilter.options.filter(v => `${{v}}`.toLowerCase().includes(q));
                        }},
                        confirmHeaderFilter() {{
                            const field = this.headerFilter.field;
                            if (!field) return;

                            if (this.headerFilter.mode === 'datetime') {{
                                const from = this.headerFilter.dateFrom || '';
                                const to = this.headerFilter.dateTo || '';
                                if (!from && !to) {{
                                    delete this.dateTimeFilters[field];
                                }} else {{
                                    this.dateTimeFilters[field] = {{
                                        from: from ? new Date(from).toISOString() : '',
                                        to: to ? new Date(to).toISOString() : ''
                                    }};
                                }}
                            }} else {{
                                const selected = [...this.headerFilter.selected];
                                if (!selected.length || selected.length === this.headerFilter.options.length) {{
                                    delete this.columnFilters[field];
                                }} else {{
                                    this.columnFilters[field] = selected;
                                }}
                                delete this.dateTimeFilters[field];
                            }}

                            this.applyColumnFilters(true);
                            this.headerFilter.open = false;
                        }},
                        cancelHeaderFilter() {{
                            this.headerFilter.open = false;
                        }},
                        applyColumnFilters(emitEvent) {{
                            const fields = Object.keys(this.columnFilters);
                            const dateFields = Object.keys(this.dateTimeFilters);
                            if (!fields.length && !dateFields.length) {{
                                this.rows = [...this.allRows];
                            }} else {{
                                this.rows = (this.allRows || []).filter(row => {{
                                    const passList = fields.every(f => this.columnFilters[f].includes(`${{row?.[f] ?? ''}}`));
                                    if (!passList) return false;

                                    const passDate = dateFields.every(f => {{
                                        const rule = this.dateTimeFilters[f] || {{}};
                                        const raw = row?.[f];
                                        if (!raw) return false;
                                        const val = new Date(raw).getTime();
                                        if (Number.isNaN(val)) return false;
                                        const from = rule.from ? new Date(rule.from).getTime() : null;
                                        const to = rule.to ? new Date(rule.to).getTime() : null;
                                        if (from !== null && val < from) return false;
                                        if (to !== null && val > to) return false;
                                        return true;
                                    }});
                                    return passDate;
                                }});
                            }}
                            this.currentPage = 1;
                            this.selectedRow = null;
                            if (emitEvent) {{
                                const summaryList = Object.fromEntries(
                                    Object.entries(this.columnFilters).map(([k, v]) => [k, v.join(',')])
                                );
                                const summaryDate = Object.fromEntries(
                                    Object.entries(this.dateTimeFilters).flatMap(([k, v]) => [
                                        [`${{k}}_from`, v.from || ''],
                                        [`${{k}}_to`, v.to || '']
                                    ])
                                );
                                const summary = {{ ...summaryList, ...summaryDate }};
                                this.$dispatch('query', {{ filters: summary }});
                                {(!string.IsNullOrEmpty(OnFilter) ? $"window['{OnFilter}'] && window['{OnFilter}'](summary);" : "")}
                            }}
                        }},

                        // Edit Actions
                        addRow() {{
                            {(!string.IsNullOrEmpty(OnInsert) ? $"window['{OnInsert}'] && window['{OnInsert}']();" : "")}
                            this.$dispatch('add');
                        }},
                        editRow(row) {{
                            console.log('[GDataGrid] editRow()', row);
                            {(!string.IsNullOrEmpty(OnUpdate) ? $"window['{OnUpdate}'] && window['{OnUpdate}'](row);" : "")}
                            this.$dispatch('edit', {{row: row}});
                        }},
                        onView(row) {{
                            console.log('[GDataGrid] onView()', row);
                            {(!string.IsNullOrEmpty(OnView) ? $"window['{OnView}'] && window['{OnView}'](row);" : "")}
                            this.$dispatch('view', {{row: row}});
                        }},
                        async deleteRow(row) {{
                            {(!string.IsNullOrEmpty(OnDeleting) ? $"if (window['{OnDeleting}']) {{ if(window['{OnDeleting}'](row) === false) return; }}" : "")}

                            // MVC CRUD Auto Apply Delete
                            if ('{AutoApply}'.toLowerCase() === 'true' && '{RemoteName}') {{
                                const tableParam = '{DataMember}' ? '?DataMember={DataMember}' : '';
                                const url = `/{RemoteName}/delete${{tableParam}}`.replace('//', '/');
                                try {{
                                    const res = await fetch(url, {{
                                        method: 'POST',
                                        headers: {{ 'Content-Type': 'application/json' }},
                                        body: JSON.stringify(row)
                                    }});
                                    if(res.ok) this.fetchData();
                                }} catch(e) {{ console.error('Delete auto-apply failed:', e); }}
                            }}

                            {(!string.IsNullOrEmpty(OnDelete) ? $"if (window['{OnDelete}']) window['{OnDelete}'](row);" : "")}
                            this.$dispatch('delete', {{row: row}});
                        }},

                        startEdit(row) {{
                            console.log('[GDataGrid] startEdit()', row);
                            this.editingId = row['{actualIdField}'];
                            this.editRowData = JSON.parse(JSON.stringify(row));
                        }},
                        cancelEdit() {{
                            this.editingId = null;
                            this.editRowData = {{}};
                        }},
                        async confirmSave() {{
                            if(!this.editingId) return;

                            // MVC CRUD Auto Apply Update/Insert (假設 editingId 代表 row id，若無則為 insert)
                            // 這裡由於未實作 native row add，主要是 update
                            if ('{AutoApply}'.toLowerCase() === 'true' && '{RemoteName}') {{
                                const action = 'update'; // 如果有 insert 情境可判斷 editingId !== 'new' 來決定
                                const tableParam = '{DataMember}' ? '?DataMember={DataMember}' : '';
                                const url = `/{RemoteName}/${{action}}${{tableParam}}`.replace('//', '/');
                                try {{
                                    const res = await fetch(url, {{
                                        method: 'POST',
                                        headers: {{ 'Content-Type': 'application/json' }},
                                        body: JSON.stringify(this.editRowData)
                                    }});
                                    if(res.ok) this.fetchData();
                                }} catch(e) {{ console.error('Update auto-apply failed:', e); }}
                            }}

                            this.$dispatch('save', {{
                                row: this.editRowData,
                                callback: (success) => {{
                                    if(success) this.cancelEdit();
                                }}
                            }});
                        }},

                        toggleSort(key) {{
                            if (!this.isSortableField(key)) return;
                            if (this.multiSortEnabled) {{
                                const existing = this.getSortRule(key);
                                const nextDir = existing?.dir === 'asc' ? 'desc' : 'asc';
                                this.setSort(key, nextDir);
                                return;
                            }}
                            if (this.sortKey === key) this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
                            else {{ this.sortKey = key; this.sortDir = 'asc'; }}
                            if (this.titleSortEnabled) this.titleSortField = key;
                        }},
                        isSortableField(key) {{
                            return !this.sortableFields?.length || this.sortableFields.includes(key);
                        }},
                        getSortRule(key) {{
                            return (this.sortRules || []).find(r => r.key === key);
                        }},
                        getSortOrder(key) {{
                            const idx = (this.sortRules || []).findIndex(r => r.key === key);
                            return idx < 0 ? 0 : idx + 1;
                        }},
                        isSorted(key, dir) {{
                            if (this.multiSortEnabled) {{
                                const r = this.getSortRule(key);
                                return !!r && r.dir === dir;
                            }}
                            return this.sortKey === key && this.sortDir === dir;
                        }},
                        setSort(key, dir) {{
                            if (!this.isSortableField(key)) return;
                            if (this.multiSortEnabled) {{
                                const rules = [...(this.sortRules || [])];
                                const idx = rules.findIndex(r => r.key === key);
                                if (idx >= 0) {{
                                    rules[idx] = {{ key, dir: (dir === 'desc') ? 'desc' : 'asc' }};
                                }} else {{
                                    rules.push({{ key, dir: (dir === 'desc') ? 'desc' : 'asc' }});
                                }}
                                this.sortRules = rules;
                            }}
                            this.sortKey = key;
                            this.sortDir = (dir === 'desc') ? 'desc' : 'asc';
                            if (this.titleSortEnabled) this.titleSortField = key;
                        }},
                        setTitleSort(dir) {{
                            if (!this.titleSortEnabled) return;
                            const key = this.titleSortField || this.sortKey;
                            if (!key) return;
                            this.setSort(key, dir);
                        }},
                        prevPage() {{ if (this.currentPage > 1) this.currentPage--; }},
                        nextPage() {{ if (this.currentPage < this.totalPages) this.currentPage++; }},
                        jumpPage(v) {{
                            const p = parseInt(v);
                            if (!isNaN(p)) this.currentPage = Math.min(Math.max(1, p), this.totalPages);
                        }},
                        getSelectedRow() {{ return this.selectedRow; }},
                        refresh() {{ this.fetchData(); }}
                    }};
                }}
                </script>
            ");
        }

        private static HashSet<string> ParseSortableFields(
            string sortableColumns,
            List<(string Field, string Title, string Width, string Align, string EditorType, string EditorOptions, string FilterType)> cols)
        {
            if (string.IsNullOrWhiteSpace(sortableColumns))
            {
                return cols
                    .Select(c => c.Field)
                    .Where(f => !string.IsNullOrWhiteSpace(f))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            return sortableColumns
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static List<(string Field, string Title, string Width, string Align, string EditorType, string EditorOptions, string FilterType)> ParseColumns(string cols)
        {
            if (string.IsNullOrWhiteSpace(cols)) return new();
            return cols.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c =>
                {
                    var parts = c.Trim().Split(':');
                    return (
                        Field : parts.ElementAtOrDefault(0)?.Trim() ?? "",
                        Title : parts.ElementAtOrDefault(1)?.Trim() ?? parts[0].Trim(),
                        Width : parts.ElementAtOrDefault(2)?.Trim() ?? "",
                        Align : parts.ElementAtOrDefault(3)?.Trim() ?? "left",
                        EditorType : parts.ElementAtOrDefault(4)?.Trim() ?? "readonly",
                        EditorOptions : parts.ElementAtOrDefault(5)?.Trim() ?? "",
                        FilterType : parts.ElementAtOrDefault(6)?.Trim() ?? ""
                    );
                })
                .Where(c => !string.IsNullOrEmpty(c.Field))
                .ToList();
        }
    }
}
