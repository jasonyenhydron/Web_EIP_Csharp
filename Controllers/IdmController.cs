using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Web_EIP_Csharp.Helpers;

namespace Web_EIP_Csharp.Controllers
{
    public class IdmController : Controller
    {
        [HttpGet("Idm/IDMGD01")]
        public IActionResult IDMGD01()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = username;
            ViewBag.UserName = HttpContext.Session.GetString("user_name");
            ViewBag.NumericUserId = HttpContext.Session.GetString("numeric_user_id");

            return View("~/Views/MisPrograms/IDMGD01.cshtml");
        }

        [HttpGet("Idm/select")]
        public async Task<IActionResult> Select(string DataMember, string programNo, string employeeId, string displayCode)
        {
            if (DataMember != "IDMGD01") return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
            {
                return Unauthorized(new { status = "error", message = "Not logged in" });
            }

            try
            {
                var programs = new List<Dictionary<string, object>>();

                using (var connection = OracleDbHelper.GetConnection(username, password, tns))
                {
                    await connection.OpenAsync();

                    var sql = @"
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

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.BindByName = true;
                        command.Parameters.Add(new OracleParameter("program_no", string.IsNullOrEmpty(programNo) ? "" : programNo));
                        command.Parameters.Add(new OracleParameter("employee_id", string.IsNullOrEmpty(employeeId) ? (object)DBNull.Value : employeeId));
                        command.Parameters.Add(new OracleParameter("display_code", string.IsNullOrEmpty(displayCode) ? (object)DBNull.Value : displayCode));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var dict = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    dict[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                }
                                programs.Add(dict);
                            }
                        }
                    }
                }

                return Ok(new { status = "success", data = programs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // ---- IDMGD01 儲存單筆紀錄 ----
        [HttpPost("Idm/update")]
        public async Task<IActionResult> Update(string DataMember, [FromBody] Dictionary<string, object> payload)
        {
            if (DataMember != "IDMGD01") return BadRequest(new { status = "error", message = "Unsupported DataMember" });
            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns      = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null || !payload.ContainsKey("ROWID"))
                return BadRequest(new { status = "error", message = "Missing ROWID" });

            try
            {
                string rowId      = payload["ROWID"]?.ToString();
                string displayCode = payload.ContainsKey("DISPLAY_CODE") ? payload["DISPLAY_CODE"]?.ToString() : null;
                string purpose     = payload.ContainsKey("PURPOSE")       ? payload["PURPOSE"]?.ToString()       : null;
                string programType = payload.ContainsKey("PROGRAM_TYPE")  ? payload["PROGRAM_TYPE"]?.ToString()  : null;
                string employeeId  = payload.ContainsKey("EMPLOYEE_ID")   ? payload["EMPLOYEE_ID"]?.ToString()   : null;
                string planStart   = payload.ContainsKey("PLAN_START_DEVELOP_DATE") ? payload["PLAN_START_DEVELOP_DATE"]?.ToString() : null;
                string planFinish  = payload.ContainsKey("PLAN_FINISH_DEVELOP_DATE") ? payload["PLAN_FINISH_DEVELOP_DATE"]?.ToString() : null;
                string realStart   = payload.ContainsKey("REAL_START_DEVELOP_DATE") ? payload["REAL_START_DEVELOP_DATE"]?.ToString() : null;
                string realFinish  = payload.ContainsKey("REAL_FINISH_DEVELOP_DATE") ? payload["REAL_FINISH_DEVELOP_DATE"]?.ToString() : null;
                string planHours   = payload.ContainsKey("PLAN_WORK_HOURS") ? payload["PLAN_WORK_HOURS"]?.ToString() : null;
                string realHours   = payload.ContainsKey("REAL_WORK_HOURS") ? payload["REAL_WORK_HOURS"]?.ToString() : null;

                using (var connection = OracleDbHelper.GetConnection(username, password, tns))
                {
                    await connection.OpenAsync();
                    var sql = @"UPDATE idm_program SET
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

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.BindByName  = true;
                        cmd.Parameters.Add(new OracleParameter("display_code", string.IsNullOrEmpty(displayCode) ? (object)DBNull.Value : displayCode));
                        cmd.Parameters.Add(new OracleParameter("purpose",      string.IsNullOrEmpty(purpose)     ? (object)DBNull.Value : purpose));
                        cmd.Parameters.Add(new OracleParameter("program_type", string.IsNullOrEmpty(programType) ? (object)DBNull.Value : programType));
                        cmd.Parameters.Add(new OracleParameter("employee_id",  string.IsNullOrEmpty(employeeId)  ? (object)DBNull.Value : employeeId));
                        cmd.Parameters.Add(new OracleParameter("plan_start",   string.IsNullOrEmpty(planStart)   ? (object)DBNull.Value : planStart));
                        cmd.Parameters.Add(new OracleParameter("plan_finish",  string.IsNullOrEmpty(planFinish)  ? (object)DBNull.Value : planFinish));
                        cmd.Parameters.Add(new OracleParameter("real_start",   string.IsNullOrEmpty(realStart)   ? (object)DBNull.Value : realStart));
                        cmd.Parameters.Add(new OracleParameter("real_finish",  string.IsNullOrEmpty(realFinish)  ? (object)DBNull.Value : realFinish));
                        cmd.Parameters.Add(new OracleParameter("plan_hours",   string.IsNullOrEmpty(planHours)   ? (object)DBNull.Value : decimal.TryParse(planHours, out var ph)   ? (object)ph   : DBNull.Value));
                        cmd.Parameters.Add(new OracleParameter("real_hours",   string.IsNullOrEmpty(realHours)   ? (object)DBNull.Value : decimal.TryParse(realHours, out var rh)   ? (object)rh   : DBNull.Value));
                        cmd.Parameters.Add(new OracleParameter("tr_id",        username));
                        cmd.Parameters.Add(new OracleParameter("row_id",       rowId));

                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0)
                            return NotFound(new { status = "error", message = "Record not found" });
                    }
                }

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
            if (DataMember != "IDMGD01") return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null) return BadRequest(new { status = "error", message = "Missing payload" });

            var programNo = GetString(payload, "PROGRAM_NO")?.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(programNo))
                return BadRequest(new { status = "error", message = "PROGRAM_NO is required" });

            try
            {
                using (var connection = OracleDbHelper.GetConnection(username, password, tns))
                {
                    await connection.OpenAsync();

                    // duplicate check
                    using (var dupCmd = connection.CreateCommand())
                    {
                        dupCmd.CommandText = "SELECT COUNT(1) FROM idm_program WHERE PROGRAM_NO = :program_no";
                        dupCmd.BindByName = true;
                        dupCmd.Parameters.Add(new OracleParameter("program_no", programNo));
                        var count = Convert.ToInt32(await dupCmd.ExecuteScalarAsync());
                        if (count > 0)
                            return Conflict(new { status = "error", message = $"PROGRAM_NO {programNo} already exists" });
                    }

                    long nextProgramId;
                    using (var idCmd = connection.CreateCommand())
                    {
                        idCmd.CommandText = "SELECT NVL(MAX(PROGRAM_ID), 0) + 1 FROM idm_program";
                        var v = await idCmd.ExecuteScalarAsync();
                        nextProgramId = Convert.ToInt64(v);
                    }

                    var sql = @"INSERT INTO idm_program (
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

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.BindByName = true;

                        cmd.Parameters.Add(new OracleParameter("program_id", nextProgramId));
                        cmd.Parameters.Add(new OracleParameter("program_no", programNo));
                        cmd.Parameters.Add(new OracleParameter("display_code", ToDb(GetString(payload, "DISPLAY_CODE") ?? "Y")));
                        cmd.Parameters.Add(new OracleParameter("purpose", ToDb(GetString(payload, "PURPOSE"))));
                        cmd.Parameters.Add(new OracleParameter("program_type", ToDb(GetString(payload, "PROGRAM_TYPE"))));
                        cmd.Parameters.Add(new OracleParameter("employee_id", ToDb(GetString(payload, "EMPLOYEE_ID"))));
                        cmd.Parameters.Add(new OracleParameter("plan_start", ToDb(GetDateString(payload, "PLAN_START_DEVELOP_DATE"))));
                        cmd.Parameters.Add(new OracleParameter("plan_finish", ToDb(GetDateString(payload, "PLAN_FINISH_DEVELOP_DATE"))));
                        cmd.Parameters.Add(new OracleParameter("real_start", ToDb(GetDateString(payload, "REAL_START_DEVELOP_DATE"))));
                        cmd.Parameters.Add(new OracleParameter("real_finish", ToDb(GetDateString(payload, "REAL_FINISH_DEVELOP_DATE"))));
                        cmd.Parameters.Add(new OracleParameter("plan_hours", ToDb(GetDecimal(payload, "PLAN_WORK_HOURS"))));
                        cmd.Parameters.Add(new OracleParameter("real_hours", ToDb(GetDecimal(payload, "REAL_WORK_HOURS"))));
                        cmd.Parameters.Add(new OracleParameter("entry_id", username));
                        cmd.Parameters.Add(new OracleParameter("tr_id", username));

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

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
            if (DataMember != "IDMGD01") return BadRequest(new { status = "error", message = "Unsupported DataMember" });

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            if (payload == null) return BadRequest(new { status = "error", message = "Missing payload" });

            var rowId = GetString(payload, "ROWID");
            var programNo = GetString(payload, "PROGRAM_NO");

            if (string.IsNullOrWhiteSpace(rowId) && string.IsNullOrWhiteSpace(programNo))
                return BadRequest(new { status = "error", message = "ROWID or PROGRAM_NO is required" });

            try
            {
                int affected;
                using (var connection = OracleDbHelper.GetConnection(username, password, tns))
                {
                    await connection.OpenAsync();
                    using (var cmd = connection.CreateCommand())
                    {
                        if (!string.IsNullOrWhiteSpace(rowId))
                        {
                            cmd.CommandText = "DELETE FROM idm_program WHERE ROWID = :row_id";
                            cmd.Parameters.Add(new OracleParameter("row_id", rowId));
                        }
                        else
                        {
                            cmd.CommandText = "DELETE FROM idm_program WHERE PROGRAM_NO = :program_no";
                            cmd.Parameters.Add(new OracleParameter("program_no", programNo.ToUpperInvariant()));
                        }

                        cmd.BindByName = true;
                        affected = await cmd.ExecuteNonQueryAsync();
                    }
                }

                if (affected == 0)
                    return NotFound(new { status = "error", message = "Record not found" });

                return Ok(new { status = "success", message = "Deleted OK" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        private static string GetString(Dictionary<string, object> payload, string key)
            => payload.ContainsKey(key) ? payload[key]?.ToString() : null;

        private static string GetDateString(Dictionary<string, object> payload, string key)
        {
            var raw = GetString(payload, key);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (DateTime.TryParse(raw, out var dt)) return dt.ToString("yyyy-MM-dd");
            return null;
        }

        private static decimal? GetDecimal(Dictionary<string, object> payload, string key)
        {
            var raw = GetString(payload, key);
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out d)) return d;
            return null;
        }

        private static object ToDb(string value) => string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
        private static object ToDb(decimal? value) => value.HasValue ? (object)value.Value : DBNull.Value;
    }
}
