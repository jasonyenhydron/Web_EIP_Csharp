using Microsoft.AspNetCore.Razor.TagHelpers;

/*
 * EipDialogTagHelper — 對應 jeasyui Dialog
 * 用法：
 *   <eip-dialog id="confirmDlg" title="確認刪除" width="md" trigger-id="btnDelete" close-btn="true">
 *       <p>確定要刪除這筆資料嗎？</p>
 *       <div class="flex justify-end gap-2 mt-4">
 *           <eip-button text="確認" type="danger" onclick="doDelete(); eipDialogClose('confirmDlg')"/>
 *           <eip-button text="取消" type="secondary" onclick="eipDialogClose('confirmDlg')"/>
 *       </div>
 *   </eip-dialog>
 *
 * width: sm | md | lg | xl | full
 * 在 JS 中呼叫：
 *   eipDialogOpen('confirmDlg')
 *   eipDialogClose('confirmDlg')
 */
namespace Web_EIP_Csharp.Views.Components
{
    [HtmlTargetElement("eip-dialog")]
    public class EipDialogTagHelper : TagHelper
    {
        /// <summary>Dialog ID（必填）</summary>
        public string Id { get; set; } = "";

        /// <summary>對話框標題</summary>
        public string Title { get; set; } = "";

        /// <summary>寬度：sm(384px) | md(512px) | lg(768px) | xl(1024px) | full</summary>
        public string Width { get; set; } = "md";

        /// <summary>顯示右上角關閉按鈕</summary>
        public bool CloseBtn { get; set; } = true;

        /// <summary>點擊背景關閉</summary>
        public bool BackdropClose { get; set; } = true;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var content = (await output.GetChildContentAsync()).GetContent();
            var maxW    = Width switch
            {
                "sm"   => "max-w-sm",
                "md"   => "max-w-lg",
                "lg"   => "max-w-3xl",
                "xl"   => "max-w-5xl",
                "full" => "max-w-full mx-4",
                _      => "max-w-lg"
            };
            var backdropClick = BackdropClose
                ? $"""onclick="if(event.target===this)eipDialogClose('{Id}')" """
                : "";
            var closeBtnHtml = CloseBtn
                ? $"""<button type="button" onclick="eipDialogClose('{Id}')" class="text-slate-400 hover:text-slate-600 hover:bg-slate-100 p-1.5 rounded-lg transition-all"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg></button>"""
                : "";

            output.TagName = "div";
            output.Attributes.SetAttribute("id", Id);
            output.Attributes.SetAttribute("role", "dialog");
            output.Attributes.SetAttribute("aria-modal", "true");
            output.Attributes.SetAttribute("class", "fixed inset-0 bg-slate-900/60 backdrop-blur-sm hidden z-[200] items-center justify-center p-4");
            output.Attributes.SetAttribute("style", "display:none;");

            output.Content.SetHtmlContent($"""
                <div class="bg-white rounded-2xl shadow-2xl w-full {maxW} flex flex-col border border-slate-200 transform transition-all duration-200 scale-95 opacity-0" id="{Id}-content">
                    <!-- Dialog Header -->
                    <div class="flex items-center justify-between px-5 py-4 border-b border-slate-200 bg-gradient-to-r from-blue-600 to-blue-700 rounded-t-2xl">
                        <h3 class="text-base font-bold text-white flex items-center gap-2">
                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                            {Title}
                        </h3>
                        <div class="flex items-center gap-1 text-white">
                            {closeBtnHtml}
                        </div>
                    </div>
                    <!-- Dialog Body -->
                    <div class="p-5 overflow-y-auto max-h-[75vh]">
                        {content}
                    </div>
                </div>
            """);
        }
    }
}
