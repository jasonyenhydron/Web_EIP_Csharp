# web_eip_csharp_development

## Purpose
此 skill 用於本專案日常開發、畫面調整、TagHelper 擴充與 CRUD 修正。

適用情境：
- ASP.NET Core MVC (`net10.0`)
- Razor TagHelper 元件式頁面
- 前端組合：`Alpine.js + Tailwind + Flowbite + HTMX`
- Datagrid / DataForm / LOV 為主的企業內部系統畫面
- RESTful API 串接
- Dapper + `DbHelper` 系列資料庫存取
- 支援 Oracle / MariaDB / SQL Server / SQLite

核心目標：
- 優先重用既有元件，不在頁面散落重複 HTML / JS
- 讓 `g-datagrid` 與 `g-dataform` 成為主要 CRUD 載體
- 維持頁面簡單明瞭，必要時加入繁體中文說明備註

## Project Rules (must follow)
1. 優先使用 `Views/Components` 內既有 TagHelper。
2. 無法用既有元件完成時，才補頁面專屬 HTML / JS。
3. 共用資源載入優先使用 `<g-js>` / `<g-style>`，避免每頁重複貼 `<script>` / `<link>`。
4. Datagrid 行為優先擴充 `Views/Components/GDataGridTagHelper.cs`，避免查詢、排序、CRUD 邏輯散落頁面。
5. 單筆資料維護優先使用 `Views/Components/GDataFormTagHelper.cs`。
6. Grid 編輯或新增時，優先透過 `g-dataform` 或 `g-datagrid` 內建 generated form 處理，不要在 grid row 內手寫 CRUD 邏輯。
7. 錯誤顯示優先走 `<g-error-message />` 或元件內既有 toast / error flow；API 例外格式以 `Program.cs` exception handler 為準。
8. 資料庫存取優先使用 `DbHelper` / `OracleDbHelper` / `MariaDbHelper` / `SqlserverDbHelper` / `SqliteDbHelper`，避免直接 new connection。
9. 頁面路由優先使用 `Area` 與既有頁面配置，不要任意新增風格不一致的 Controller 路徑。
10. Razor 頁面維持薄層，複雜互動優先回收進 TagHelper 或共用 JS。
11. 修改時優先保持可編譯、功能正確，再做結構整理；歷史亂碼檔案不要順手大改無關內容。

## Key Files
- `Views/Components/GDataGridTagHelper.cs`
- `Views/Components/GDataFormTagHelper.cs`
- `Models/DataForm/FormColumnModels.cs`
- `Views/Shared/_Layout.cshtml`
- `Views/Shared/_PopupLayout.cshtml`
- `Views/Shared/_LovModal.cshtml`
- `Helpers/OracleDbHelper.cs`
- `Helpers/MariaDbHelper.cs`
- `Helpers/SqlserverDbHelper.cs`
- `Helpers/SqliteDbHelper.cs`
- `Program.cs`

參考頁面：
- `Views/MisPrograms/IDMGD01.cshtml`
- `Views/MisPrograms/HRMGD47.cshtml`
- `Views/MisPrograms/CMMGD16.cshtml`
- `Views/MisPrograms/PRMPQ11.cshtml`

## Component Strategy

### 何時用 `g-datagrid`
適合：
- 清單查詢
- 分頁、排序、欄位過濾
- 列點選、雙擊、選取事件
- 內建 CRUD 指令或 generated form
- 需要 `query-columns` + `form-columns` 一次定義查詢與編輯欄位

優先屬性：
- `api`
- `columns`
- `query-columns`
- `form-columns`
- `edit-mode`
- `allow-add` / `allow-update` / `allow-delete`
- `on-row-click` / `on-load-success` / `on-inserted` / `on-updated` / `on-deleted`
- `title-sort-enabled` / `multi-sort-enabled`
- `buffer-view`
- `record-lock`

### 何時用 `g-dataform`
適合：
- 單筆資料維護
- 有查詢條件面板，但主要工作是編輯單筆資料
- 表單驗證、欄位比較、預設值、主鍵欄位
- `select` / `lov` / `textarea` / `date` / `datetime-local` / `number`
- 需要 `continue-add`、`show-apply-button`、`query-api`

