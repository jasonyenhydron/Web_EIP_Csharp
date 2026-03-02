# EIP 自訂元件使用手冊
# Views/Components — Tag Helper 元件庫

> 對應 jeasyui 功能，採用 Tailwind CSS + Alpine.js 實作，副檔名為 `.cs`（ASP.NET Core Tag Helper）

---

## 📦 引用設定

`_ViewImports.cshtml` 已自動加入（無需其他設定）：

```csharp
@addTagHelper *, Web_EIP_Csharp
```

JS 工具函數引用（加至 `_Layout.cshtml` 或個別 `@section Scripts`）：

```html
<script src="~/js/eip-components.js" asp-append-version="true"></script>
```

---

## 1. `<eip-panel>` — 面板 (jeasyui Panel)

**檔案**：`EipPanelTagHelper.cs`

```html
<eip-panel title="基本資訊" icon="info" collapsible="true">
    <p>面板內容...</p>
</eip-panel>
```

| 屬性 | 說明 | 預設 |
|------|------|------|
| `title` | 標題文字 | `""` |
| `icon` | info\|edit\|calendar\|user\|filter\|clock\|setting\|code | `""` |
| `collapsible` | 是否顯示收合按鈕 | `false` |
| `collapsed` | 預設收合 | `false` |
| `class` | 額外 CSS | `""` |

**JS 控制**：`eipPanelToggle(panelId)` — 但元件已自動處理

---

## 2. `<eip-tabs>` + `<eip-tab>` — 分頁標籤 (jeasyui Tabs)

**檔案**：`EipTabsTagHelper.cs`

```html
<eip-tabs active-tab="0">
    <eip-tab title="程式清單" icon="code">
        <p>清單內容...</p>
    </eip-tab>
    <eip-tab title="程式詳細" icon="info">
        <p>詳細內容...</p>
    </eip-tab>
    <eip-tab title="程式維護" icon="edit">
        <p>編輯內容...</p>
    </eip-tab>
</eip-tabs>
```

> ⚠️ 使用 Alpine.js `x-data` 管理 `active` 狀態，`<eip-tab>` 必須為 `<eip-tabs>` 的直接子元素

| 屬性（eip-tabs） | 說明 | 預設 |
|------|------|------|
| `active-tab` | 預設顯示第幾 tab（0-based） | `0` |

| 屬性（eip-tab） | 說明 |
|------|------|
| `title` | Tab 標籤文字 |
| `icon` | info\|edit\|calendar\|user... |

---

## 3. `<eip-dialog>` — 對話框 (jeasyui Dialog)

**檔案**：`EipDialogTagHelper.cs`

```html
<eip-dialog id="deleteDlg" title="確認刪除" width="sm">
    <p class="text-slate-700">確定要刪除這筆資料嗎？</p>
    <div class="flex justify-end gap-2 mt-5">
        <eip-button text="確認刪除" type="danger"    icon="trash" onclick="doDelete(); eipDialogClose('deleteDlg')"/>
        <eip-button text="取消"     type="secondary" icon="close" onclick="eipDialogClose('deleteDlg')"/>
    </div>
</eip-dialog>

<!-- 觸發按鈕 -->
<eip-button text="刪除" type="danger" icon="trash" onclick="eipDialogOpen('deleteDlg')"/>
```

| 屬性 | 說明 | 值 |
|------|------|-----|
| `id` | 對話框 ID（必填） | string |
| `title` | 標題 | string |
| `width` | 寬度 | `sm`\|`md`\|`lg`\|`xl`\|`full` |
| `close-btn` | 右上角關閉按鈕 | `true`\|`false` |
| `backdrop-close` | 點背景關閉 | `true`\|`false` |

**JS**：`eipDialogOpen('id')` / `eipDialogClose('id')`

---

## 4. `<eip-button>` — 按鈕 (jeasyui LinkButton)

**檔案**：`EipButtonTagHelper.cs`

```html
<eip-button text="儲存"   type="primary"   icon="save"    onclick="save()"/>
<eip-button text="刪除"   type="danger"    icon="trash"   onclick="del()"/>
<eip-button text="新增"   type="success"   icon="plus"    onclick="add()"/>
<eip-button text="查詢"   type="info"      icon="search"  submit="true"/>
<eip-button text="取消"   type="secondary" icon="close"   onclick="cancel()"/>
<eip-button text="重置"   type="ghost"     icon="refresh" onclick="reset()"/>
<eip-button text="匯出"   type="warning"   icon="download"/>
<eip-button text="處理中" type="primary"   disabled="true"/>
```

| 屬性 | 說明 | 值 |
|------|------|-----|
| `type` | 樣式 | `primary`\|`secondary`\|`danger`\|`warning`\|`success`\|`info`\|`ghost` |
| `icon` | save\|trash\|edit\|search\|plus\|close\|check\|refresh\|upload\|download\|print | |
| `size` | 大小 | `sm`\|`md`\|`lg` |
| `submit` | type="submit" | `true`\|`false` |
| `disabled` | 禁用 | `true`\|`false` |
| `id` | 按鈕 ID | string |
| `onclick` | JS 事件 | string |

---

## 5. `<eip-form-group>` — 表單欄位群組 (jeasyui Form)

**檔案**：`EipFormGroupTagHelper.cs`

```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
    <eip-form-group label="員工編號" required="true">
        <input type="text" name="emp_no" class="eip-input" x-model="form.empNo">
    </eip-form-group>

    <eip-form-group label="請假類別" required="true" col-span="2">
        <select name="leave_type" class="eip-input">
            <option value="">請選擇...</option>
        </select>
    </eip-form-group>

    <eip-form-group label="備註" help="最多 200 字" col-span="4">
        <textarea name="remark" rows="3" class="eip-input"></textarea>
    </eip-form-group>
</div>
```

