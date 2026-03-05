// 功能：PRMPQ11 視圖模型，提供頁面預設查詢條件與顯示所需初始化資料。
// 輸入：前端畫面載入時的預設組織與狀態條件。
// 輸出：PRMPQ11.cshtml 可直接綁定使用的預設值屬性。
// 依賴：Views/MisPrograms/PRMPQ11.cshtml。
namespace Web_EIP_Csharp.Models.ViewModels
{
    public class PrmPq11ViewModel
    {
        public int OrganizationId { get; set; } = 10611;
        public string StatusTo { get; set; } = "95";
    }
}