優先屬性：
- `api`
- `query-api`
- `tool-items`
- `on-load-success` / `on-applied` / `on-query-loaded`
- `duplicate-check`
- `validate-style`
- `empty-string-as-null`
- `chain-dataform-id`
- `parent-object-id`
- `relation-columns`

## GDataGrid Current Capability Summary

### 基本能力
`GDataGridTagHelper` 目前已具備：
- API 資料載入
- 分頁與 page list
- row number / stripe
- row click / row dblclick
- title filter / query panel
- title sort / multi sort
- generated form dialog
- HTMX 表單送出
- toast 顯示
- query-column / form-column JSON 宣告
- CRUD hooks：`OnInsert` / `OnInserted` / `OnUpdate` / `OnUpdated` / `OnDelete` / `OnDeleted` / `OnDeleting` / `OnView`
- 查詢後自動套用 `AutoApply`
- buffer view / record lock / duplicate check 等參數

### 常用屬性群組

#### 1. 資料載入
- `api`
- `member-id`
- `id-field` / `IDField`
- `page-size`
- `pagination`
- `page-list`
- `not-init-grid`

#### 2. 畫面與互動
- `title`
- `extra-class`
- `clean-style`
- `row-numbers`
- `striped`
- `show-filter`
- `title-filter-enabled`
- `toolbar-html`

#### 3. CRUD 控制
- `allow-add`
- `allow-update`
- `allow-delete`
- `view-command-visible`
- `update-command-visible`
- `delete-command-visible`
- `edit-mode`
- `always-close`
- `auto-apply`
- `duplicate-check`

#### 4. 查詢 / 編輯定義
- `query-columns`
- `form-columns`
- `query-auto-column`
- `query-title`
- `query-mode`

#### 5. 排序 / 多選 / 進階控制
- `title-sort-enabled`
- `title-sort-field`
- `sortable-columns`
- `multi-sort-enabled`
- `multi-select`
- `check-on-select`
- `buffer-view`
- `record-lock`
- `record-lock-mode`
- `parent-object-id`
- `relation-columns`

### `query-columns` 可用欄位
目前可用設定包含：
- `FieldName`
- `Caption`
- `Condition`
- `AndOr`
- `DataType`
- `DefaultMethod`
- `DefaultValue`
- `Editor`
- `EditorOptions`
- `Format`
- `IsNvarChar`
- `NewLine`
- `RemoteMethod`
- `RowSpan`
- `Span`
- `TableName`
- `Width`
- `Api`
- `LovTitle`
- `LovApi`
- `LovColumns`
- `LovFields`
- `LovKeyValue`
- `LovKeyDisplay`
- `LovDisplayFormat`

常見 `Editor`：
- `text` / `gtext`
- `readonly`
- `date` / `datebox` / `g-datebox`
- `checkbox` / `g-checkbox`
- `select` / `gcombobox` / `infocombobox`
- `lov` / `lovinput` / `g-lov-input`

### `form-columns` 可用欄位
目前可用設定包含：
- `FieldName`
- `Caption`
- `ColumnType`
- `ColSpan`
- `AlwaysReadOnly`
- `Required`
- `IsPrimaryKey`
- `Hidden`
- `DefaultValue`
- `Placeholder`
- `MaxLength`
- `Options`
- `OptionsApi`
- `LovTitle`
- `LovApi`
- `LovColumns`
- `LovFields`
- `LovKeyValue`
- `LovKeyDisplay`
- `LovDisplayFormat`

常見 `ColumnType`：
- `text` / `gtext` / `g-textbox`
- `textarea` / `g-textarea`
- `number` / `g-numberbox`
- `date` / `g-datebox`
- `datetime-local` / `g-datetimebox`
- `select` / `gcombobox`
- `checkbox`
- `radio` / `g-radiogroup`
- `lov` / `lovinput` / `g-lov-input`
- `readonly` / `g-readonly`
- `hidden` / `g-hidden`

