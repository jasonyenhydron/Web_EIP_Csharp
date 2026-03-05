# SA - Software Architecture

## 1. 目標
本系統是 Oracle ERP/EIP Web 前端入口，提供：
- 登入與 Session 管理
- MIS 程式清單與程式啟動
- IDM/HRM 範例維護頁
- 可重用 UI 元件（TagHelper）與 LOV 查詢模式

## 2. 架構風格
- **Server-side MVC + API 混合**
  - Razor View 負責頁面骨架
  - Controller 同時提供頁面與 JSON API
- **元件化 UI**
  - 以 `Views/Components/*TagHelper.cs` 封裝共用 UI/行為
- **Oracle 資料層集中**
  - `Helpers/OracleDbHelper` 統一資料查詢邏輯

## 3. 邏輯分層
- Presentation
  - `Views/*`, `Views/Shared/*`, `wwwroot/js/*`, `wwwroot/css/*`
- Application
  - `Controllers/*`（流程控制、Session 檢查、DTO 轉換）
- Data Access
  - `OracleDbHelper` + `OracleConnection/OracleCommand`

## 4. 核心元件
- `AccountController`：登入驗證、Session 寫入/清除
- `MisProgramsController`：MIS 程式清單與建議查詢
- `IdmController`：IDMGD01 CRUD API + popup view
- `HrmGd47Contorller`：HRMGD47 view + submit API
- `LovController`：通用 LOV API（employees, leave-types, departments...）

## 5. 前端架構
- Layout
  - `_Layout.cshtml`：一般頁框
  - `_PopupLayout.cshtml`：popup 專用框（關閉鈕、未存檔離開確認）
- 共用元件
  - `g-datagrid`, `g-dataform`, `g-lov-input`, `g-combobox`, `g-error-message`
  - `g-js`, `g-style` 用於統一載入必要資源
- 行為層
  - Alpine.js 管理頁面狀態
  - `g-components.js` 提供 dialog/toast/LOV 等共用方法

## 6. 錯誤處理架構
- Backend: `Program.cs` 使用 `UseExceptionHandler`
  - API 請求回傳 JSON（`message`, `lineNumber`, `fileName`, `detail`）
  - 非 API 請求導向 `/Home/Error`
- Frontend: `<g-error-message />`
  - 攔截 `window.error`, `unhandledrejection`, `fetch` 非 2xx
  - 顯示可複製錯誤訊息與行號

## 7. Session 與安全
- Session Key：`username`, `password`, `tns`, `user_name`
- 未登入自動導向 Login 或回傳 401
- `Cookie.HttpOnly = true`, `IsEssential = true`
- 目前資料庫連線由 `OracleDbHelper` 建立（實際使用 service account）

## 8. 可擴充性
- 新頁面應優先用 TagHelper 組裝，降低重複碼
- 新 LOV 來源可擴充 `LovController + OracleDbHelper`
- 新 datagrid 能力優先擴充 `GDataGridTagHelper`

## 9. 技術風險
- 部分舊檔存在中文亂碼，需逐步統一 UTF-8
- Oracle SQL 字串拼接錯誤（如 ORA-00933）風險高
- Session 內保存敏感資訊需評估後續加密或替代方案
