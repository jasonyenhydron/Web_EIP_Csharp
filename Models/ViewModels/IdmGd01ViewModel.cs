// 功能：資料模型結構定義與型別約束。
// 輸入：資料來源欄位值或 API 請求資料。
// 輸出：可供控制器、視圖與 API 使用的強型別物件。
// 依賴：Model Binding、序列化、DataAnnotations 驗證機制。

using Web_EIP_Csharp.Models.Lov;

namespace Web_EIP_Csharp.Models.ViewModels
{
    public class IdmGd01ViewModel
    {
        public LovInputConfig EmployeeLov { get; set; } = new();
    }
}

