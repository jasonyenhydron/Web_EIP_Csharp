// 功能：資料模型結構定義與型別約束。
// 輸入：資料來源欄位值或 API 請求資料。
// 輸出：可供控制器、視圖與 API 使用的強型別物件。
// 依賴：Model Binding、序列化、DataAnnotations 驗證機制。

namespace Web_EIP_Csharp.Models.MISTreeMenu
{
    public class IdmProgramVModel
    {
        public int? LanguageId { get; set; }
        public long? ProgramId { get; set; }
        public string ProgramNo { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string PlanStartDevelopDate { get; set; } = string.Empty;
        public string PlanFinishDevelopDate { get; set; } = string.Empty;
        public string RealStartDevelopDate { get; set; } = string.Empty;
        public string RealFinishDevelopDate { get; set; } = string.Empty;
        public decimal? PlanWorkHours { get; set; }
        public decimal? RealWorkHours { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string ProgramType { get; set; } = string.Empty;
    }
}
