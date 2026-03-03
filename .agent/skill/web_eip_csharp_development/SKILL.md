# web_eip_csharp_development

## Purpose
此 skill 用於本專案的日常開發與修正，確保修改符合目前架構：
- ASP.NET Core MVC + Oracle
- Razor TagHelper 元件化 UI
- Popup + Datagrid + LOV 為主要互動模式

## Project Rules (must follow)
1. 優先使用 `Views/Components` 的 TagHelper。
2. 無法以現有元件實現時，才補頁面專屬 HTML/JS。
3. 共用 JS/CSS 載入優先使用 `<g-js>` / `<g-style>`。
4. Datagrid 相關功能優先擴充 `GDataGridTagHelper`，避免頁面散落重複邏輯。
5. 前後端錯誤顯示優先走 `<g-error-message />`。

## Key Files
- Bootstrap & middleware: `Program.cs`
- Oracle access: `Helpers/OracleDbHelper.cs`
- Layout:
  - `Views/Shared/_Layout.cshtml`
  - `Views/Shared/_PopupLayout.cshtml`
  - `Views/Shared/_LovModal.cshtml`
- Main pages:
  - `Views/MisPrograms/MisPrograms.cshtml`
  - `Views/MisPrograms/IDMGD01.cshtml`
  - `Views/MisPrograms/HRMGD47.cshtml`
- TagHelpers: `Views/Components/*.cs`

## Common Workflows

### A) 新增或修正 Datagrid 功能
1. 先改 `Views/Components/GDataGridTagHelper.cs`
2. 再在目標 `.cshtml` 只調整屬性參數
3. 確認事件有綁定（`@add/@view/@edit/@delete/@query`）
4. `dotnet build` 驗證

### B) LOV 欄位需求
1. 優先用 `<g-lov-input>` 宣告式屬性：
   - `lov-api`, `lov-columns`, `lov-fields`
   - `lov-key-hidden/code/name`
2. 若缺資料來源，再補 `LovController` 與 `OracleDbHelper`
3. 預設啟用 buffer view（每次 50 筆）

### C) 共用資源載入
1. 先使用 `<g-style profile="...">`
2. 再使用 `<g-js profile="...">`
3. 避免在多個頁面重複貼相同 `<script>` / `<link>`

### D) 例外處理
1. API 例外格式以 `Program.cs` exception handler 為準
2. 前端錯誤顯示用 `g-error-message`
3. 需要時補 `message + lineNumber + fileName + detail`

## Validation Checklist
每次改動至少做：
1. `dotnet build`
2. 開頁面看 Console 是否有 Alpine/JS error
3. CRUD 或查詢流程至少走一次
4. 檢查未存檔離開提示（popup 頁）

## Notes
- 本專案目前仍有部分歷史檔案亂碼，修檔時優先保持可編譯與功能正確，再逐步清理。
- SQL 若動態拼接，先檢查 bind 參數與語法，避免 ORA-00933。
