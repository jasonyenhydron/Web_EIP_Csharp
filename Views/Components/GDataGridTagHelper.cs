using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

/*
 * GDataGridTagHelper
 * Alpine.js based data grid with:
 * - sorting (single / multi)
 * - header filtering
 * - pagination
 * - optional CRUD action buttons
 *
 * Example:
 *   <g-datagrid id="programGrid"
 *               api="/api/mis/programs/IDMGD01/list"
 *               columns="PROGRAM_NO:程式編號:120:left,DISPLAY_CODE:顯示:60:center,PURPOSE:用途"
 *               page-size="20"
 *               striped="true"
 *               on-row-click="onRowSelected(row)"/>
 *
 * columns format:
 *   field:title:width:align(editor/filter options...)
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-datagrid")]
    public class GDataGridTagHelper : TagHelper
    {
        private sealed class QueryColumnDefinition
        {
            public string FieldName { get; set; } = "";
            public string Caption { get; set; } = "";
            public string Condition { get; set; } = "=";
            public string AndOr { get; set; } = "AND";
            public string DataType { get; set; } = "string";
            public string DefaultMethod { get; set; } = "";
            public string DefaultValue { get; set; } = "";
            public string Editor { get; set; } = "text";
            public string EditorOptions { get; set; } = "";
            public string Format { get; set; } = "";
            public bool IsNvarChar { get; set; } = false;
            public bool NewLine { get; set; } = false;
            public bool RemoteMethod { get; set; } = false;
            public int RowSpan { get; set; } = 1;
            public int Span { get; set; } = 3;
            public string TableName { get; set; } = "";
            public int Width { get; set; } = 0;
            public bool Api { get; set; } = false;
        }

        public string Id           { get; set; } = "";
        [HtmlAttributeName("api")]
        public string Api          { get; set; } = "";
        public string ApiUrl       { get; set; } = "";
        public string RemoteName   { get; set; } = ""; // ?謘餉爸 controller name
        [HtmlAttributeName("member-id")]
        public string DataMember   { get; set; } = ""; // ?謘餉爸 action name

        public string Columns      { get; set; } = "";
        public int    PageSize     { get; set; } = 20;
        public bool   Striped      { get; set; } = true;

        public bool   ShowRowNum   { get; set; } = false;
        public bool   RowNumbers   { get; set; } = false;

        public string OnRowClick   { get; set; } = "";
        public string OnRowDblClick{ get; set; } = "";
        public string ToolbarHtml  { get; set; } = "";
        public string Class        { get; set; } = "";
        public string ExtraClass   { get; set; } = "";
        [HtmlAttributeName("clean-style")]
        public bool CleanStyle     { get; set; } = false;
        public string EditMode     { get; set; } = "";

        public string IdField      { get; set; } = "ROWID";
        public string IDField      { get; set; } = "";

        public bool   ShowFilter   { get; set; } = false;
        public bool   QueryAutoColumn { get; set; } = false;
        public bool   TitleFilterEnabled { get; set; } = true;
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
        public string QueryTitle   { get; set; } = "?鈭亙眺";
        public int    QueryLeft    { get; set; } = 0;
        public int    QueryTop     { get; set; } = 0;
        public string QueryColumns { get; set; } = "";
        public bool   MultiSelect  { get; set; } = false;
        public string EditDialogID { get; set; } = "";
        public bool   TitleSortEnabled { get; set; } = false;
        public string TitleSortField { get; set; } = "";
        public string SortableColumns { get; set; } = "";
        public bool   MultiSortEnabled { get; set; } = false;
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

        // --- JS ?哨?颲?Callback (Alpine.js integration) ---
        public string OnLoadSuccess{ get; set; } = ""; // callback after load success
        public string OnSelect     { get; set; } = ""; // ?綜等???
        public string OnInsert     { get; set; } = ""; // ?????
        public string OnInserted   { get; set; } = ""; // ????謅?
        public string OnUpdate     { get; set; } = ""; // callback before update
        public string OnUpdated    { get; set; } = ""; // callback after update
        public string OnDelete     { get; set; } = ""; // ??畸???
        public string OnDeleted    { get; set; } = ""; // ??畸??謅?
        public string OnDeleting   { get; set; } = ""; // ??畸??謅?
        public string OnFilter     { get; set; } = ""; // ?剜?蹓?
        public string OnView       { get; set; } = ""; // ?潘撩?

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId   = string.IsNullOrEmpty(Id) ? $"grid_{Guid.NewGuid():N}" : Id;
            var fnName   = $"gDataGrid_{compId}";
            var zebraClassExpr = Striped ? "(rowIdx % 2 === 0 ? 'bg-white' : 'bg-slate-50')" : "''";
            var rowClick = !string.IsNullOrEmpty(OnRowClick)
                ? $"@click.stop=\"({OnRowClick.Replace("\"","&quot;")})(row)\""
                : "";
            var rowDbl   = !string.IsNullOrEmpty(OnRowDblClick)
                ? $"@dblclick.stop=\"({OnRowDblClick.Replace("\"","&quot;")})(row)\""
                : "";
            var rowCursor = (!string.IsNullOrEmpty(OnRowClick) || !string.IsNullOrEmpty(OnRowDblClick))
                ? "cursor-pointer" : "";

            // ??? columns
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
            var queryColumns = ParseQueryColumns(QueryColumns);
            var queryColumnsJson = JsonSerializer.Serialize(queryColumns, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var queryPanelHtml = BuildQueryPanelHtml(queryColumns);

            // --- Option: Title ---
            var titleHeaderHtml = "";
            if (!string.IsNullOrEmpty(Title))
            {
                var helpIcon = !string.IsNullOrEmpty(HelpLink) ? $"<a href='{HelpLink}' target='_blank' class='text-slate-400 hover:text-blue-500 transition-colors' title='Help'><svg class='w-5 h-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg></a>" : "";
                var titleSortButtons = TitleSortEnabled
                    ? @"<div class='inline-flex items-center rounded-lg border border-slate-200 bg-white overflow-hidden'>
                            <button type='button' @click='setTitleSort(""asc"")' :class='(sortKey===titleSortField && sortDir===""asc"") ? ""text-blue-600 bg-slate-100"" : ""text-slate-500 bg-slate-100""' class='w-8 h-8 inline-flex items-center justify-center transition-colors' title='升冪'>
                                <svg class='w-4 h-4' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M5 15l7-7 7 7'/></svg>
                            </button>
                            <button type='button' @click='setTitleSort(""desc"")' :class='(sortKey===titleSortField && sortDir===""desc"") ? ""text-blue-600 bg-slate-100"" : ""text-slate-500 bg-slate-100""' class='w-8 h-8 inline-flex items-center justify-center border-l border-slate-200 transition-colors' title='降冪'>
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
                    filterHtml.Append(@"<th class=""px-3 py-2 bg-slate-100 border-b-2 border-slate-200""></th>");
                }
                tdHtml.Append(@"<td class=""px-3 py-2 text-center text-xs text-slate-400 border-b border-slate-100"" x-text=""(currentPage-1)*pageSize+rowIdx+1""></td>");
            }

            if (MultiSelect)
            {
                thHtml.Append(@"<th class=""px-3 py-2.5 text-center bg-slate-100 border-b-2 border-slate-200 w-10 shrink-0""><input type=""checkbox"" @change=""toggleAll($event.target.checked)"" class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500 cursor-pointer""></th>");
                if (actualShowFilter)
                {
                    filterHtml.Append(@"<th class=""px-3 py-2 bg-slate-100 border-b-2 border-slate-200""></th>");
                }
                tdHtml.Append($@"<td class=""px-3 py-2 text-center border-b border-slate-100"" @click.stop><input type=""checkbox"" :value=""row['{actualIdField}']"" x-model=""selectedIds"" @change=""onSelectRow(row, $event.target.checked)"" class=""w-4 h-4 text-blue-600 border-slate-300 rounded focus:ring-blue-500 cursor-pointer""></td>");
            }

            foreach (var col in cols)
            {
                var wStyle  = string.IsNullOrEmpty(col.Width) ? "" : $"width:{col.Width}px;min-width:{col.Width}px;";
                var thAlign = col.Align is "center" or "right" ? $"text-{col.Align}" : "text-left";
                var tdAlign = col.Align is "center" or "right" ? $"text-{col.Align}" : "text-left";
                var isSortable = sortableFieldSet.Contains(col.Field);
                var sortableHeaderClass = isSortable ? "cursor-pointer bg-slate-100" : "cursor-default";
                var filterIconBtn = actualTitleFilterEnabled
                    ? $@"<button type=""button"" @click.stop=""openHeaderFilter('{col.Field}', $event)"" title=""欄位篩選""
                               class=""ml-1 inline-flex items-center justify-center w-5 h-5 rounded bg-slate-100 text-blue-600 transition-colors"">
                            <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><use href=""#icon-filter""></use></svg>
                       </button>"
                    : "";

                var sortIconBtns = $@"
                            <span x-show=""isSortableField('{col.Field}')"" class=""inline-flex items-center rounded border border-slate-200 bg-white overflow-hidden"">
                                <button type=""button"" @click.stop=""setSort('{col.Field}', 'asc')"" title=""升冪""
                                        :class=""isSorted('{col.Field}', 'asc') ? 'text-blue-600 bg-slate-100' : 'text-slate-400 bg-slate-100'""
                                        class=""w-4 h-4 inline-flex items-center justify-center transition-colors"">
                                    <svg class=""w-3 h-3"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                        <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 15l7-7 7 7""/>
                                    </svg>
                                </button>
                                <button type=""button"" @click.stop=""setSort('{col.Field}', 'desc')"" title=""降冪""
                                        :class=""isSorted('{col.Field}', 'desc') ? 'text-blue-600 bg-slate-100' : 'text-slate-400 bg-slate-100'""
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
                                  x-text=""`#${{getSortOrder('{col.Field}')}}`""
                                  class=""text-[10px] text-slate-600 font-semibold""></span>
                            {sortIconBtns}
                            {filterIconBtn}
                        </span>
                    </th>");
                if (ShowFilter)
                {
                    filterHtml.Append($@"<th class=""px-2 py-1.5 bg-slate-100 border-b border-slate-200"">");
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

                // --- 3. ??寞???(Td) ---
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
            if (!string.IsNullOrEmpty(EditMode) && (AllowUpdate || AllowDelete || UpdateCommandVisible || DeleteCommandVisible || ViewCommandVisible))
            {
                thHtml.Append(@"<th class=""px-3 py-2.5 text-center text-xs font-bold text-slate-500 uppercase tracking-wider bg-slate-100 border-b-2 border-slate-200 w-24 shrink-0 sticky right-0"">操作</th>");
                if (actualShowFilter)
                {
                    filterHtml.Append(@"<th class=""px-3 py-2 bg-slate-100 border-b-2 border-slate-200 sticky right-0 text-center"">
                        <button type=""button"" @click=""applyFilter()"" title=""套用欄位篩選""
                                class=""inline-flex items-center justify-center w-7 h-7 rounded-lg border border-blue-200 text-blue-600 bg-slate-100 transition-colors"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><use href=""#icon-filter""></use></svg>
                        </button>
                    </th>");
                }

                tdHtml.Append($@"<td class=""px-3 py-2 text-center text-sm border-b border-slate-100 whitespace-nowrap sticky right-0 transition-colors"">
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
               theadHtml.Append($@"<tr class=""bg-slate-100"">{filterHtml}</tr>");
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
                <div class=""flex flex-wrap items-center justify-between gap-3 px-4 py-2 bg-slate-100 border-t border-slate-200 text-sm shrink-0 select-none"">
                    <div class=""flex items-center gap-2"">
                        <select x-model=""pageSize"" @change=""currentPage=1""
                                class=""pl-2 pr-6 py-1 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-blue-400 cursor-pointer"">
                            {pageListOpts}
                        </select>
                    </div>
                    <div class=""flex items-center gap-1"">
                        <span class=""text-xs text-slate-500 whitespace-nowrap pr-1"">{(string.IsNullOrEmpty(actualTotalCaption) ? "共 " : actualTotalCaption)}<span class=""font-bold text-slate-700"" x-text=""rows.length""></span> 筆</span>
                        <button type=""button"" @click=""prevPage()"" :disabled=""currentPage<=1""
                                :class=""currentPage<=1?'opacity-40 cursor-not-allowed':'bg-slate-100'""
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
                                :class=""currentPage>=totalPages?'opacity-40 cursor-not-allowed':'bg-slate-100'""
                                class=""w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors"">
                            <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 5l7 7-7 7""/></svg>
                        </button>
                    </div>
                </div>";
            }

            output.TagName = "div";
            output.Attributes.SetAttribute("id", compId);
            var cleanStyleClass = CleanStyle ? "flex-1 border-0 rounded-none w-full shadow-none" : "";
            var defaultClass = $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden flex flex-col relative min-h-0 w-full {cleanStyleClass}".Trim();
            var finalClass = TagHelperClassResolver.Resolve(defaultClass, Class, ExtraClass);
            output.Attributes.SetAttribute("class", finalClass);
            output.Attributes.SetAttribute("x-data", $"{fnName}()");
            output.Attributes.SetAttribute("x-init", "init()");

	            output.Content.SetHtmlContent($@"
                    {queryPanelHtml}
	                <!-- Toolbar -->
                <div class=""g-grid-toolbar flex items-center justify-between gap-3 px-4 py-2.5 border-b border-slate-200 bg-slate-100 shrink-0 flex-wrap"">
                    <div class=""flex items-center gap-2"">
                        {ToolbarHtml}
                        {((AllowAdd && !string.IsNullOrEmpty(EditMode)) ? $@"
                        <button type=""button"" @click=""addRow()"" class=""flex items-center gap-1 px-2.5 py-1.5 text-xs text-white bg-slate-100 rounded-lg shadow-sm transition-colors font-medium"">
                            <svg class=""w-3.5 h-3.5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 4v16m8-8H4""/></svg>
                            新增
                        </button>
                        " : "")}
                        <button type=""button"" @click=""fetchData()"" title=""重新載入""
                                class=""flex items-center gap-1 px-2.5 py-1.5 text-xs text-slate-600 hover:text-blue-700 bg-slate-100 rounded-lg border border-slate-200 bg-white transition-colors font-medium"">
                            <svg class=""w-3.5 h-3.5"" :class=""loading?'animate-spin':''"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15""/>
                            </svg>
                            重整
                        </button>
                        
                    </div>
	                    {(!Pagination ? $@"<span class=""text-xs text-slate-400"">{(string.IsNullOrEmpty(actualTotalCaption) ? "共 " : actualTotalCaption)}<span class=""font-bold text-slate-600"" x-text=""rows.length""></span> 筆</span>" : "")}
	                </div>
                    {titleHeaderHtml}
	                <div x-show=""headerFilter.open""
	                     x-transition
	                     @click.away=""cancelHeaderFilter()""
                     class=""absolute z-40 bg-white border border-slate-200 rounded-xl shadow-xl w-64""
                     :style=""`left:${{headerFilter.x}}px; top:${{headerFilter.y}}px;`""
                     style=""display:none;"">
                    <div class=""px-3 py-2 border-b border-slate-100 text-sm font-semibold text-slate-700 flex items-center justify-between"">
                        <span x-text=""headerFilter.title || '篩選'""></span>
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
                        <button type=""button"" @click=""confirmHeaderFilter()"" class=""px-3 py-1.5 text-xs rounded-md bg-slate-100 text-white bg-slate-100"">OK</button>
                        <button type=""button"" @click=""cancelHeaderFilter()"" class=""px-3 py-1.5 text-xs rounded-md border border-slate-300 text-slate-600 bg-slate-100"">Cancel</button>
                    </div>
                </div>
                <!-- Table -->
                <div class=""g-grid-body overflow-x-auto overflow-y-auto flex-1 min-h-0"" style=""min-height:120px; max-height: var(--g-grid-body-max-height, 52vh); scrollbar-gutter: stable both-edges;"">
                    <div x-show=""loading"" class=""absolute inset-0 z-20 bg-white backdrop-blur-[1px] flex items-center justify-center text-blue-600 gap-2"">
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
                        <p class=""text-sm"">目前無資料</p>
                    </div>
                    <table x-show=""rows.length>0"" class=""min-w-full border-collapse"">
                        <thead class=""sticky top-0 z-10"">{theadHtml}</thead>
                        <tbody>
                            <template x-for=""(row, rowIdx) in pagedRows"" :key=""rowIdx"">
                                <tr class=""group transition-colors {rowCursor}""
                                    :class=""[{zebraClassExpr}, 'hover:bg-slate-100', selectedRow===row?'bg-slate-100 outline outline-1 outline-blue-400':'']""
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
                        selectedIds: [], // ??踐? MultiSelect
                        queryColumns: {queryColumnsJson},
                        queryValues: {{}},
                        queryPanelOpen: true,

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
                                this.initQueryColumns();
	                            if (this.titleSortEnabled && this.titleSortField && !this.sortKey) {{
	                                this.setSort(this.titleSortField, 'asc');
	                            }}
                            if ({(!actualNotInitGrid).ToString().ToLower()}) {{
                                await this.fetchData();
	                            }}
	                        }},
                            initQueryColumns() {{
                                if (!Array.isArray(this.queryColumns) || this.queryColumns.length === 0) return;
                                for (const col of this.queryColumns) {{
                                    const key = col?.fieldName;
                                    if (!key) continue;
                                    const dv = col?.defaultValue ?? '';
                                    this.queryValues[key] = dv;
                                }}
                            }},
                            invokeQueryDefaultMethod(fieldName) {{
                                const col = (this.queryColumns || []).find(x => x.fieldName === fieldName);
                                if (!col || !col.defaultMethod) return;
                                if (col.remoteMethod || col.api) return;
                                const fnText = String(col.defaultMethod).trim();
                                if (!fnText) return;
                                try {{
                                    if (fnText.includes('(')) {{
                                        // Supports inline call expression.
                                        (new Function(fnText))();
                                    }} else if (typeof window[fnText] === 'function') {{
                                        const ret = window[fnText](fieldName, this.queryValues[fieldName], this.queryValues);
                                        if (ret !== undefined && ret !== null) this.queryValues[fieldName] = ret;
                                    }}
                                }} catch (e) {{
                                    console.error('[GDataGrid] invokeQueryDefaultMethod error:', e);
                                }}
                            }},
                            buildQueryParams() {{
                                const params = {{}};
                                for (const col of (this.queryColumns || [])) {{
                                    const key = col?.fieldName;
                                    if (!key) continue;
                                    const raw = this.queryValues[key];
                                    if (raw === null || raw === undefined || `${{raw}}`.trim() === '') continue;
                                    params[key] = raw;
                                    params[`${{key}}_condition`] = col.condition || '=';
                                    params[`${{key}}_andOr`] = col.andOr || 'AND';
                                    params[`${{key}}_dataType`] = col.dataType || 'string';
                                }}
                                return params;
                            }},
                            executeQuery() {{
                                const queryParams = this.buildQueryParams();
                                this.$dispatch('query', {{ filters: queryParams }});
                                {(!string.IsNullOrEmpty(OnFilter) ? $"window['{OnFilter}'] && window['{OnFilter}'](queryParams);" : "")}
                                this.fetchData(queryParams);
                            }},
                            toggleQueryPanel() {{
                                this.queryPanelOpen = !this.queryPanelOpen;
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
                                let json = null;
                                try {{
                                    json = await res.json();
                                }} catch {{
                                    json = {{ status: 'error', data: [] }};
                                }}
                                if (res.status === 401) {{ window.location.href = '/Account/Login'; return; }}
                                if (!res.ok) {{
                                    console.error('GDataGrid fetch http error:', res.status, json);
                                    this.allRows = [];
                                    this.rows = [];
                                    this.applyColumnFilters(false);
                                    this.currentPage = 1;
                                    this.selectedRow = null;
                                    return;
                                }}
                                const dataRows = Array.isArray(json?.data)
                                    ? json.data
                                    : (Array.isArray(json) ? json : []);
                                this.allRows = dataRows;
                                this.rows = [...dataRows];
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
                                const mergedParams = {{ ...this.buildQueryParams(), ...cleanFilters }};
	                            this.$dispatch('query', {{ filters: mergedParams }});
	                            {(!string.IsNullOrEmpty(OnFilter) ? $"window['{OnFilter}'] && window['{OnFilter}'](mergedParams);" : "")}
	                            // If user doesn't catch the @query event to do a custom fetch,
	                            // we do a default fetch with URLSearchParams.
	                            const params = new URLSearchParams(mergedParams).toString();
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
                            if ('{AutoApply}'.toLowerCase() === 'true' && '{RemoteName}') {{
                                const action = 'update'; // use update path for inline row save
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

        private static List<QueryColumnDefinition> ParseQueryColumns(string raw)
        {
            var result = new List<QueryColumnDefinition>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            try
            {
                var list = JsonSerializer.Deserialize<List<QueryColumnDefinition>>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (list != null) result.AddRange(list);
            }
            catch
            {
                try
                {
                    var one = JsonSerializer.Deserialize<QueryColumnDefinition>(raw, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (one != null) result.Add(one);
                }
                catch
                {
                    return new List<QueryColumnDefinition>();
                }
            }

            foreach (var q in result)
            {
                q.FieldName = (q.FieldName ?? string.Empty).Trim();
                q.Caption = string.IsNullOrWhiteSpace(q.Caption) ? q.FieldName : q.Caption.Trim();
                q.Condition = NormalizeQueryCondition(q.Condition);
                q.AndOr = NormalizeAndOr(q.AndOr);
                q.DataType = NormalizeDataType(q.DataType);
                q.Editor = NormalizeEditor(q.Editor);
                q.Span = Math.Clamp(q.Span <= 0 ? 3 : q.Span, 1, 12);
                q.RowSpan = q.RowSpan <= 0 ? 1 : q.RowSpan;
            }

            return result.Where(q => !string.IsNullOrWhiteSpace(q.FieldName)).ToList();
        }

        private static string NormalizeEditor(string? editor)
        {
            var e = (editor ?? string.Empty).Trim().ToLowerInvariant();
            return e switch
            {
                "checkbox" => "checkbox",
                "numberbox" => "numberbox",
                "validatebox" => "validatebox",
                "datebox" => "datebox",
                "infocombobox" => "gcombobox",
                "gcombobox" => "gcombobox",
                "infocombogrid" => "gcombogrid",
                "gcombogrid" => "gcombogrid",
                "inforefval" => "lovinput",
                "glovinput" => "lovinput",
                "lovinput" => "lovinput",
                "password" => "password",
                _ => "text"
            };
        }

        private static string NormalizeQueryCondition(string? condition)
        {
            var c = (condition ?? string.Empty).Trim();
            return c switch
            {
                "=" => "=",
                "!=" => "!=",
                "<>" => "!=",
                ">" => ">",
                ">=" => ">=",
                "<" => "<",
                "<=" => "<=",
                "%" => "%",
                "%%" => "%%",
                _ => "="
            };
        }

        private static string NormalizeAndOr(string? andOr) =>
            string.Equals((andOr ?? "").Trim(), "OR", StringComparison.OrdinalIgnoreCase) ? "OR" : "AND";

        private static string NormalizeDataType(string? dataType)
        {
            var t = (dataType ?? string.Empty).Trim().ToLowerInvariant();
            return t switch
            {
                "number" => "number",
                "datetime" => "datetime",
                "guid" => "guid",
                _ => "string"
            };
        }

        private static List<(string Value, string Text)> ParseEditorOptions(string options)
        {
            var result = new List<(string, string)>();
            if (string.IsNullOrWhiteSpace(options)) return result;
            foreach (var part in options.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var idx = part.IndexOf('=');
                if (idx <= 0)
                {
                    result.Add((part, part));
                    continue;
                }
                var value = part[..idx].Trim();
                var text = part[(idx + 1)..].Trim();
                result.Add((value, string.IsNullOrWhiteSpace(text) ? value : text));
            }
            return result;
        }

        private static string BuildQueryPanelHtml(List<QueryColumnDefinition> queryColumns)
        {
            if (queryColumns.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine(@"<div class=""g-grid-query border-b border-slate-200 shrink-0"">");
            sb.AppendLine(@"  <div class=""g-grid-query-head flex items-center justify-between px-4 py-3 border-b border-slate-200"">");
            sb.AppendLine(@"    <div class=""inline-flex items-center gap-2 text-blue-600 font-semibold"">");
            sb.AppendLine(@"      <svg class=""w-4 h-4 text-blue-600"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M7 7h10l-4 5v5l-2-1v-4L7 7z""/></svg>");
            sb.AppendLine(@"      <span>查詢條件</span>");
            sb.AppendLine(@"    </div>");
            sb.AppendLine(@"    <button type=""button"" @click=""toggleQueryPanel()"" class=""inline-flex items-center gap-1.5 h-8 px-3 rounded-md border border-slate-300 text-sm font-semibold text-slate-700 bg-white hover:bg-slate-50 transition-colors"">");
            sb.AppendLine(@"      <svg class=""w-3.5 h-3.5 transition-transform"" :class=""queryPanelOpen ? '' : 'rotate-180'"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M19 9l-7 7-7-7""/></svg>");
            sb.AppendLine(@"      <span x-text=""queryPanelOpen ? '收合' : '展開'""></span>");
            sb.AppendLine(@"    </button>");
            sb.AppendLine(@"  </div>");
            sb.AppendLine(@"  <div x-show=""queryPanelOpen"" class=""g-grid-query-body px-4 py-3"">");
            sb.AppendLine(@"    <div class=""grid grid-cols-1 md:grid-cols-12 gap-3"">");

            foreach (var col in queryColumns)
            {
                var field = System.Net.WebUtility.HtmlEncode(col.FieldName);
                var caption = System.Net.WebUtility.HtmlEncode(col.Caption);
                var span = Math.Clamp(col.Span <= 0 ? 3 : col.Span, 1, 12);

                sb.AppendLine($@"    <div class=""md:col-span-{span} flex flex-col gap-1"">");
                sb.AppendLine($@"      <label class=""text-xs font-semibold text-slate-600"">{caption}</label>");

                var model = $"queryValues['{field}']";
                switch (col.Editor)
                {
                    case "numberbox":
                        sb.AppendLine($@"      <input type=""number"" x-model=""{model}"" class=""w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                        break;
                    case "datebox":
                        sb.AppendLine($@"      <input type=""date"" x-model=""{model}"" class=""w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                        break;
                    case "password":
                        sb.AppendLine($@"      <input type=""password"" x-model=""{model}"" class=""w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                        break;
                    case "checkbox":
                        sb.AppendLine($@"      <label class=""inline-flex items-center gap-2 py-2""><input type=""checkbox"" :checked=""{model}==='Y' || {model}===true || {model}===1"" @change=""{model} = $event.target.checked ? 'Y' : 'N'"" class=""w-4 h-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500""><span class=""text-sm text-slate-600"">是/否</span></label>");
                        break;
                    case "gcombobox":
                        sb.AppendLine($@"      <select x-model=""{model}"" class=""w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                        sb.AppendLine(@"        <option value="""">請選擇</option>");
                        foreach (var opt in ParseEditorOptions(col.EditorOptions))
                        {
                            sb.AppendLine($@"        <option value=""{System.Net.WebUtility.HtmlEncode(opt.Value)}"">{System.Net.WebUtility.HtmlEncode(opt.Text)}</option>");
                        }
                        sb.AppendLine(@"      </select>");
                        break;
                    case "gcombogrid":
                    case "lovinput":
                        sb.AppendLine(@"      <div class=""relative flex"">");
                        sb.AppendLine($@"        <input type=""text"" x-model=""{model}"" class=""w-full pr-10 px-3 py-2 text-sm border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                        sb.AppendLine($@"        <button type=""button"" @click=""invokeQueryDefaultMethod('{field}')"" class=""absolute right-2 top-1/2 -translate-y-1/2 text-slate-500 hover:text-blue-600"" title=""選取"">");
                        sb.AppendLine(@"          <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/></svg>");
                        sb.AppendLine(@"        </button>");
                        sb.AppendLine(@"      </div>");
                        break;
                    default:
                        sb.AppendLine($@"      <input type=""text"" x-model=""{model}"" class=""w-full px-3 py-2 text-sm border border-slate-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:outline-none"">");
                        break;
                }
                sb.AppendLine(@"    </div>");
            }

            sb.AppendLine(@"    </div>");
            sb.AppendLine(@"    <div class=""flex items-center gap-2 mt-3"">");
            sb.AppendLine(@"      <button type=""button"" @click=""executeQuery()"" class=""inline-flex items-center gap-1.5 px-3 py-2 text-sm font-semibold text-white bg-blue-600 rounded-lg hover:bg-blue-700"">");
            sb.AppendLine(@"        <svg class=""w-4 h-4"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z""/></svg>");
            sb.AppendLine(@"        <span>查詢</span>");
            sb.AppendLine(@"      </button>");
            sb.AppendLine(@"    </div>");
            sb.AppendLine(@"  </div>");
            sb.AppendLine(@"</div>");

            return sb.ToString();
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









