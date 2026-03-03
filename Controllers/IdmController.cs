using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Threading.Tasks;
using Web_EIP_Csharp.Helpers;

namespace Web_EIP_Csharp.Controllers
{
    public class IdmController : Controller
    {
        private static string BuildDbConnectionString(string tns) =>
            DbHelper.DefaultConnectionString;

        [HttpGet("Idm/IDMGD01")]
        public IActionResult IDMGD01()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

            ViewBag.UserId = username;
            ViewBag.UserName = HttpContext.Session.GetString("user_name");
            ViewBag.NumericUserId = HttpContext.Session.GetString("numeric_user_id");
            return View("~/Views/MisPrograms/IDMGD01.cshtml");
        }

        [HttpGet("Idm/select")]
        public async Task<IActionResult> Select(string DataMember, string programNo, string employeeId, string displayCode)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                const string sql = @"
                    SELECT ROWID, PROGRAM_ID, PURPOSE, EMPLOYEE_ID, VENDOR_ID, PERSON_ID,
                           PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                           REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                           PLAN_WORK_HOURS, REAL_WORK_HOURS,
                           ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE,
                           PROGRAM_NO, DISPLAY_CODE, PROGRAM_TYPE
                    FROM idm_program
                    WHERE program_no like :program_no || '%'
                      AND (employee_id = :employee_id or :employee_id is null)
                      AND (display_code = :display_code or :display_code is null)
                    ORDER BY program_no";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_no", string.IsNullOrEmpty(programNo) ? string.Empty : programNo),
                        DbHelper.CreateParameter("employee_id", string.IsNullOrEmpty(employeeId) ? (object)DBNull.Value : employeeId),
                        DbHelper.CreateParameter("display_code", string.IsNullOrEmpty(displayCode) ? (object)DBNull.Value : displayCode)
                    });

                var programs = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn c in dt.Columns)
                        dict[c.ColumnName] = row[c] == DBNull.Value ? string.Empty : row[c];
                    programs.Add(dict);
                }

                return Ok(new { status = "success", data = programs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/update")]
        public async Task<IActionResult> Update(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null || !payload.ContainsKey("ROWID"))
                return BadRequest(new { status = "error", message = "Missing ROWID" });

            try
            {
                var rowId = GetString(payload, "ROWID");
                var displayCode = GetString(payload, "DISPLAY_CODE");
                var purpose = GetString(payload, "PURPOSE");
                var programType = GetString(payload, "PROGRAM_TYPE");
                var employeeId = GetString(payload, "EMPLOYEE_ID");
                var planStart = GetDateString(payload, "PLAN_START_DEVELOP_DATE");
                var planFinish = GetDateString(payload, "PLAN_FINISH_DEVELOP_DATE");
                var realStart = GetDateString(payload, "REAL_START_DEVELOP_DATE");
                var realFinish = GetDateString(payload, "REAL_FINISH_DEVELOP_DATE");
                var planHours = GetDecimal(payload, "PLAN_WORK_HOURS");
                var realHours = GetDecimal(payload, "REAL_WORK_HOURS");

                const string sql = @"
                    UPDATE idm_program SET
                        DISPLAY_CODE              = :display_code,
                        PURPOSE                   = :purpose,
                        PROGRAM_TYPE              = :program_type,
                        EMPLOYEE_ID               = :employee_id,
                        PLAN_START_DEVELOP_DATE   = TO_DATE(:plan_start,  'YYYY-MM-DD'),
                        PLAN_FINISH_DEVELOP_DATE  = TO_DATE(:plan_finish, 'YYYY-MM-DD'),
                        REAL_START_DEVELOP_DATE   = TO_DATE(:real_start,  'YYYY-MM-DD'),
                        REAL_FINISH_DEVELOP_DATE  = TO_DATE(:real_finish, 'YYYY-MM-DD'),
                        PLAN_WORK_HOURS           = :plan_hours,
                        REAL_WORK_HOURS           = :real_hours,
                        TR_ID                     = :tr_id,
                        TR_DATE                   = SYSDATE
                    WHERE ROWID = :row_id";

                var rows = await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("display_code", string.IsNullOrEmpty(displayCode) ? (object)DBNull.Value : displayCode),
                        DbHelper.CreateParameter("purpose", string.IsNullOrEmpty(purpose) ? (object)DBNull.Value : purpose),
                        DbHelper.CreateParameter("program_type", string.IsNullOrEmpty(programType) ? (object)DBNull.Value : programType),
                        DbHelper.CreateParameter("employee_id", string.IsNullOrEmpty(employeeId) ? (object)DBNull.Value : employeeId),
                        DbHelper.CreateParameter("plan_start", string.IsNullOrEmpty(planStart) ? (object)DBNull.Value : planStart),
                        DbHelper.CreateParameter("plan_finish", string.IsNullOrEmpty(planFinish) ? (object)DBNull.Value : planFinish),
                        DbHelper.CreateParameter("real_start", string.IsNullOrEmpty(realStart) ? (object)DBNull.Value : realStart),
                        DbHelper.CreateParameter("real_finish", string.IsNullOrEmpty(realFinish) ? (object)DBNull.Value : realFinish),
                        DbHelper.CreateParameter("plan_hours", planHours.HasValue ? (object)planHours.Value : DBNull.Value),
                        DbHelper.CreateParameter("real_hours", realHours.HasValue ? (object)realHours.Value : DBNull.Value),
                        DbHelper.CreateParameter("tr_id", username),
                        DbHelper.CreateParameter("row_id", rowId)
                    });

                if (rows == 0)
                    return NotFound(new { status = "error", message = "Record not found" });

                return Ok(new { status = "success", message = "Saved OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/insert")]
        public async Task<IActionResult> Insert(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var programNo = GetString(payload, "PROGRAM_NO")?.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(programNo))
                return BadRequest(new { status = "error", message = "PROGRAM_NO is required" });

            try
            {
                var conn = BuildDbConnectionString(tns);
                var dupObj = await DbHelper.ExecuteScalarAsync(
                    conn,
                    CommandType.Text,
                    "SELECT COUNT(1) FROM idm_program WHERE PROGRAM_NO = :program_no",
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_no", programNo)
                    });
                if (Convert.ToInt32(dupObj) > 0)
                    return Conflict(new { status = "error", message = $"PROGRAM_NO {programNo} already exists" });

                var idObj = await DbHelper.ExecuteScalarAsync(
                    conn,
                    CommandType.Text,
                    "SELECT NVL(MAX(PROGRAM_ID), 0) + 1 FROM idm_program");
                var nextProgramId = Convert.ToInt64(idObj);

                const string sql = @"
                    INSERT INTO idm_program (
                        PROGRAM_ID, PROGRAM_NO, DISPLAY_CODE, PURPOSE, PROGRAM_TYPE, EMPLOYEE_ID,
                        PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                        REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                        PLAN_WORK_HOURS, REAL_WORK_HOURS,
                        ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE
                    ) VALUES (
                        :program_id, :program_no, :display_code, :purpose, :program_type, :employee_id,
                        TO_DATE(:plan_start,  'YYYY-MM-DD'), TO_DATE(:plan_finish, 'YYYY-MM-DD'),
                        TO_DATE(:real_start,  'YYYY-MM-DD'), TO_DATE(:real_finish, 'YYYY-MM-DD'),
                        :plan_hours, :real_hours,
                        :entry_id, SYSDATE, :tr_id, SYSDATE
                    )";

                await DbHelper.ExecuteNonQueryAsync(
                    conn,
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_id", nextProgramId),
                        DbHelper.CreateParameter("program_no", programNo),
                        DbHelper.CreateParameter("display_code", ToDb(GetString(payload, "DISPLAY_CODE") ?? "Y")),
                        DbHelper.CreateParameter("purpose", ToDb(GetString(payload, "PURPOSE"))),
                        DbHelper.CreateParameter("program_type", ToDb(GetString(payload, "PROGRAM_TYPE"))),
                        DbHelper.CreateParameter("employee_id", ToDb(GetString(payload, "EMPLOYEE_ID"))),
                        DbHelper.CreateParameter("plan_start", ToDb(GetDateString(payload, "PLAN_START_DEVELOP_DATE"))),
                        DbHelper.CreateParameter("plan_finish", ToDb(GetDateString(payload, "PLAN_FINISH_DEVELOP_DATE"))),
                        DbHelper.CreateParameter("real_start", ToDb(GetDateString(payload, "REAL_START_DEVELOP_DATE"))),
                        DbHelper.CreateParameter("real_finish", ToDb(GetDateString(payload, "REAL_FINISH_DEVELOP_DATE"))),
                        DbHelper.CreateParameter("plan_hours", ToDb(GetDecimal(payload, "PLAN_WORK_HOURS"))),
                        DbHelper.CreateParameter("real_hours", ToDb(GetDecimal(payload, "REAL_WORK_HOURS"))),
                        DbHelper.CreateParameter("entry_id", username),
                        DbHelper.CreateParameter("tr_id", username)
                    });

                return Ok(new { status = "success", message = "Inserted OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/delete")]
        public async Task<IActionResult> Delete(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var rowId = GetString(payload, "ROWID");
            var programNo = GetString(payload, "PROGRAM_NO");
            if (string.IsNullOrWhiteSpace(rowId) && string.IsNullOrWhiteSpace(programNo))
                return BadRequest(new { status = "error", message = "ROWID or PROGRAM_NO is required" });

            try
            {
                string sql;
                DbParameter[] ps;
                if (!string.IsNullOrWhiteSpace(rowId))
                {
                    sql = "DELETE FROM idm_program WHERE ROWID = :row_id";
                    ps = new DbParameter[] { DbHelper.CreateParameter("row_id", rowId) };
                }
                else
                {
                    sql = "DELETE FROM idm_program WHERE PROGRAM_NO = :program_no";
                    ps = new DbParameter[] { DbHelper.CreateParameter("program_no", programNo!.ToUpperInvariant()) };
                }

                var affected = await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    ps);

                if (affected == 0)
                    return NotFound(new { status = "error", message = "Record not found" });

                return Ok(new { status = "success", message = "Deleted OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("Idm/selectRoleFunctionPrograms")]
        public async Task<IActionResult> SelectRoleFunctionPrograms(string DataMember, long? programId, int positionNo = 1)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });
            if (!programId.HasValue)
                return Ok(new { status = "success", data = new List<Dictionary<string, object>>() });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                const string sql = @"
                    SELECT ROWID, ROLE_FUNCTION_ID, PROGRAM_ID, POSITION_NO,
                           ROLE_ID, ROLE_NO, ROLE_NAME, FUNCTION_NO,
                           DISPLAY_ORDER, DISPLAY_COLOR,
                           ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE
                    FROM idm_role_function_program
                    WHERE PROGRAM_ID = :program_id
                      AND POSITION_NO = :position_no
                    ORDER BY NVL(DISPLAY_ORDER, 999999), ROLE_NO, FUNCTION_NO";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_id", programId.Value),
                        DbHelper.CreateParameter("position_no", positionNo)
                    });

                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn c in dt.Columns)
                        dict[c.ColumnName] = row[c] == DBNull.Value ? string.Empty : row[c];
                    rows.Add(dict);
                }

                return Ok(new { status = "success", data = rows });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/upsertRoleFunctionProgram")]
        public async Task<IActionResult> UpsertRoleFunctionProgram(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var programId = GetLong(payload, "PROGRAM_ID");
            if (!programId.HasValue)
                return BadRequest(new { status = "error", message = "PROGRAM_ID is required" });

            var rowId = GetString(payload, "ROWID");
            var positionNo = GetLong(payload, "POSITION_NO") ?? 1;
            var roleId = GetLong(payload, "ROLE_ID");
            var roleNo = GetString(payload, "ROLE_NO");
            var roleName = GetString(payload, "ROLE_NAME");
            var functionNo = GetString(payload, "FUNCTION_NO");
            var displayOrder = GetLong(payload, "DISPLAY_ORDER");
            var displayColor = GetString(payload, "DISPLAY_COLOR");

            try
            {
                var conn = BuildDbConnectionString(tns);
                if (!string.IsNullOrWhiteSpace(rowId))
                {
                    const string updateSql = @"
                        UPDATE idm_role_function_program SET
                            ROLE_ID = :role_id,
                            ROLE_NO = :role_no,
                            ROLE_NAME = :role_name,
                            FUNCTION_NO = :function_no,
                            DISPLAY_ORDER = :display_order,
                            DISPLAY_COLOR = :display_color,
                            POSITION_NO = :position_no,
                            TR_ID = :tr_id,
                            TR_DATE = SYSDATE
                        WHERE ROWID = :row_id";

                    var affected = await DbHelper.ExecuteNonQueryAsync(
                        conn,
                        CommandType.Text,
                        updateSql,
                        new DbParameter[]
                        {
                            DbHelper.CreateParameter("role_id", ToDb(roleId)),
                            DbHelper.CreateParameter("role_no", ToDb(roleNo)),
                            DbHelper.CreateParameter("role_name", ToDb(roleName)),
                            DbHelper.CreateParameter("function_no", ToDb(functionNo)),
                            DbHelper.CreateParameter("display_order", ToDb(displayOrder)),
                            DbHelper.CreateParameter("display_color", ToDb(displayColor)),
                            DbHelper.CreateParameter("position_no", positionNo),
                            DbHelper.CreateParameter("tr_id", username),
                            DbHelper.CreateParameter("row_id", rowId)
                        });

                    if (affected == 0)
                        return NotFound(new { status = "error", message = "Detail record not found" });
                }
                else
                {
                    var nextIdObj = await DbHelper.ExecuteScalarAsync(
                        conn,
                        CommandType.Text,
                        "SELECT NVL(MAX(ROLE_FUNCTION_ID), 0) + 1 FROM idm_role_function_program");
                    var nextId = Convert.ToInt64(nextIdObj);

                    const string insertSql = @"
                        INSERT INTO idm_role_function_program (
                            ROLE_FUNCTION_ID, PROGRAM_ID, POSITION_NO,
                            ROLE_ID, ROLE_NO, ROLE_NAME, FUNCTION_NO, DISPLAY_ORDER, DISPLAY_COLOR,
                            ENTRY_ID, ENTRY_DATE, TR_ID, TR_DATE
                        ) VALUES (
                            :role_function_id, :program_id, :position_no,
                            :role_id, :role_no, :role_name, :function_no, :display_order, :display_color,
                            :entry_id, SYSDATE, :tr_id, SYSDATE
                        )";

                    await DbHelper.ExecuteNonQueryAsync(
                        conn,
                        CommandType.Text,
                        insertSql,
                        new DbParameter[]
                        {
                            DbHelper.CreateParameter("role_function_id", nextId),
                            DbHelper.CreateParameter("program_id", programId.Value),
                            DbHelper.CreateParameter("position_no", positionNo),
                            DbHelper.CreateParameter("role_id", ToDb(roleId)),
                            DbHelper.CreateParameter("role_no", ToDb(roleNo)),
                            DbHelper.CreateParameter("role_name", ToDb(roleName)),
                            DbHelper.CreateParameter("function_no", ToDb(functionNo)),
                            DbHelper.CreateParameter("display_order", ToDb(displayOrder)),
                            DbHelper.CreateParameter("display_color", ToDb(displayColor)),
                            DbHelper.CreateParameter("entry_id", username),
                            DbHelper.CreateParameter("tr_id", username)
                        });
                }

                return Ok(new { status = "success", message = "Saved OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("Idm/deleteRoleFunctionProgram")]
        public async Task<IActionResult> DeleteRoleFunctionProgram(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01")
                return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null)
                return BadRequest(new { status = "error", message = "Missing payload" });

            var rowId = GetString(payload, "ROWID");
            var roleFunctionId = GetLong(payload, "ROLE_FUNCTION_ID");
            if (string.IsNullOrWhiteSpace(rowId) && !roleFunctionId.HasValue)
                return BadRequest(new { status = "error", message = "ROWID or ROLE_FUNCTION_ID is required" });

            try
            {
                string sql;
                DbParameter[] ps;

                if (!string.IsNullOrWhiteSpace(rowId))
                {
                    sql = "DELETE FROM idm_role_function_program WHERE ROWID = :row_id";
                    ps = new[] { DbHelper.CreateParameter("row_id", rowId) };
                }
                else
                {
                    sql = "DELETE FROM idm_role_function_program WHERE ROLE_FUNCTION_ID = :role_function_id";
                    ps = new[] { DbHelper.CreateParameter("role_function_id", roleFunctionId!.Value) };
                }

                var affected = await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    ps);

                if (affected == 0)
                    return NotFound(new { status = "error", message = "Detail record not found" });

                return Ok(new { status = "success", message = "Deleted OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        private static string? GetString(Dictionary<string, object> payload, string key) =>
            payload.ContainsKey(key) ? payload[key]?.ToString() : null;

        private static string? GetDateString(Dictionary<string, object> payload, string key)
        {
            var raw = GetString(payload, key);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return DateTime.TryParse(raw, out var dt) ? dt.ToString("yyyy-MM-dd") : null;
        }

        private static decimal? GetDecimal(Dictionary<string, object> payload, string key)
        {
            var raw = GetString(payload, key);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out d)) return d;
            return null;
        }

        private static long? GetLong(Dictionary<string, object> payload, string key)
        {
            var raw = GetString(payload, key);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (long.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val)) return val;
            if (long.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out val)) return val;
            return null;
        }

        private static object ToDb(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
        private static object ToDb(decimal? value) => value.HasValue ? value.Value : DBNull.Value;
        private static object ToDb(long? value) => value.HasValue ? value.Value : DBNull.Value;
    }
}



