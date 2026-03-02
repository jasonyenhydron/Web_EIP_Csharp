using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web_EIP_Csharp.Views.Components
{
    /// <summary>
    /// g-iframe-modal：含 iframe 的程式執行視窗元件
    /// 含漸層標題列、最大化/還原按鈕、關閉按鈕、iframe 內容區。
    /// 不同於 g-dialog（用於表單對話框），此元件專為載入子頁面（程式執行）設計。
    ///
    /// 使用方式（MisPrograms.cshtml）：
    ///   <g-iframe-modal
    ///       id="executionModal"
    ///       modal-content-id="executionModalContent"
    ///       iframe-id="executionIframe"
    ///       title-id="executionModalTitle"
    ///       title="程式執行"
    ///       gradient="indigo"
    ///       maximize-btn-id="execMaximizeBtn"
    ///       maximize-icon-id="execMaximizeIcon"
    ///       restore-icon-id="execRestoreIcon"
    ///       close-fn="closeExecutionModal()"
    ///       maximize-fn="toggleExecutionMaximize()" />
    ///
    /// JS 開啟：openExecutionModal('/mis/programs/IDMGD01', 'IDMGD01 程式維護')
    /// gradient 可選：indigo（預設）| blue | green | slate
    /// </summary>
    [HtmlTargetElement("g-iframe-modal")]
    public class GIframeModalTagHelper : TagHelper
    {
        // ── HTML IDs ──
        /// <summary>最外層 div id（供 JS 顯示/隱藏）</summary>
        [HtmlAttributeName("id")]
        public string Id { get; set; } = "executionModal";

        /// <summary>內層內容 div id（供最大化/還原控制）</summary>
        public string ModalContentId { get; set; } = "executionModalContent";

        /// <summary>iframe 元素 id</summary>
        public string IframeId { get; set; } = "executionIframe";

        /// <summary>標題 span id（供 JS 動態更新標題）</summary>
        public string TitleId { get; set; } = "executionModalTitle";

        // ── 最大化按鈕 IDs ──
        public string MaximizeBtnId { get; set; } = "execMaximizeBtn";
        public string MaximizeIconId { get; set; } = "execMaximizeIcon";
        public string RestoreIconId  { get; set; } = "execRestoreIcon";

        // ── 文字與行為 ──
        /// <summary>預設標題文字</summary>
        public string Title { get; set; } = "程式執行";

        /// <summary>標題列漸層色：indigo | blue | green | slate</summary>
        public string Gradient { get; set; } = "indigo";

        /// <summary>關閉按鈕 JS：onclick 執行的函式名</summary>
        public string CloseFn { get; set; } = "closeExecutionModal()";

        /// <summary>最大化切換 JS：onclick 執行的函式名</summary>
        public string MaximizeFn { get; set; } = "toggleExecutionMaximize()";

        /// <summary>iframe 初始高度（百分比），預設 95vh</summary>
        public string Height { get; set; } = "95vh";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("id", Id);
            output.Attributes.SetAttribute("class",
                "fixed inset-0 bg-slate-900/60 backdrop-blur-sm hidden z-[60] " +
                "items-center justify-center p-4 transition-all duration-300");

            // 標題列漸層色
            string gradientClass = Gradient?.ToLower() switch
            {
                "blue"  => "from-blue-600 to-blue-700",
                "green" => "from-green-600 to-teal-700",
                "slate" => "from-slate-700 to-slate-800",
                _       => "from-indigo-600 to-blue-700"
            };

            // 標題 icon（程式碼樣式）
            string titleIcon = @"<svg class=""w-5 h-5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4""/></svg>";

            // 最大化 icon
            string maximizeIcon = @"<svg class=""w-5 h-5"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                      d=""M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4""/></svg>";

            // 還原 icon
            string restoreIcon = @"<svg class=""w-[85%] h-[85%] hidden absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                      d=""M9 9L4 4m0 0v4m0-4h4m12 12l-5-5m5 5v-4m0 4h-4M9 15l-5 5m0 0v-4m0 4h4m11-11l-5 5m5-5v4m0-4h-4""/></svg>";

            // 關閉 icon
            string closeIcon = @"<svg class=""w-6 h-6"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""/></svg>";

            string html = $@"
<div class=""bg-white rounded-2xl shadow-2xl w-full max-w-7xl flex flex-col overflow-hidden border border-slate-200 transform scale-95 transition-transform duration-300""
     id=""{HtmlEncode(ModalContentId)}"" style=""height:{HtmlEncode(Height)}"">

    <!-- 標題列 -->
    <div class=""bg-gradient-to-r {gradientClass} text-white px-4 py-3 flex items-center justify-between shadow-lg shrink-0"">
        <h2 class=""text-lg font-bold flex items-center gap-3"">
            {titleIcon}
            <span id=""{HtmlEncode(TitleId)}"">{HtmlEncode(Title)}</span>
        </h2>
        <div class=""flex items-center gap-1"">
            <!-- 最大化按鈕 -->
            <button type=""button""
                    id=""{HtmlEncode(MaximizeBtnId)}""
                    onclick=""{HtmlAttr(MaximizeFn)}""
                    class=""text-white/70 hover:text-white hover:bg-white/10 w-8 h-8 flex items-center justify-center rounded-lg transition-all relative"">
                <span id=""{HtmlEncode(MaximizeIconId)}"">{maximizeIcon}</span>
                {restoreIcon.Replace(@"class=""w-[85%]", $@"id=""{HtmlEncode(RestoreIconId)}"" class=""w-[85%]")")}
            </button>
            <!-- 關閉按鈕 -->
            <button type=""button""
                    onclick=""{HtmlAttr(CloseFn)}""
                    class=""text-white/70 hover:text-white hover:bg-white/10 p-1.5 rounded-lg transition-all"">
                {closeIcon}
            </button>
        </div>
    </div>

    <!-- iframe 內容區 -->
    <div class=""flex-1 bg-slate-50 relative"">
        <iframe id=""{HtmlEncode(IframeId)}"" src=""""
                class=""absolute inset-0 w-full h-full border-0 rounded-b-2xl"">
        </iframe>
    </div>

</div>";

            output.Content.SetHtmlContent(html);
        }

        private static string HtmlEncode(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
        private static string HtmlAttr(string? s)    => s?.Replace("\"", "&quot;") ?? string.Empty;
    }
}
