# SD - Software Design

## 1. 模組設計
- Account：登入/登出/Session
- MIS Programs：程式查詢、建議、明細
- IDM（IDMGD01）：Datagrid + CRUD API
- HRM（HRMGD47）：表單提交 + LOV 輔助
- LOV API：通用查詢端點

## 2. 前端元件設計

### 2.1 GDataGridTagHelper
- 輸入：`api`、`columns`、排序/篩選/分頁與 CRUD 屬性
- 輸出：table + Alpine runtime script
- 重點：
  - Header filter
  - 單欄/多欄排序
  - row/form 模式操作
  - `g-grid-body` 高度可由 `--g-grid-body-max-height` 控制

### 2.2 GDataFormTagHelper（新版）
- 子標籤：`<form-column ... />`
- 屬性：`api`, `member-id`, `validate-style`, `relation-columns`, callbacks
- 輸出：
  - 工具列
  - View 模式顯示區
  - Add/Edit Modal
  - Delete Confirm Modal
- Runtime：`g-dataform.js`

### 2.3 GLovInputTagHelper
- 輸出：`hidden + code + name + button`
- 行為：
  - `selectonly=true`：唯讀，由 LOV 選取
  - `selectonly=false`：僅 Enter 觸發查詢
- Runtime：`g-lov-modal-runtime.js`（每頁自動注入一次）

## 3. 關鍵互動流程

### 3.1 LOV 選取流程
1. 點按 LOV 按鈕或 Enter 觸發
2. 開啟 runtime modal 並查詢資料
3. 雙擊列或按確定
4. 回填目標欄位 + callback
5. 關閉 modal

備註：已加入 suppress 機制避免回填後再次自動開窗。

### 3.2 DataForm 提交流程
1. `submitForm()`
2. `onBeforeValidate` callback
3. 欄位驗證（Hint/Dialog）
4. optional duplicate check
5. POST/PUT API
6. 成功後 `onApplied` callback 與 UI 狀態切換

## 4. 重要實作規格
- TagHelper 不可直接綁定 `data-*` 作為屬性名稱：`g-dataform` 使用 `member-id`。
- LOV 舊 partial `_LovModal.cshtml` 已移除。
- Sidebar 收合狀態儲存於 `localStorage.sidebarCollapsed`。

## 5. 近期變更摘要
- 新增 DataForm 新版（TagHelper + form-column + runtime js）。
- 修正 LOV popup 關閉後重開問題。
- `g-lov-input` 改為 Enter-only 查詢。
- 修正 sidebar `×` 收合偶發無效問題。
