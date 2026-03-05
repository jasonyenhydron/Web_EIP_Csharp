# Components Usage

本文件描述目前專案可用的主要 Razor TagHelper（`g-*` 系列）。

## 1. 全域設定

在 [Views/_ViewImports.cshtml](d:/CODE/Web_EIP_Csharp/Views/_ViewImports.cshtml) 已啟用：

```csharp
@addTagHelper *, Web_EIP_Csharp
```

## 2. `<g-style>`

統一載入必要 CSS，避免每頁重複 `<link>`。

```html
<g-style profile="layout"></g-style>
<g-style profile="popup"></g-style>
<g-style profile="mis-programs" local-version="20260303"></g-style>
```

常用屬性：
- `profile`: `layout | popup | mis-programs | none`
- `extras`: 額外 CSS（逗號/分號分隔）
- `local-version`: 版本號（自動加在 query string）

## 3. `<g-js>`

統一載入必要 JS，避免每頁重複 `<script>`。

```html
<g-js profile="popup" include-alpine="true" alpine-version="3.x.x"></g-js>
<g-js profile="mis-programs"></g-js>
```

常用屬性：
- `profile`: `popup | main | mis-programs | none`
- `extras`: 額外 JS
- `include-alpine`: 是否加載 Alpine CDN
- `defer`: 是否加 `defer`

## 4. `<g-datagrid>`

支援查詢、分頁、CRUD、title filter、排序（單欄/多欄）。

```html
<g-datagrid id="idmGrid"
            api="/Idm/select?DataMember=IDMGD01"
            columns="PROGRAM_NO:程式編號:150:left:text::text,DISPLAY_CODE:顯示:80:center:select:Y=Y;N=N:select"
            page-size="20"
            pagination="true"
            allow-add="true"
            allow-update="true"
            allow-delete="true"
            title-filter-enabled="true"
            title-sort-enabled="true"
            sortable-columns="PROGRAM_NO,DISPLAY_CODE"
            multi-sort-enabled="true"
            @@add="openCreate()"
            @@view="viewRecord($event.detail.row)"
            @@edit="editRecord($event.detail.row)"
            @@delete="confirmDelete($event.detail.row)">
</g-datagrid>
```

重點屬性：
- API
  - `api`（建議）
  - `remote-name` + `member-id`（相容舊寫法）
- 欄位
  - `columns="field:title:width:align:editorType:editorOptions:filterType"`
- CRUD
  - `allow-add`, `allow-update`, `allow-delete`
  - `view-command-visible`, `update-command-visible`, `delete-command-visible`
- 篩選/排序
  - `title-filter-enabled`
  - `title-sort-enabled`
  - `sortable-columns`
  - `multi-sort-enabled`

## 5. `<g-dataform>`

唯讀檢視資料（常用於 View 模式 popup）。

```html
<g-dataform id="programView"
            title="程式資料檢視"
            model="formRecord"
            columns="PROGRAM_NO:程式編號,EMPLOYEE_ID:員工編號,PURPOSE:用途"
            horizontal-columns-count="2">
</g-dataform>
```

## 6. `<g-lov-input>`

宣告式 LOV 輸入元件，避免頁面手寫 `openGenericLov()`。

```html
<g-lov-input label="員工"
             hidden-id="employeeId"
             code-id="employeeNo"
             name-id="employeeName"
             lov-title="尋找員工"
             lov-api="/api/lov/hrm/employees"
             lov-columns="編號,名稱,ID"
             lov-fields="employee_no,employee_name,employee_id"
             lov-key-hidden="employee_id"
             lov-key-code="employee_no"
             lov-key-name="employee_name"
             lov-buffer-view="true"
             lov-page-size="50"
             lov-sort-enabled="true">
</g-lov-input>
```

重點屬性：
- 資料映射
  - `lov-columns`, `lov-fields`
  - `lov-key-hidden`, `lov-key-code`, `lov-key-name`
- 行為
  - `lov-buffer-view`, `lov-page-size`, `lov-sort-enabled`
- 相容舊寫法
  - `lov-fn`（直接呼叫既有 JS）

## 7. `<g-combobox>`

可用靜態清單或 SQL 來源。

```html
<g-combobox label="顯示碼"
            name="displayCode"
            x-model="search.displayCode"
            items=",全部,Y:Y,N:N">
</g-combobox>
```

```html
<g-combobox label="部門"
            name="deptId"
            sql="SELECT department_id, department_name FROM cmm_department_v WHERE language_id = 1"
            value-field="DEPARTMENT_ID"
            text-field="DEPARTMENT_NAME">
</g-combobox>
```

## 8. `<g-error-message>`

全域錯誤元件，建議放在 Layout 底部。

```html
<g-error-message />
```

能力：
- 自動攔截前端 JS error / promise rejection
- 攔截 fetch 非 2xx 回應
- 顯示 message、line、source、detail
- 一鍵複製錯誤內容

## 9. 其他常用元件

- `g-button`
- `g-dialog`
- `g-panel`
- `g-tabs` / `g-tab`
- `g-page-title`
- `g-page-header`
- `g-status-bar`
- `g-alert`
- `g-empty-state`
- `g-iframe-modal`
- `g-file-uploader`
- `g-cardview`

## 10. `<g-file-uploader>`

