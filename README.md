# Web_EIP_Csharp

ASP.NET Core MVC (`net10.0`) + Oracle 的 EIP Web 專案。  
目前重點模組為：
- MIS 程式清單與執行入口 (`/mis/programs`)
- IDM 程式資料維護 (`/Idm/IDMGD01`)
- HRM 請假示例頁 (`/Hrm/HRMGD47`)
- LOV API 與共用 TagHelper 元件

## Tech Stack
- .NET 10 MVC
- Oracle.ManagedDataAccess.Core
- Tailwind CSS (CDN)
- Alpine.js
- Razor TagHelper (大量 UI 元件化)

## 專案啟動
1. 安裝 .NET 10 SDK 與 Oracle 可連線環境。
2. 設定 `appsettings.json`（目前 Session 逾時可調 `SessionTimeoutHours`）。
3. 執行：

```powershell
dotnet restore
dotnet build
dotnet run
```

預設路由會導向 `/Account/Login`。

## 主要路由
- `GET /Account/Login`：登入
- `GET /Dashboard/Index`：首頁
- `GET /mis/programs`：MIS 程式查詢頁
- `GET /Idm/IDMGD01`：IDM popup 維護頁
- `GET /Hrm/HRMGD47`：HRM popup 示範頁
- `GET /api/lov/*`：LOV 查詢 API

## 專案結構
- `Controllers/`：MVC Controller 與 API
- `Helpers/OracleDbHelper.cs`：Oracle 查詢與連線封裝
- `Views/Components/`：自訂 TagHelper（`g-datagrid`, `g-dataform`, `g-lov-input`, `g-combobox`, `g-error-message`...）
- `Views/Shared/`：`_Layout`, `_PopupLayout`, `_LovModal`, icon sprite
- `Views/MisPrograms/`：MIS/IDM/HRM 頁面
- `wwwroot/js`：前端腳本（`g-components.js`, `mis_programs.js`, `sidebar.js`）
- `wwwroot/css`：樣式
- `doc/`：SA/SD/元件使用說明

## 已實作的關鍵能力
- Datagrid CRUD（查詢/新增/編輯/刪除）
- Datagrid title filter（漏斗）與排序（支援多欄位）
- LOV Modal（buffer view 每次 50 筆、可選排序）
- Popup Layout 右下角關閉鈕 + 未存檔離開確認
- 全域錯誤顯示元件 `<g-error-message />`（可攔截前後端例外並複製訊息）
- CSS/JS 共用載入元件：`<g-style>`、`<g-js>`

## 開發原則（本專案）
- 優先使用 `Views/Components` 的 TagHelper 組頁。
- 只有元件無法達成時，才補寫頁面專屬 HTML/JS。
- 新增共用功能優先抽成 TagHelper，避免重複。

## 常見問題
- `InvalidOperationException: The view 'XXX' was not found`
  - 檢查 Controller `return View(...)` 的實體路徑是否對應到 `Views/.../*.cshtml`。
- LOV API 500 / ORA-00933
  - 優先檢查 SQL 字串拼接與條件參數是否一致。
- Datagrid 按鈕沒反應
  - 先看 Console 是否有 Alpine 表達式錯誤（例如變數未定義）。

## Update Notes (2026-03-03)
- `g-datagrid` table body now uses a constrained scroll area (`g-grid-body`) with `overflow-y-auto` and default `max-height: var(--g-grid-body-max-height, 52vh)`.
- Pagination now shows total record count (`共 X 筆`) on the left side of page switch controls.
- Toolbar total count is now shown only when pagination is disabled, to avoid duplicate count display.
- `IdmController` role-function query added schema compatibility for `FUNCTION_NO` / `FUNCTION_ID` and fallback SQL, reducing Oracle invalid identifier failures.
- Datagrid fetch flow now guards against malformed/non-array responses to avoid client-side iteration errors.