| 屬性 | 說明 |
|------|------|
| `label` | 欄位標籤 |
| `required` | 顯示紅色 * |
| `help` | 提示文字（灰色小字） |
| `error` | 錯誤訊息（紅色，可綁定 Alpine） |
| `col-span` | Grid 欄寬：1\|2\|3\|4 |

**CSS class `eip-input`**（建議加至 `site.css`）：

```css
.eip-input {
    width: 100%;
    padding: 0.375rem 0.75rem;
    font-size: 0.875rem;
    border: 1px solid #cbd5e1;
    border-radius: 0.5rem;
    background: white;
    transition: all 0.15s;
}
.eip-input:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59,130,246,0.15);
}
```

---

## 6. `<eip-search-box>` — 搜尋下拉 (jeasyui ComboBox)

**檔案**：`EipSearchBoxTagHelper.cs`

```html
<!-- 員工查詢 -->
<input type="hidden" id="hidEmpId" name="employee_id">
<eip-search-box
    id="empSearch"
    name="employee_no"
    placeholder="輸入員工編號或姓名..."
    api-url="/api/lov/hrm/employees"
    display-field="employee_no"
    value-field="employee_id"
    label-fields="employee_no,employee_name"
    target-id="hidEmpId"/>
```

| 屬性 | 說明 |
|------|------|
| `api-url` | 查詢 API（需接受 `?query=` 參數） |
| `display-field` | 輸入框顯示欄位（如 `employee_no`） |
| `value-field` | 實際值欄位（寫入 `target-id`） |
| `label-fields` | 下拉顯示欄位（逗號分隔，第一個為主） |
| `target-id` | 儲存實際值的 hidden input ID |

---

## 7. `<eip-pagination>` — 分頁 (jeasyui Pagination)

**檔案**：`EipPaginationTagHelper.cs`

```html
<!-- 搭配 Alpine.js 使用（寫在 x-data 範圍內） -->
<eip-pagination
    alpine-total="totalPages"
    alpine-current="currentPage"
    alpine-prev="prevPage()"
    alpine-next="nextPage()"
    alpine-count="programs.length"
    page-size-options="10,20,50"
    alpine-page-size="pageSize"/>
```

| 屬性 | Alpine 對應 |
|------|------|
| `alpine-total` | 總頁數變數名 |
| `alpine-current` | 目前頁碼變數名 |
| `alpine-prev` | 上一頁 method（含括號） |
| `alpine-next` | 下一頁 method（含括號） |
| `alpine-count` | 總筆數表達式 |
| `alpine-page-size` | 每頁筆數變數名 |
| `page-size-options` | 逗號分隔選項 |

---

## JS 工具函數（`eip-components.js`）

| 函數 | 說明 |
|------|------|
| `eipPanelToggle(id)` | 收合/展開 Panel |
| `eipDialogOpen(id)` | 開啟 Dialog |
| `eipDialogClose(id)` | 關閉 Dialog |
| `eipToast(msg, type)` | Toast 通知（success\|error\|warning\|info） |
| `await eipConfirm(msg, title)` | 確認對話框（Promise，回傳 true/false） |

```javascript
// eipConfirm 用法
async function deleteRecord() {
    if (!(await eipConfirm('確定要刪除此筆資料？', '確認刪除'))) return;
    // 執行刪除...
    eipToast('刪除成功', 'success');
}
```

---

## 綜合範例（HRMGD01 style）

```html
<eip-panel title="查詢條件" icon="filter" collapsible="true">
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <eip-form-group label="程式代號">
            <input type="text" class="eip-input" x-model="filter.programNo">
        </eip-form-group>
        <eip-form-group label="負責員工">
            <input type="hidden" id="hidEmpId">
            <eip-search-box api-url="/api/lov/hrm/employees"
                            display-field="employee_no"
                            value-field="employee_id"
                            label-fields="employee_no,employee_name"
                            target-id="hidEmpId"/>
        </eip-form-group>
        <eip-form-group label=" ">
            <div class="flex gap-2 mt-4">
                <eip-button text="查詢" type="primary" icon="search" onclick="search()"/>
                <eip-button text="重置" type="ghost"   icon="refresh" onclick="reset()"/>
            </div>
        </eip-form-group>
    </div>
</eip-panel>

<eip-tabs class="mt-4" active-tab="0">
    <eip-tab title="資料清單" icon="code">
        <!-- Table + Pagination -->
        <eip-pagination alpine-total="totalPages" alpine-current="currentPage"
                        alpine-prev="prevPage()" alpine-next="nextPage()"
                        alpine-count="items.length"/>
    </eip-tab>
    <eip-tab title="維護表單" icon="edit">
        <div class="grid grid-cols-2 gap-4">
            <eip-form-group label="程式名稱" required="true">
                <input type="text" class="eip-input" x-model="form.name">
            </eip-form-group>
        </div>
        <div class="flex justify-end gap-2 mt-4">
            <eip-button text="儲存" type="primary" icon="save"  onclick="save()"/>
            <eip-button text="取消" type="ghost"   icon="close" onclick="cancel()"/>
        </div>
    </eip-tab>
</eip-tabs>

<!-- 刪除確認 Dialog -->
<eip-dialog id="dlgDelete" title="確認刪除" width="sm">
    <p class="text-slate-700 text-sm">確定刪除此筆資料？此動作無法還原。</p>
    <div class="flex justify-end gap-2 mt-5">
        <eip-button text="確認刪除" type="danger"    icon="trash" onclick="doDelete(); eipDialogClose('dlgDelete')"/>
        <eip-button text="取消"     type="secondary" icon="close" onclick="eipDialogClose('dlgDelete')"/>
    </div>
</eip-dialog>
```
