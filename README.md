# Web_EIP_Csharp

ASP.NET Core MVC (`net10.0`) + Oracle 的企業內部 EIP 系統。

## 主要功能
- MIS 程式清單與查詢：`/mis/programs`
- IDM 功能管理（IDMGD01）：`/Idm/IDMGD01`
- HRM 請假作業（HRMGD47）：`/Hrm/HRMGD47`
- LOV 查詢元件（TagHelper + Runtime Modal）

## 技術棧
- .NET 10 MVC
- Oracle.ManagedDataAccess.Core
- Razor TagHelper
- Alpine.js
- Tailwind CSS

## 啟動方式
1. 設定 `appsettings.json`（Oracle 連線、SessionTimeoutHours）。
2. 執行：

```powershell
dotnet restore
dotnet build
dotnet run
```

預設登入頁：`/Account/Login`

## 關鍵路由
- `GET /mis/programs`
- `GET /Idm/IDMGD01`
- `GET /Hrm/HRMGD47`
- `GET /api/lov/*`

## 專案結構
- `Controllers/`：MVC / API
- `Helpers/OracleDbHelper.cs`：Oracle 連線與查詢
- `Views/Components/`：TagHelper（`g-datagrid`, `g-dataform`, `g-lov-input`...）
- `Views/Shared/`：`_Layout`, `_PopupLayout`, 共用元件
- `wwwroot/js/`：前端 runtime（`g-lov-modal-runtime.js`, `g-dataform.js`, `sidebar.js`...）
- `wwwroot/css/`：主樣式
- `doc/`：使用說明與 SA/SD

## 目前規格重點（2026-03）
- LOV Modal 已改為 Runtime 注入：
  - 由 `GLovInputTagHelper` 自動注入 `/js/g-lov-modal-runtime.js`（每頁一次）
  - `Views/Shared/_LovModal.cshtml` 已移除
- `g-lov-input` 查詢模式：
  - `selectonly=false` 時改為「只按 Enter 查詢」
  - 不再用 `oninput` 自動查詢
- LOV 選取穩定性：
  - 修正雙擊列 / 按確定後 popup 重新跳出問題（回填事件抑制 reopen）
- `g-datagrid`：
  - body 固定滾動容器（`--g-grid-body-max-height`）
  - 分頁區顯示總筆數
  - 多欄排序 / header filter / inline edit 行為修正
- `g-dataform` 升級：
  - 支援 `<form-column>` 子標籤定義欄位
  - 支援 CRUD / Modal / 驗證 / Master-Detail / LOV / Select options API
  - Runtime：`/js/g-dataform.js`
- Sidebar：
  - 修正收合按鈕偶發無效（避免重複初始化 + collapsed 寬度覆蓋）

## 注意事項
- `g-dataform` 請使用 `member-id`，不要直接在 TagHelper 屬性用 `data-member`。
- 若頁面有快取，前端 JS/CSS 更新後請用 `Ctrl+F5` 強制重整。