### 實作建議
1. 若頁面只是單純查詢 + CRUD，優先直接在頁面填 `query-columns`、`form-columns`。
2. 若同類型 grid 反覆出現相同互動，回頭擴充 `GDataGridTagHelper`，不要在頁面重複貼 script。
3. `on-row-click`、`on-load-success` 等 callback 盡量註冊到 `window.gDataGridRuntime` 對應 app callback。
4. 刪除動作若需客製確認訊息，可沿用 `@@delete` 綁定模式。
5. 若要 generated form，優先補 `form-columns`，不要額外手刻 modal。

## GDataForm Current Capability Summary

### 基本能力
`GDataFormTagHelper` 目前已具備：
- 查詢條件面板 `query-column`
- 單筆資料 view / add / edit / delete
- modal 編輯視窗
- 欄位驗證
- 欄位比較驗證
- 預設值
- 主鍵唯讀
- `Options` / `OptionsApi` 下拉選項
- LOV 欄位與格式化顯示
- Toast / status message / callback hooks
- `continue-add` / `show-apply-button`
- `empty-string-as-null`
- chain / parent / relation 設定

### 表單層級屬性
- `id`
- `title`
- `api`
- `query-api`
- `member-id`
- `horizontal-columns-count`
- `caption-alignment`
- `extra-class`
- `always-read-only`
- `continue-add`
- `is-auto-page-close`
- `duplicate-check`
- `validate-style`
- `show-apply-button`
- `not-init-load`
- `chain-dataform-id`
- `parent-object-id`
- `relation-columns`
- `tool-items`
- `query-title`
- `on-load-success`
- `on-apply`
- `on-applied`
- `on-cancel`
- `on-before-validate`
- `on-query-loaded`
- `status-target-id`
- `saving-message`
- `validate-failed-message`
- `save-success-message`
- `save-error-message`
- `success-toast-message`
- `error-toast-message`
- `exception-toast-message`
- `empty-string-as-null`

### `form-column` 可用欄位
以 `Models/DataForm/FormColumnModels.cs` 為準，常用屬性：
- `field-name`
- `caption`
- `column-type`
- `caption-alignment`
- `col-span`
- `always-read-only`
- `required`
- `is-primary-key`
- `hidden`
- `default-value`
- `placeholder`
- `max-length`
- `min`
- `max`
- `options`
- `options-api`
- `lov-title`
- `lov-api`
- `lov-columns`
- `lov-fields`
- `lov-key-value`
- `lov-key-display`
- `lov-display-format`
- `lov-on-confirm`
- `validate-fn`
- `validate-message`
- `compare-field`
- `compare-mode`
- `value-type`
- `on-change`

### `query-column` 可用欄位
- `field-name`
- `query-field`
- `caption`
- `editor`
- `span`
- `default-value`
- `placeholder`
- `options`
- `lov-title`
- `lov-api`
- `lov-columns`
- `lov-fields`
- `lov-key-value`
- `lov-key-display`
- `lov-display-format`
- `readonly`

### `FormColumnType`
目前模型列舉支援：
- `Text`
- `Number`
- `Date`
- `DateTimeLocal`
- `Select`
- `Checkbox`
- `Radio`
- `Lov`
- `Readonly`
- `Textarea`
- `Hidden`

頁面可用別名實際上也接受：
- `g-textbox`
- `g-numberbox`
- `g-datebox`
- `g-datetimebox`
- `g-radiogroup`
- `g-lov-input`
- `g-readonly`
- `g-hidden`

### 實作建議
1. 單筆主畫面優先選 `g-dataform`，不要為了單筆編輯硬套 `g-datagrid`。
2. 有跨欄位驗證時，優先用 `compare-field` + `compare-mode` + `validate-message`。
3. Select 若資料來自 API，優先用 `options-api`，不要在頁面自行 fetch 後組 DOM。
4. LOV 欄位若需要顯示文字欄位，記得搭配 hidden/display 欄位成對設計。
5. 狀態列訊息優先透過 `status-target-id` 與 callback 更新。

## Common Workflows

