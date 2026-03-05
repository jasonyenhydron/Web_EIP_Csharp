// 功能：資料模型結構定義與型別約束。
// 輸入：資料來源欄位值或 API 請求資料。
// 輸出：可供控制器、視圖與 API 使用的強型別物件。
// 依賴：Model Binding、序列化、DataAnnotations 驗證機制。

namespace Web_EIP_Csharp.Models.Oracle
{
    public class OracleBindParameterRequest
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string? DbType { get; set; }
        public string? Direction { get; set; }
        public int? Size { get; set; }
    }

    public class OracleProcExecuteRequest
    {
        public string? PackageName { get; set; }
        public string ProcedureName { get; set; } = string.Empty;
        public List<OracleBindParameterRequest>? Parameters { get; set; }
    }

    public class OracleFunctionExecuteRequest
    {
        public string FunctionName { get; set; } = string.Empty;
        public string? ReturnDbType { get; set; }
        public List<OracleBindParameterRequest>? Parameters { get; set; }
    }

    public class OracleSchedulerJobCreateRequest
    {
        public string JobName { get; set; } = string.Empty;
        public string JobType { get; set; } = "PLSQL_BLOCK";
        public string JobAction { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string? RepeatInterval { get; set; }
        public bool Enabled { get; set; } = true;
        public bool AutoDrop { get; set; } = false;
        public string? Comments { get; set; }
    }

    public class OracleSchedulerJobActionRequest
    {
        public string JobName { get; set; } = string.Empty;
        public bool Force { get; set; } = false;
        public bool UseCurrentSession { get; set; } = false;
    }
}

