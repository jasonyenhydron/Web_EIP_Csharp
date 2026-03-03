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

## 10. 建議使用準則

1. 頁面元件優先：先找 `Views/Components` 是否已有可用元件。  
2. 行為集中：共用行為放 TagHelper 或 `g-components.js`。  
3. 避免重複：CSS/JS 載入一律走 `g-style` / `g-js`。  
4. 新功能先抽象：可重用的需求優先做成元件屬性。  
