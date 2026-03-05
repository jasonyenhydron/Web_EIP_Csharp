# OracleDbHelper Frontend API（Oracle 11g）

以下 API 由 `Controllers/OracleOpsController.cs` 提供，底層呼叫 `Helpers/OracleDbHelper.cs`。

## 1) 執行 Procedure / Package Procedure

`POST /api/oracle/proc/execute`

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

回傳：

```json
{
  "status": "success",
  "outputs": {
    "p_message": "OK"
  }
}
```

## 2) 執行 Function

`POST /api/oracle/func/execute`

```json
{
  "functionName": "PKG_HRM.GET_EMP_NAME",
  "returnDbType": "String",
  "parameters": [
    { "name": "p_emp_id", "value": 1001, "dbType": "Int32", "direction": "Input" }
  ]
}
```

## 3) Scheduler Job

### 建立 Job
`POST /api/oracle/job/create`

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

### 執行 Job
`POST /api/oracle/job/run`

```json
{
  "jobName": "JOB_SYNC_EMP",
  "useCurrentSession": false
}
```

### 啟用/停用/刪除 Job
- `POST /api/oracle/job/enable`
- `POST /api/oracle/job/disable`
- `POST /api/oracle/job/drop`

Request body：

```json
{
  "jobName": "JOB_SYNC_EMP",
  "force": true
}
```

### 查詢 Job
`GET /api/oracle/job/list`

可選參數：`owner`

---

## 參數欄位說明

- `dbType`: 對應 .NET `System.Data.DbType`，例如 `String`, `Int32`, `DateTime`, `Decimal`。
- `direction`: `Input`, `Output`, `InputOutput`, `ReturnValue`。

## 安全限制

- 物件名稱（procedure/function/package/job）會驗證格式，只允許 Oracle 識別字元。
- 所有輸入值都走 bind parameter，不拼接值到 SQL。
- 需通過登入 Session（沿用現有系統驗證）。

