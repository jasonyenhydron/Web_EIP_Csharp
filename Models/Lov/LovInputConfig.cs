// 功能：資料模型結構定義與型別約束。
// 輸入：資料來源欄位值或 API 請求資料。
// 輸出：可供控制器、視圖與 API 使用的強型別物件。
// 依賴：Model Binding、序列化、DataAnnotations 驗證機制。

namespace Web_EIP_Csharp.Models.Lov
{
    public class LovInputConfig
    {
        public string? Title { get; set; }
        public string? Api { get; set; }
        public string? Columns { get; set; }
        public string? Fields { get; set; }
        public string? KeyHidden { get; set; }
        public string? KeyCode { get; set; }
        public string? KeyName { get; set; }
        public string? DisplayFormat { get; set; }
        public string? OnConfirm { get; set; }
        public bool? BufferView { get; set; }
        public int? PageSize { get; set; }
        public bool? SortEnabled { get; set; }
    }
}

