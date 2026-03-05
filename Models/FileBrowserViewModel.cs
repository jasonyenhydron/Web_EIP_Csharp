// 功能：資料模型結構定義與型別約束。
// 輸入：資料來源欄位值或 API 請求資料。
// 輸出：可供控制器、視圖與 API 使用的強型別物件。
// 依賴：Model Binding、序列化、DataAnnotations 驗證機制。

namespace Web_EIP_Csharp.Models
{
    public class FileBrowserViewModel
    {
        public string RootDisplay { get; set; } = string.Empty;
        public string CurrentRelativePath { get; set; } = string.Empty;
        public string? ParentRelativePath { get; set; }
        public string? ErrorMessage { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
        public bool RequiresWindowsAuth { get; set; }
        public bool IsWindowsAuthPassed { get; set; }
        public string AuthUserName { get; set; } = string.Empty;
        public string RequestedPath { get; set; } = string.Empty;
        public List<FileEntryViewModel> Entries { get; set; } = new();
    }

    public class FileEntryViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long? SizeBytes { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
