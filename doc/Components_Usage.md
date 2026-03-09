# Components Usage

本文件說明目前專案可用的主要 TagHelper 與建議用法。

## 1. 基本設定
在 `Views/_ViewImports.cshtml` 加入：

```csharp
@addTagHelper *, Web_EIP_Csharp
```

## 2. `<g-style>`
載入樣式。

```html
<g-style profile="layout"></g-style>
<g-style profile="popup"></g-style>
<g-style profile="mis-programs"></g-style>
```

常用屬性：
- `profile`: `layout | popup | mis-programs | none`
- `extras`: 額外 CSS 路徑
- `local-version`: 本地快取版本字串

## 3. `<g-js>`
載入腳本。

```html
<g-js profile="main"></g-js>
<g-js profile="popup" include-alpine="true"></g-js>
```

常用屬性：
- `profile`: `main | popup | datagrid | mis-programs | none`
- `extras`: 額外 JS 路徑
- `include-alpine`: 是否自動載入 Alpine.js
- `defer`: script defer

## 4. `<g-datagrid>`
支援查詢、排序、篩選、分頁、CRUD 事件。

```html
<g-datagrid id="idmGrid"
            api="/Idm/select?DataMember=IDMGD01"
            columns="PROGRAM_NO:程式編號:150:left:text::text,DISPLAY_CODE:顯示:80:center:select:Y=Y;N=N:select"
            page-size="20"
            pagination="true"
            title-filter-enabled="true"
            title-sort-enabled="true"
            sortable-columns="PROGRAM_NO,DISPLAY_CODE"
            multi-sort-enabled="true">
</g-datagrid>
```

## 5. `<g-dataform>`（新版）
採子標籤 `<form-column>` 定義欄位。

```html
<g-dataform id="leaveForm"
            title="請假申請"
            api="/api/leave"
            member-id="HRMGD47"
            horizontal-columns-count="2"
            validate-style="Hint"
            show-apply-button="true">

  <form-column field-name="leave_id" caption="假單號" column-type="Readonly" is-primary-key="true" />
  <form-column field-name="leave_type" caption="假別" column-type="Select" required="true" options-api="/api/lov/hrm/leave-types" />
  <form-column field-name="start_date" caption="開始日" column-type="Date" required="true" />
</g-dataform>
```

說明：
- Runtime 由 `g-dataform.js` 提供（TagHelper 自動注入一次）。
- `member-id` 為正式屬性；舊 `data-member` 僅作相容讀取。

## 6. `<form-column>`
`g-dataform` 子元素，定義欄位型別、驗證、選項、LOV。

常用屬性：
- `field-name`, `caption`, `column-type`
- `required`, `is-primary-key`, `always-read-only`, `hidden`
- `default-value`, `placeholder`, `max-length`, `min`, `max`
- `options`, `options-api`
- `lov-api`, `lov-columns`, `lov-fields`, `lov-key-value`, `lov-key-display`
- `validate-fn`, `validate-message`, `on-change`

## 7. `<g-lov-input>`
LOV 輸入控制項（hidden/code/name/button）。

```html
<g-lov-input label="員工"
             lov-api="/api/lov/hrm/employees"
             lov-columns="編號,姓名,ID"
             lov-fields="employee_no,employee_name,employee_id"
             lov-key-hidden="employee_id"
             lov-key-code="employee_no"
             lov-key-name="employee_name"
             selectonly="false" />
```

目前規格：
- `selectonly=false` 時，**只按 Enter 查詢**。
- Modal 由 `/js/g-lov-modal-runtime.js` 注入，不再使用 `_LovModal.cshtml`。

## 8. `<g-combobox>`
靜態或 SQL 來源下拉。

## 9. `<g-error-message>`
全域前端錯誤顯示元件。

## 10. 近期修正重點
- LOV 確認/雙擊後不再因回填事件造成 popup 重新開啟。
- Sidebar 收合按鈕修正（防重複初始化 + collapsed 寬度規則）。
