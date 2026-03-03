# SD - Software Design

## 1. 設計範圍
本文件描述目前已落地的模組設計：
- 登入與 Session
- MIS 程式清單頁
- IDM `IDMGD01` datagrid + form CRUD
- HRM `HRMGD47` 請假示例
- LOV Modal / ErrorMessage / JS/CSS 載入 TagHelper

## 2. 模組設計

### 2.1 Account
- `GET /Account/Login`：顯示登入頁
- `POST /Account/Login`：
  - 呼叫 `OracleDbHelper.ValidateUserLoginAsync`
  - 成功後寫入 Session
- `GET /Account/Logout`：清除 Session

### 2.2 MIS Programs
- `GET /mis/programs`
  - 依 `program_no/employee_id/display_code` 查詢 `idm_program_v`
  - 分類資料供樹狀 UI 呈現
- `GET /api/mis/programs/suggestions`
  - 程式代號自動完成
- `GET /api/mis/programs/{program_no}`
  - 取得程式明細

### 2.3 IDM IDMGD01
- View：`Views/MisPrograms/IDMGD01.cshtml`（popup layout）
- API：
  - `GET /Idm/select?DataMember=IDMGD01`
  - `POST /Idm/insert?DataMember=IDMGD01`
  - `POST /Idm/update?DataMember=IDMGD01`
  - `POST /Idm/delete?DataMember=IDMGD01`
- UI：
  - `g-datagrid` 作為主表
  - `g-dialog` + 表單欄位做 view/edit/create

### 2.4 HRM HRMGD47
- View：`Views/MisPrograms/HRMGD47.cshtml`
- API：
  - `POST /Hrm/submit`（新增請假申請）
- 使用 `g-lov-input` 查詢員工、假別、部門

### 2.5 LOV API
- `GET /api/lov/hrm/leave-types`
- `GET /api/lov/hrm/employees`
- `GET /api/lov/cmm/departments`
- `GET /api/lov/hrm/booking-departments`
- 支援分頁參數：`page`, `pageSize`

## 3. TagHelper 設計重點

### 3.1 `g-datagrid`
- 主要屬性：
  - `api`（新）或 `remote-name + member-id`（相容）
  - `columns`, `page-size`, `pagination`
  - `allow-add`, `allow-update`, `allow-delete`
  - `title-filter-enabled`, `title-sort-enabled`
  - `sortable-columns`, `multi-sort-enabled`
- 事件：
  - `@add`, `@edit`, `@view`, `@delete`, `@query`
- 內建能力：
  - header filter popup
  - 單欄/多欄排序
  - CRUD 按鈕欄位

### 3.2 `g-dataform`
- 用於唯讀檢視
- 以 `columns="FIELD:Label,..."` 決定欄位
- `horizontal-columns-count` 控制排版

### 3.3 `g-lov-input`
- 支援 declarative 設定，不需頁面手寫 `openGenericLov`
- 重點屬性：
  - `lov-api`, `lov-columns`, `lov-fields`
  - `lov-key-hidden/code/name`
  - `lov-buffer-view`, `lov-page-size`, `lov-sort-enabled`

### 3.4 `g-combobox`
- 支援兩種來源：
  - `items="Y:Y,N:N"`（靜態）
  - `sql="SELECT ..."`（Oracle 動態）

### 3.5 `g-js` / `g-style`
- 用 profile 載入必要資源，避免每頁重複 script/link
- 可用 `extras` 加載頁面額外檔案

### 3.6 `g-error-message`
- 全域錯誤彈窗
- 自動攔截 API/JS 錯誤並展示行號、來源、詳細堆疊

## 4. 資料庫設計摘要
- `idm_program`, `idm_program_v`
- `hrm_em_ask_for_leave`
- `hrm_employee_v`, `hrm_leave_l`, `cmm_department_v`, `hrm_booking_department_v`

## 5. 介面事件流程（IDMGD01）
1. 頁面 init 後呼叫 datagrid `fetchData`
2. 點選 `新增/編輯/檢視` 開啟 `g-dialog`
3. 儲存時送 `insert/update`
4. 成功後關閉 dialog 並重新查詢
5. 刪除先確認，再送 `delete`，最後刷新表格

## 6. 例外與回應格式
- API 錯誤統一：
  - `status = "error"`
  - `message`
  - `lineNumber`, `fileName`, `detail`（全域 handler）

## 7. 開發規範（落地）
- 優先使用 `Views/Components` 既有元件
- 不重覆手寫共用 JS/CSS 載入，改用 `g-js/g-style`
- 新增 UI 功能先評估是否抽到 TagHelper
- 所有 CRUD API 應維持 JSON 一致格式
