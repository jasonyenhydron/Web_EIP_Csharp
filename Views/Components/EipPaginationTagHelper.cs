using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * EipPaginationTagHelper — 對應 jeasyui Pagination
 * 伺服器端分頁版本（配合 Alpine.js 狀態使用）
 *
 * 用法（Server-side，與 Alpine 搭配）：
 *   <eip-pagination
 *       alpine-total="totalPages"
 *       alpine-current="currentPage"
 *       alpine-prev="prevPage()"
 *       alpine-next="nextPage()"
 *       alpine-count="programs.length"
 *       page-size-options="10,20,50"
 *       alpine-page-size="pageSize"/>
 *
 * 所有 alpine-* 屬性直接做為 Alpine.js x-bind 綁定的表達式
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("eip-pagination")]
    public class EipPaginationTagHelper : TagHelper
    {
        /// <summary>Alpine 總頁數表達式，如 totalPages</summary>
        public string AlpineTotal   { get; set; } = "totalPages";

        /// <summary>Alpine 目前頁數表達式，如 currentPage</summary>
        public string AlpineCurrent { get; set; } = "currentPage";

        /// <summary>Alpine 上一頁 method，如 prevPage()</summary>
        public string AlpinePrev    { get; set; } = "prevPage()";

        /// <summary>Alpine 下一頁 method，如 nextPage()</summary>
        public string AlpineNext    { get; set; } = "nextPage()";

        /// <summary>Alpine 跳頁 method（帶參數），如 jumpPage($event.target.value)</summary>
        public string AlpineJump    { get; set; } = "jumpPage($event.target.value)";

        /// <summary>Alpine 總筆數表達式，如 programs.length</summary>
        public string AlpineCount   { get; set; } = "";

        /// <summary>Alpine 每頁筆數變數，如 pageSize</summary>
        public string AlpinePageSize { get; set; } = "pageSize";

        /// <summary>每頁筆數選項（逗號分隔），如 10,20,50</summary>
        public string PageSizeOptions { get; set; } = "10,20,50";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var options = PageSizeOptions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Select(s => $"""<option :selected="{AlpinePageSize}=={s}" value="{s}">{s} 筆/頁</option>""");
            var optionHtml = string.Join("\n", options);
            var countHtml  = !string.IsNullOrEmpty(AlpineCount)
                ? $"""<span class="text-slate-400">共 <span x-text="{AlpineCount}" class="font-bold text-slate-600"></span> 筆</span>"""
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("class",
                "flex flex-wrap items-center justify-between gap-3 px-4 py-2.5 bg-slate-50 border-t border-slate-200 text-sm");

            output.Content.SetHtmlContent($"""
                <!-- 左側：筆數資訊 + 每頁筆數 -->
                <div class="flex items-center gap-3 flex-wrap">
                    {countHtml}
                    <select x-model="{AlpinePageSize}" @@change="currentPage=1"
                            class="pl-2 pr-6 py-1 text-xs border border-slate-300 rounded-lg bg-white focus:outline-none focus:ring-1 focus:ring-blue-400 text-slate-600 cursor-pointer">
                        {optionHtml}
                    </select>
                </div>

                <!-- 右側：分頁控制 -->
                <div class="flex items-center gap-1">
                    <!-- 上一頁 -->
                    <button type="button" @@click="{AlpinePrev}" :disabled="{AlpineCurrent}<=1"
                            :class="{AlpineCurrent}<=1 ? 'opacity-40 cursor-not-allowed' : 'hover:bg-slate-200'"
                            class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
                        </svg>
                    </button>

                    <!-- 頁碼輸入 -->
                    <div class="flex items-center gap-1.5 px-2">
                        <input type="number" min="1" :max="{AlpineTotal}"
                               :value="{AlpineCurrent}"
                               @@change="{AlpineJump}"
                               class="w-14 text-center text-sm border border-slate-300 rounded-lg py-1 focus:outline-none focus:ring-1 focus:ring-blue-400">
                        <span class="text-slate-400 text-xs">/</span>
                        <span x-text="{AlpineTotal}" class="text-xs font-bold text-slate-600 min-w-[1rem] text-center"></span>
                        <span class="text-slate-400 text-xs">頁</span>
                    </div>

                    <!-- 下一頁 -->
                    <button type="button" @@click="{AlpineNext}" :disabled="{AlpineCurrent}>={AlpineTotal}"
                            :class="{AlpineCurrent}>={AlpineTotal} ? 'opacity-40 cursor-not-allowed' : 'hover:bg-slate-200'"
                            class="w-8 h-8 flex items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-600 transition-colors">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                        </svg>
                    </button>
                </div>
            """);
        }
    }
}
