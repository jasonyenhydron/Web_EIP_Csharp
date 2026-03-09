# Oracle API 規格（同步至目前版本）

本文件描述目前專案中 Oracle 相關 API 的使用方式與注意事項。

## 1. 基本說明
- Controller：`Controllers/OracleOpsController.cs`
- 核心資料存取：`Helpers/OracleDbHelper.cs`
- 主要用途：
  - 呼叫 Procedure / Function
  - 操作 Scheduler Job（建立、執行、啟停、刪除、查詢）

## 2. Procedure / Package Procedure
- Endpoint：`POST /api/oracle/proc/execute`

Request 範例：
```json
{
  "packageName": "PKG_HRM",
  "procedureName": "SYNC_EMPLOYEE",
  "parameters": [
    { "name": "p_emp_id", "value": 1001, "dbType": "Int32", "direction": "Input" },
    { "name": "p_message", "value": null, "dbType": "String", "direction": "Output", "size": 4000 }
  ]
}
```

Response 範例：
```json
{
  "status": "success",
  "outputs": {
    "p_message": "OK"
  }
}
```

## 3. Function
- Endpoint：`POST /api/oracle/func/execute`

Request 範例：
```json
{
  "functionName": "PKG_HRM.GET_EMP_NAME",
  "returnDbType": "String",
  "parameters": [
    { "name": "p_emp_id", "value": 1001, "dbType": "Int32", "direction": "Input" }
  ]
}
```

## 4. Scheduler Job

### 4.1 建立 Job
- Endpoint：`POST /api/oracle/job/create`

```json
{
  "jobName": "JOB_SYNC_EMP",
  "jobType": "PLSQL_BLOCK",
  "jobAction": "BEGIN PKG_HRM.SYNC_EMPLOYEE; END;",
  "repeatInterval": "FREQ=MINUTELY;INTERVAL=10",
  "enabled": true,
  "autoDrop": false,
  "comments": "Sync employee every 10 mins"
}
```

### 4.2 執行 Job
- Endpoint：`POST /api/oracle/job/run`

```json
{
  "jobName": "JOB_SYNC_EMP",
  "useCurrentSession": false
}
```

### 4.3 啟用 / 停用 / 刪除 Job
- `POST /api/oracle/job/enable`
- `POST /api/oracle/job/disable`
- `POST /api/oracle/job/drop`

Request body 範例：
```json
{
  "jobName": "JOB_SYNC_EMP",
  "force": true
}
```

### 4.4 查詢 Job
- Endpoint：`GET /api/oracle/job/list`
- 支援 query string：`owner`

## 5. 參數格式規格
- `dbType`：對應 .NET `System.Data.DbType`，例如 `String`, `Int32`, `DateTime`, `Decimal`
- `direction`：`Input`, `Output`, `InputOutput`, `ReturnValue`

## 6. 錯誤與安全
- 所有參數皆應使用 bind parameter（禁止字串拼接 SQL）
- API 回傳以 JSON 為主，錯誤時包含 `status` / `message`
- 請在登入與 Session 有效下呼叫 API

## 7. 與目前前端規格的關聯
- LOV、DataGrid、DataForm 主要走業務 API（如 `/api/lov/*`、`/Idm/*`、`/api/*`），
  本文件的 Oracle API 主要用於 DBA/批次/維運導向操作。
- 近期 UI 規格更新（LOV runtime、DataForm 新版、Sidebar 修正）不影響本 Oracle API 路由。