### A) 新增或修正 Datagrid 功能
1. 先確認是頁面參數不足，還是 `GDataGridTagHelper` 缺能力。
2. 若屬於共用功能，先改 `Views/Components/GDataGridTagHelper.cs`。
3. 頁面只保留 `query-columns`、`form-columns`、callback 名稱與必要屬性。
4. 確認事件有綁定：
   - `@add`
   - `@view`
   - `@edit`
   - `@delete`
   - `@query`
5. 驗證查詢、點列、排序、CRUD 是否正常。
6. 最後執行 `dotnet build`。

### B) 新增或修正 DataForm 功能
1. 先確認是否已能用 `form-column` / `query-column` 屬性達成。
2. 若為共用表單能力，修改 `Views/Components/GDataFormTagHelper.cs` 或 `Models/DataForm/FormColumnModels.cs`。
3. 若只是單頁規則，使用 `on-load-success`、`on-applied`、`on-before-validate` 等 callback。
4. 驗證 add / edit / delete / query 流程至少各一次。
5. 確認驗證訊息、toast、status bar 與 auto close 行為。

### C) LOV 欄位需求
1. 優先使用 `g-dataform` / `g-datagrid` 內建 LOV 欄位設定。
2. 先補：
   - `lov-api`
   - `lov-columns`
   - `lov-fields`
   - `lov-key-value`
   - `lov-key-display`
   - `lov-display-format`
3. 若還缺資料來源，再補對應 controller / helper。
4. 顯示欄位與實際值欄位要分清楚，避免只存 display text。

### D) 共用資源載入
1. 樣式先用 `<g-style profile="...">`
2. Script 先用 `<g-js profile="...">`
3. Datagrid 頁面通常要確認：
   - `include-alpine="true"`
   - `include-htmx="true"`
4. 避免同一頁重複載入 Alpine / HTMX / Flowbite。

### E) API 與錯誤處理
1. API 回傳格式與錯誤格式遵循 `Program.cs` 例外處理。
2. 前端錯誤優先接既有 toast / `g-error-message` 流程。
3. 若要顯示詳細錯誤，可補：
   - `message`
   - `lineNumber`
   - `fileName`
   - `detail`
4. SQL 動態拼接時，優先檢查 bind 參數與 SQL 語法，避免 `ORA-00933`。

## Page Implementation Guidelines

### Razor 頁面應保持精簡
建議保留：
- 元件宣告
- API 路徑
- 欄位 JSON 或 child tags
- 少量頁面專屬 callback

不建議：
- 大量手寫 CRUD fetch
- 在每頁複製 modal HTML
- 把共用查詢條件、排序、錯誤處理重寫一次

### JavaScript 寫法
優先：
- 用 Alpine component 包住頁面局部狀態
- callback 名稱交給 TagHelper 屬性綁定
- 共用邏輯掛進 runtime 或元件內

避免：
- 在全域直接堆大量匿名函式
- 用 jQuery 風格直接操作 generated DOM 取代既有資料流

## Validation Checklist
每次修改至少確認：
1. `dotnet build`
2. 頁面開啟後 Console 無 Alpine / HTMX / JS error
3. CRUD 或查詢流程至少實際跑一次
4. LOV 開窗、選值、回填正常
5. 錯誤訊息或 toast 顯示正常
6. 若有排序 / 分頁 / title filter，至少各測一次

## Example Decision Rules
- 要做查詢清單 + 列編輯：先選 `g-datagrid`
- 要做單筆主檔維護：先選 `g-dataform`
- 要做查詢條件 popup LOV：先用內建 `lov-*` 屬性
- 要做下拉選單且資料來自 API：先用 `options-api`
- 要做跨欄位驗證：先用 `compare-field` / `compare-mode`
- 要讓 Grid CRUD 共用新行為：先改 `GDataGridTagHelper`
- 要讓所有表單欄位型別都能用：先改 `GDataFormTagHelper` + `FormColumnModels`

## Notes
- 本專案部分歷史檔案仍有亂碼，修檔時以可編譯、可執行、功能正確為優先。
- 若只為了修一頁，不要順手重寫整批歷史頁面。
- 若遇到元件已具備能力，優先調整宣告參數，不要直接繞過元件。
- 文件與註解請使用繁體中文，描述以簡單明瞭為主。
