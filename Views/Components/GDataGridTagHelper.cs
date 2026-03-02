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
 *   寬度與對齊可省略，寬度省略時 flex:1
 *
 * on-row-click : row 被 click 時的 JS 回呼（參數為 row 物件）
 * on-row-dblclick : row 被雙擊時的 JS 回呼
 * toolbar-html : 表格上方工具列的 HTML 字串（可加入自訂按鈕）
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("g-datagrid")]
    public class GDataGridTagHelper : TagHelper
    {
        public string Id           { get; set; } = "";
        public string ApiUrl       { get; set; } = "";
        public string Columns      { get; set; } = "";  // "field:title:width:align, ..."
        public int    PageSize     { get; set; } = 20;
        public bool   Striped      { get; set; } = true;
        public bool   ShowRowNum   { get; set; } = false;  // 顯示行號欄
        public string OnRowClick   { get; set; } = "";
        public string OnRowDblClick{ get; set; } = "";
        public string ToolbarHtml  { get; set; } = "";
        public string Class        { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var compId   = string.IsNullOrEmpty(Id) ? $"grid_{Guid.NewGuid():N}" : Id;
            var fnName   = $"gDataGrid_{compId}";
            var striped  = Striped ? "odd:bg-white even:bg-slate-50/60" : "";
            var rowClick = !string.IsNullOrEmpty(OnRowClick)
                ? $"onclick=\"({OnRowClick})(row)\""
                : "";
            var rowDbl   = !string.IsNullOrEmpty(OnRowDblClick)
                ? $"ondblclick=\"({OnRowDblClick})(row)\""
                : "";
            var rowCursor = (!string.IsNullOrEmpty(OnRowClick) || !string.IsNullOrEmpty(OnRowDblClick))
                ? "cursor-pointer" : "";

            // 解析 columns
            var cols = ParseColumns(Columns);
            var thHtml = new System.Text.StringBuilder();
            var tdHtml = new System.Text.StringBuilder();

            if (ShowRowNum)
            {
                thHtml.Append("""
                    <th class="px-3 py-2.5 text-center text-xs font-bold text-slate-500 uppercase tracking-wider bg-slate-100 border-b-2 border-slate-200 w-10 shrink-0">#</th>
                """);
                tdHtml.Append("""
                    <td class="px-3 py-2 text-center text-xs text-slate-400 border-b border-slate-100" x-text="(currentPage-1)*pageSize+rowIdx+1"></td>
                """);
            }

            foreach (var (field, title, width, align) in cols)
            {
                var wStyle  = string.IsNullOrEmpty(width) ? "" : $"width:{width}px;min-width:{width}px;";
                var thAlign = align is "center" or "right" ? $"text-{align}" : "text-left";
                var tdAlign = align is "center" or "right" ? $"text-{align}" : "text-left";
                thHtml.Append($"""
                    <th @@dblclick="toggleSort('{field}')" style="{wStyle}"
                        class="px-3 py-2.5 {thAlign} text-xs font-bold text-slate-500 uppercase tracking-wider bg-slate-100 border-b-2 border-slate-200 cursor-pointer hover:bg-slate-200 transition-colors select-none whitespace-nowrap group">
                        <span class="inline-flex items-center gap-1">
                            {title}
                            <svg x-show="sortKey==='{field}'" :class="sortDir==='asc'?'':'rotate-180'"
                                 class="w-3 h-3 shrink-0 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7"/>
                            </svg>
                        </span>
                    </th>
                """);
                tdHtml.Append($"""
                    <td class="px-3 py-2 {tdAlign} text-sm text-slate-700 border-b border-slate-100 whitespace-nowrap" x-text="row['{field}']??''"></td>
                """);
            }

            output.TagName = "div";
            output.Attributes.SetAttribute("id", compId);
            output.Attributes.SetAttribute("class", $"bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden flex flex-col {Class}");
            output.Attributes.SetAttribute("x-data", $"{fnName}()");
            output.Attributes.SetAttribute("x-init", "init()");

            output.Content.SetHtmlContent($"""
                <!-- Toolbar -->
                <div class="flex items-center justify-between gap-3 px-4 py-2.5 border-b border-slate-200 bg-slate-50/70 shrink-0 flex-wrap">
                    <div class="flex items-center gap-2">
                        {ToolbarHtml}
                        <button type="button" @@click="fetchData()" title="重新整理"
                                class="flex items-center gap-1 px-2.5 py-1.5 text-xs text-slate-600 hover:text-blue-700 hover:bg-blue-50 rounded-lg border border-slate-200 bg-white transition-colors font-medium">
                            <svg class="w-3.5 h-3.5" :class="loading?'animate-spin':''" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
                            </svg>
                            重整
                        </button>
                    </div>
                    <span class="text-xs text-slate-400">共 <span class="font-bold text-slate-600" x-text="rows.length"></span> 筆</span>
                </div>

                <!-- Table -->
                <div class="overflow-auto flex-1" style="min-height:120px;">
                    <!-- Loading -->
                    <div x-show="loading" class="flex items-center justify-center py-12 text-slate-400 gap-2">
                        <svg class="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
                            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
                            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                        </svg>
                        <span class="text-sm">載入中...</span>
                    </div>
                    <!-- Empty -->
                    <div x-show="!loading && rows.length===0" class="flex flex-col items-center justify-center py-12 text-slate-400">
                        <svg class="w-10 h-10 mb-2 text-slate-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                        </svg>
                        <p class="text-sm">無資料</p>
                    </div>
                    <!-- Table -->
                    <table x-show="!loading && rows.length>0" class="min-w-full border-collapse">
                        <thead class="sticky top-0 z-10">
                            <tr>{thHtml}</tr>
                        </thead>
                        <tbody>
                            <template x-for="(row, rowIdx) in pagedRows" :key="rowIdx">
                                <tr class="group {striped} hover:bg-blue-50/50 transition-colors {rowCursor}"
                                    :class="selectedRow===row?'bg-blue-100/60 outline outline-1 outline-blue-400':''"
                                    {rowClick} {rowDbl}
                                    @@click="selectedRow=row">
                                    {tdHtml}
                                </tr>
                            </template>
                        </tbody>
                    </table>
                </div>

                <!-- Pagination -->
                <div class="flex flex-wrap items-center justify-between gap-3 px-4 py-2 bg-slate-50/80 border-t border-slate-200 text-sm shrink-0 select-none">
                    <div class="flex items-center gap-2">
                        <select x-model="pageSize" @@change="currentPage=1"
                                class="pl-2 pr-6 py-1 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-blue-400 cursor-pointer">
                            <option value="10">10 筆/頁</option>
                            <option value="20">20 筆/頁</option>
                            <option value="50">50 筆/頁</option>
                            <option value="100">100 筆/頁</option>
                        </select>
                    </div>
                    <div class="flex items-center gap-1">
                        <button type="button" @@click="prevPage()" :disabled="currentPage<=1"
                                :class="currentPage<=1?'opacity-40 cursor-not-allowed':'hover:bg-slate-200'"
                                class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/></svg>
                        </button>
                        <div class="flex items-center gap-1.5 px-2">
                            <input type="number" min="1" :max="totalPages" :value="currentPage"
                                   @@change="jumpPage($event.target.value)"
                                   class="w-14 text-center text-xs border border-slate-300 rounded-lg py-1.5 focus:outline-none focus:ring-1 focus:ring-blue-400">
                            <span class="text-slate-400 text-xs">/</span>
                            <span x-text="totalPages" class="text-xs font-bold text-slate-700"></span>
                            <span class="text-slate-400 text-xs">頁</span>
                        </div>
                        <button type="button" @@click="nextPage()" :disabled="currentPage>=totalPages"
                                :class="currentPage>=totalPages?'opacity-40 cursor-not-allowed':'hover:bg-slate-200'"
                                class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/></svg>
                        </button>
                    </div>
                </div>

                <!-- Alpine Component Script -->
                <script>
                function {fnName}() {{
                    return {{
                        rows       : [],
                        loading    : false,
                        sortKey    : '',
                        sortDir    : 'asc',
                        currentPage: 1,
                        pageSize   : {PageSize},
                        selectedRow: null,

                        get sortedRows() {{
                            if (!this.sortKey) return this.rows;
                            const dir = this.sortDir === 'asc' ? 1 : -1;
                            return [...this.rows].sort((a, b) => {{
                                const av = a[this.sortKey] ?? '';
                                const bv = b[this.sortKey] ?? '';
                                return av < bv ? -dir : av > bv ? dir : 0;
                            }});
                        }},
                        get totalPages()  {{ return Math.max(1, Math.ceil(this.sortedRows.length / this.pageSize)); }},
                        get pagedRows()   {{
                            const s = (this.currentPage - 1) * this.pageSize;
                            return this.sortedRows.slice(s, s + Number(this.pageSize));
                        }},

                        async init()     {{ await this.fetchData(); }},
                        async fetchData() {{
                            this.loading = true;
                            try {{
                                const res  = await fetch('{ApiUrl}');
                                const json = await res.json();
                                if (res.status === 401) {{ window.location.href = '/Account/Login'; return; }}
                                this.rows  = json.data ?? json;
                                this.currentPage = 1;
                            }} catch(e) {{ console.error('GDataGrid fetch error:', e); }}
                            finally    {{ this.loading = false; }}
                        }},
                        toggleSort(key) {{
                            if (this.sortKey === key) this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
                            else {{ this.sortKey = key; this.sortDir = 'asc'; }}
                        }},
                        prevPage() {{ if (this.currentPage > 1) this.currentPage--; }},
                        nextPage() {{ if (this.currentPage < this.totalPages) this.currentPage++; }},
                        jumpPage(v) {{
                            const p = parseInt(v);
                            if (!isNaN(p)) this.currentPage = Math.min(Math.max(1, p), this.totalPages);
                        }},
                        // 供外部使用：取得目前選中列
                        getSelectedRow() {{ return this.selectedRow; }},
                        // 供外部使用：重新整理
                        refresh() {{ this.fetchData(); }}
                    }};
                }}
                </script>
            """);
        }

        /// <summary>解析 columns 字串，格式：field:title:width:align</summary>
        private static List<(string Field, string Title, string Width, string Align)> ParseColumns(string cols)
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
                        Align : parts.ElementAtOrDefault(3)?.Trim() ?? "left"
                    );
                })
                .Where(c => !string.IsNullOrEmpty(c.Field))
                .ToList();
        }
    }
}