可在 DataForm、DataGrid 或獨立區塊使用，支援三種模式：
- `instantly`: 選檔後立即上傳
- `useButtons`: 選檔後按 Upload 才上傳
- `useForm`: 僅作為原生 `<input type="file">`，由整個 form submit

```html
<g-file-uploader id="docUploader"
                 label="附件上傳"
                 name="files"
                 upload-url="/api/files/upload"
                 upload-mode="useButtons"
                 accept=".pdf,.doc,.docx,.xlsx,.png,.jpg"
                 allowed-file-extensions=".pdf,.doc,.docx,.xlsx,.png,.jpg"
                 multiple="true"
                 max-file-size="10485760"
                 folder="incoming"
                 result-input-id="uploadedFilesJson"
                 result-value-field="relativePath"
                 column-name="ATTACH_PATH"
                 table-id="leaveGrid"
                 on-value-changed="onUploaderValueChanged"
                 on-uploaded="onUploaderUploaded"
                 on-upload-error="onUploaderError">
</g-file-uploader>
<input type="hidden" id="uploadedFilesJson" name="uploadedFilesJson" />
```

常用屬性：
- 基本
  - `id`, `label`, `name`
  - `upload-url`（預設 `/api/files/upload`）
  - `upload-mode`（`instantly | useButtons | useForm`）
- 檔案限制
  - `accept`
  - `allowed-file-extensions`
  - `multiple`
  - `max-file-size` / `min-file-size`（bytes）
- 行為與回呼
  - `folder`（上傳子資料夾）
  - `result-input-id`（回填上傳結果）
  - `result-value-field`（預設 `relativePath`，可改 `storedName` / 其他回傳欄位）
  - `column-name`（上傳後自動回填同列欄位或指定 table 的欄位）
  - `table-id`（配合 `column-name` 更新 `g-datagrid` 的 selectedRow/editRowData）
  - `on-value-changed`
  - `on-uploaded`
  - `on-upload-error`
- 顯示
  - `select-button-text`, `upload-button-text`, `cancel-button-text`, `drop-zone-text`
  - `hint`, `show-file-list`
  - `class`, `extra-class`, `wrapper-class`

## 11. `<g-cardview>`

以卡片方式顯示資料，支援：
- API / 全域變數資料來源
- 搜尋
- 欄位排序（asc/desc）
- 分頁
- 卡片點擊與按鈕事件回呼

```html
<g-cardview id="employeeCards"
            title="員工卡片"
            api="/api/lov/hrm/employees"
            key-field="employee_id"
            title-field="employee_name"
            subtitle-field="employee_no"
            description-field="department_name"
            meta-fields="employee_no,department_name"
            search-fields="employee_no,employee_name,department_name"
            sort-fields="employee_name:姓名,employee_no:編號"
            default-sort-field="employee_name"
            default-sort-dir="asc"
            page-size="8"
            cards-per-row="auto"
            card-min-width="260"
            primary-action-text="查看"
            secondary-action-text="聯絡"
            on-card-click="onEmployeeCardClick"
            on-primary-action="onEmployeeView"
            on-secondary-action="onEmployeeContact">
</g-cardview>
```

常用屬性：
- 資料來源
  - `api`
  - `source-var`（全域 JS 變數，例如 `window.demoCards`）
- 卡片欄位
  - `key-field`
  - `cover-field`, `cover-alt-field`
  - `title-field`, `subtitle-field`, `description-field`
  - `meta-fields`
- 查詢排序
  - `search-fields`
  - `sort-fields="field:label,field:label"`
  - `default-sort-field`, `default-sort-dir`
- 版面
  - `page-size`
  - `cards-per-row="auto|3|4..."`
  - `card-min-width`
- 事件
  - `on-card-click`
  - `on-primary-action`
  - `on-secondary-action`

## 12. 建議使用準則

1. 頁面元件優先：先找 `Views/Components` 是否已有可用元件。  
2. 行為集中：共用行為放 TagHelper 或 `g-components.js`。  
3. 避免重複：CSS/JS 載入一律走 `g-style` / `g-js`。  
4. 新功能先抽象：可重用的需求優先做成元件屬性。  

## 13. 近期更新（2026-03-04）

- 新增 `g-file-uploader` 元件
  - 可在 DataForm / DataGrid / 獨立區塊使用
  - 支援 `instantly`、`useButtons`、`useForm` 三種模式
  - 支援副檔名/檔案大小限制、拖拉上傳、回呼函式
  - 後端新增 `POST /api/files/upload`
- 新增 `g-cardview` 元件
  - 參考 DevExpress CardView 的卡片展示互動模式
  - 支援搜尋、排序、分頁、卡片操作按鈕與回呼

`g-datagrid` 已更新以下行為：

- 可捲動區域
  - 表格主體使用 `.g-grid-body`。
  - 預設樣式包含 `max-height: var(--g-grid-body-max-height, 52vh)` 與 `overflow-y-auto`，避免出現「有捲軸但無法捲動」。
- 分頁筆數位置
  - `共 X 筆` 顯示在分頁切換區（上一頁/頁碼/下一頁）左側。
  - 工具列上方的筆數僅在 `pagination="false"` 時顯示，避免重複資訊。
- 自訂高度方式
  - 可在外層容器覆寫 CSS 變數，例如：

```html
<div style="--g-grid-body-max-height: 60vh;">
  <g-datagrid ...></g-datagrid>
</div>
```
