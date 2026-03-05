// 功能：員工請假檔資料模型，對應 `HRM_EM_ASK_FOR_LEAVE` 資料表，提供請假資料新增、修改、刪除、查詢的欄位結構。
// 輸入：`employee_id`、`start_time`、`end_time`、`leave_id` 與其他請假相關欄位值。
// 輸出：員工請假資料物件（供控制器接收請求、寫入資料庫與回傳 API 使用）。
// 依賴：`HrmGd47Contorller`、`HRMGD47.cshtml`、ASP.NET Core Model Binding、DataAnnotations。

using System;
using System.ComponentModel.DataAnnotations;

namespace Web_EIP_Csharp.Models
{
    public class HrmEmAskForLeave
    {
        public long? EmAskForLeaveId { get; set; } // 員工請假ID (由 Sequence 自動產生)

        [Required]
        public long EmployeeId { get; set; } // 員工ID

        [Required]
        public DateTime StartTime { get; set; } // 請假開始時間(至分鐘)

        [Required]
        public DateTime EndTime { get; set; } // 請假結束時間(至分鐘)

        [Required]
        public long LeaveId { get; set; } // 假別ID (如：2002 調休假, 1001 特休假)

        public decimal? SystemLeaveHours { get; set; } // 系統請假時數
        public decimal? LeaveHours { get; set; } // 請假時數
        public decimal? LeaveDays { get; set; } // 請假日數

        public long? PeriodId { get; set; } // 歸屬期別

        [MaxLength(256)]
        public string AskForLeaveReason { get; set; } // 請假理由 (事由)

        public DateTime? TimeCardDate { get; set; } // 考勤卡日(由系統自動產生資料時用)

        [MaxLength(2)]
        public string EmAskForLeaveStatus { get; set; } = "00"; // 狀態 00:申請中, 95:已確認完畢, 99:註銷, 94:不予核淮, 92:核決中

        [MaxLength(1)]
        public string FlowYn { get; set; } = "N"; // 是否進行流程

        public long? OvertimePeriodId { get; set; }

        [MaxLength(30)]
        public string ConfirmUser { get; set; }

        public long? AgentEmployeeId { get; set; } // 代理人員工ID

        [MaxLength(100)]
        public string DestinationPlace { get; set; } // 公出地點

        [MaxLength(100)]
        public string TalkingAbout { get; set; } // 接洽對象

        [MaxLength(1)]
        public string ReturnYn { get; set; } // 是否返回工作地點 (Y/N)

        public long? PayApplyId { get; set; }

        [MaxLength(30)]
        public string TransportationCode { get; set; } // 交通工具代碼

        public decimal? UnitAmt { get; set; }
        public decimal? Amt { get; set; }
        public decimal? Distance { get; set; }
        public long? CompanyCarId { get; set; }

        [MaxLength(30)]
        public string EntryId { get; set; } // 建檔人
        public DateTime? EntryDate { get; set; } // 建檔日

        [MaxLength(30)]
        public string TrId { get; set; } // 異動人
        public DateTime? TrDate { get; set; } // 異動日

        [MaxLength(1)]
        public string OfferedCertificateYn { get; set; } = "N"; // 已提供證明文件

        public DateTime? VestingDate { get; set; }

        [MaxLength(2)]
        public string ReconHoursStatus { get; set; }

        [MaxLength(1)]
        public string OverseasYn { get; set; } // 海外出差 (Y/N)

        [MaxLength(1)]
        public string DocHrConfirmYn { get; set; }

        [MaxLength(30)]
        public string DocHrConfirmUser { get; set; }
        public DateTime? DocHrConfirmTime { get; set; }
    }
}
