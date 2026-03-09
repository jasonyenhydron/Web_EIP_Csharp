# SA - Software Architecture

## 1. 系統目標
Web EIP 提供 MIS/IDM/HRM 的統一入口，透過 MVC + API + TagHelper 組成可重用前端元件。

## 2. 架構風格
- Server-side MVC + JSON API
- TagHelper 驅動 UI 元件
- Oracle 為主要資料來源
- Alpine.js 管理前端互動狀態

## 3. 分層
- Presentation：`Views/*`, `Views/Shared/*`, `wwwroot/js/*`, `wwwroot/css/*`
- Application：`Controllers/*`
- Data Access：`Helpers/OracleDbHelper.cs`

## 4. 核心元件
- `g-datagrid`：列表、排序、篩選、分頁、CRUD 事件
- `g-dataform` + `form-column`：表單 CRUD、驗證、Master-Detail、LOV
- `g-lov-input`：LOV 欄位輸入
- `g-js` / `g-style`：資源載入聚合

## 5. LOV 架構（現行）
- Modal/JS Runtime：`wwwroot/js/g-lov-modal-runtime.js`
- HTML 輸出與設定組裝：`GLovInputTagHelper`
- 注入方式：每頁第一次使用 `g-lov-input` 時自動注入 runtime
- 舊 partial：`_LovModal.cshtml` 已移除

## 6. DataForm 架構（現行）
- 後端：`GDataFormTagHelper` + `FormColumnTagHelper`
- 模型：`Models/DataForm/FormColumnModels.cs`
- 前端：`wwwroot/js/g-dataform.js`
- 支援：
  - View/Add/Edit/Delete
  - Flowbite Modal
  - Required + 自訂驗證函式
  - Select 靜態/動態選項
  - LOV 欄位
  - Master-Detail relation

## 7. 穩定性修正（近期）
- LOV 選取後回填造成 reopen 已修正（抑制回填觸發自動查詢）。
- Sidebar 收合失效已修正（重複綁定防護與寬度覆蓋修正）。

## 8. 安全與控制
- Session 驗證與逾時控制
- API 錯誤統一 JSON 回應
- 前端錯誤顯示由 `g-error-message` 及既有錯誤流程處理
