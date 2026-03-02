using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Web_EIP_Csharp.Helpers;
using Web_EIP_Csharp.Models;

namespace Web_EIP_Csharp.Controllers
{
    public class MisProgramsController : Controller
    {
        [HttpGet("mis/programs")]
        public async Task<IActionResult> Index(string program_no, string employee_id, string display_code = "Y")
        {
            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var programs = new List<Dictionary<string, object>>();

                using (var connection = OracleDbHelper.GetConnection(username, password, tns))
                {
                    await connection.OpenAsync();

                    var sql = @"
                        SELECT LANGUAGE_ID, PROGRAM_ID, PROGRAM_NO, PROGRAM_NAME,
                               EMPLOYEE_ID, PURPOSE,
                               PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                               REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                               PLAN_WORK_HOURS, REAL_WORK_HOURS,
                               DISPLAY_CODE, PROGRAM_TYPE
                        FROM idm_program_v
                        WHERE program_no LIKE :program_no || '%'
                          AND (employee_id = :employee_id OR :employee_id IS NULL)
                          AND (display_code = :display_code OR :display_code IS NULL)
                          AND LANGUAGE_ID = 1
                        ORDER BY program_no";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.BindByName = true;
                        command.Parameters.Add(new OracleParameter("program_no", string.IsNullOrEmpty(program_no) ? "" : program_no));
                        command.Parameters.Add(new OracleParameter("employee_id", string.IsNullOrEmpty(employee_id) ? (object)DBNull.Value : employee_id));
                        command.Parameters.Add(new OracleParameter("display_code", string.IsNullOrEmpty(display_code) ? (object)DBNull.Value : display_code));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var dict = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    dict.Add(reader.GetName(i), reader.GetValue(i));
                                }
                                programs.Add(dict);
                            }
                        }
                    }
                }

                // Categorize programs by prefix (System only)
                var categories = new Dictionary<string, List<Dictionary<string, object>>>();
                var systemNames = new Dictionary<string, string>
                {
                    { "HRM", "人力資源管理系統" },
                    { "FIN", "財務管理系統" },
                    { "INV", "庫存管理系統" },
                    { "PUR", "採購管理系統" },
                    { "SAL", "銷售管理系統" },
                    { "MFG", "生產管理系統" },
                    { "SDM", "訂單管理系統" }
                };

                foreach (var program in programs)
                {
                    var progNo = program.ContainsKey("PROGRAM_NO") ? program["PROGRAM_NO"].ToString() : "";
                    if (progNo.Length >= 3)
                    {
                        var systemPrefix = progNo.Substring(0, 3);
                        var systemName = systemNames.ContainsKey(systemPrefix) ? systemNames[systemPrefix] : $"{systemPrefix} 系統";

                        if (!categories.ContainsKey(systemName))
                            categories[systemName] = new List<Dictionary<string, object>>();

                        categories[systemName].Add(program);
                    }
                    else
                    {
                        if (!categories.ContainsKey("其他"))
                            categories["其他"] = new List<Dictionary<string, object>>();
                        categories["其他"].Add(program);
                    }
                }

                ViewBag.Categories = categories;
                ViewBag.Programs = programs;
                ViewBag.ProgramNoFilter = program_no ?? "";
                ViewBag.EmployeeIdFilter = employee_id ?? "";
                ViewBag.DisplayCodeFilter = display_code ?? "Y";
                ViewBag.UserName = HttpContext.Session.GetString("user_name") ?? username;

                return View("MisPrograms");
            }
            catch (OracleException e)
            {
                ViewBag.Error = $"資料庫錯誤: {e.Message}";
                ViewBag.Programs = new List<Dictionary<string, object>>();
                return View("MisPrograms");
            }
            catch (Exception e)
            {
                ViewBag.Error = $"系統錯誤: {e.Message}";
                ViewBag.Programs = new List<Dictionary<string, object>>();
                return View("MisPrograms");
            }
        }

        [HttpGet("api/mis/programs/suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 1 || q.Length > 50)
            {
                return Json(new List<object>()); // return empty list
            }

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
            {
                return Unauthorized(new { status = "error", message = "Not logged in" });
            }

            try
            {
                var suggestions = await OracleDbHelper.GetProgramSuggestionsAsync(username, password, tns, q);
                return Json(suggestions);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = "error", message = e.Message });
            }
        }

        [HttpGet("api/mis/programs/{program_no}")]
        public async Task<IActionResult> GetProgramDetail(string program_no)
        {
            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
            {
                return Unauthorized(new { status = "error", message = "Not logged in" });
            }

            try
            {
                using (var connection = OracleDbHelper.GetConnection(username, password, tns))
                {
                    await connection.OpenAsync();

                    var sql = @"
                        SELECT LANGUAGE_ID, PROGRAM_ID, PROGRAM_NO, PROGRAM_NAME,
                               EMPLOYEE_ID, PURPOSE,
                               PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                               REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                               PLAN_WORK_HOURS, REAL_WORK_HOURS,
                               DISPLAY_CODE, PROGRAM_TYPE
                        FROM idm_program_v
                        WHERE program_no = :program_no
                          AND LANGUAGE_ID = 1";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.BindByName = true;
                        command.Parameters.Add(new OracleParameter("program_no", program_no.ToUpper()));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var program = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var val = reader.GetValue(i);
                                    program.Add(reader.GetName(i), val == DBNull.Value ? null : val);
                                }
                                return Json(program);
                            }
                            else
                            {
                                return NotFound(new { status = "error", message = "Program not found" });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = "error", message = e.Message });
            }
        }

        [HttpGet("mis/programs/HRMGD47")]
        public IActionResult HRMGD47()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = username;
            ViewBag.UserName = HttpContext.Session.GetString("user_name") ?? username;

            // Extract numeric employee ID from username (e.g. GONU7353 -> 7353)
            var numericId = new string(username.Where(char.IsDigit).ToArray());
            ViewBag.NumericUserId = string.IsNullOrEmpty(numericId) ? username : numericId;

            return View();
        }

        [HttpGet("api/mis/programs/HRMGD47/leave-types")]
        public async Task<IActionResult> GetLeaveTypes([FromQuery] string query = "")
        {
            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
            {
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            }

            try
            {
                var types = await OracleDbHelper.GetLeaveTypesAsync(username, password, tns, query);
                return Json(new { status = "success", data = types });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("api/mis/programs/HRMGD47/submit")]
        public async Task<IActionResult> SubmitLeaveApplication([FromBody] HrmEmAskForLeave model)
        {
            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
            {
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = "error", message = "Data validation failed", errors = ModelState });
            }

            try
            {
                using (var connection = OracleDbHelper.GetConnection(username, password, tns))
                {
                    await connection.OpenAsync();

                    var sql = @"
                        INSERT INTO hrm_em_ask_for_leave (
                            employee_id, start_time, end_time, leave_id,
                            system_leave_hours, leave_hours, leave_days,
                            ask_for_leave_reason, em_ask_for_leave_status,
                            flow_yn, agent_employee_id,
                            destination_place, talking_about, return_yn,
                            entry_id, entry_date
                        ) VALUES (
                            :employee_id, :start_time, :end_time, :leave_id,
                            :system_leave_hours, :leave_hours, :leave_days,
                            :ask_for_leave_reason, :em_ask_for_leave_status,
                            :flow_yn, :agent_employee_id,
                            :destination_place, :talking_about, :return_yn,
                            :entry_id, sysdate
                        ) RETURNING em_ask_for_leave_id INTO :new_id";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.BindByName = true;

                        command.Parameters.Add(new OracleParameter("employee_id", model.EmployeeId));
                        command.Parameters.Add(new OracleParameter("start_time", model.StartTime));
                        command.Parameters.Add(new OracleParameter("end_time", model.EndTime));
                        command.Parameters.Add(new OracleParameter("leave_id", model.LeaveId));

                        command.Parameters.Add(new OracleParameter("system_leave_hours", model.SystemLeaveHours ?? (object)DBNull.Value));
                        command.Parameters.Add(new OracleParameter("leave_hours", model.LeaveHours ?? (object)DBNull.Value));
                        command.Parameters.Add(new OracleParameter("leave_days", model.LeaveDays ?? (object)DBNull.Value));
                        command.Parameters.Add(new OracleParameter("ask_for_leave_reason", model.AskForLeaveReason ?? (object)DBNull.Value));

                        command.Parameters.Add(new OracleParameter("em_ask_for_leave_status", "00"));
                        command.Parameters.Add(new OracleParameter("flow_yn", "N"));

                        command.Parameters.Add(new OracleParameter("agent_employee_id", model.AgentEmployeeId ?? (object)DBNull.Value));
                        command.Parameters.Add(new OracleParameter("destination_place", model.DestinationPlace ?? (object)DBNull.Value));
                        command.Parameters.Add(new OracleParameter("talking_about", model.TalkingAbout ?? (object)DBNull.Value));
                        command.Parameters.Add(new OracleParameter("return_yn", model.ReturnYn ?? (object)DBNull.Value));

                        command.Parameters.Add(new OracleParameter("entry_id", username.ToUpper()));

                        var newIdParam = new OracleParameter("new_id", OracleDbType.Int64)
                        {
                            Direction = System.Data.ParameterDirection.Output
                        };
                        command.Parameters.Add(newIdParam);

                        await command.ExecuteNonQueryAsync();

                        string generatedId = newIdParam.Value?.ToString();

                        return Ok(new { status = "success", message = "Application submitted successfully", id = generatedId });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
        [HttpGet("mis/programs/IDMGD01")]
        public IActionResult IDMGD01()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = username;
            ViewBag.UserName = HttpContext.Session.GetString("user_name");
            ViewBag.NumericUserId = HttpContext.Session.GetString("numeric_user_id"); // Assuming this might be logged somewhere, else null

            return View("~/Views/MisPrograms/IDMGD01.cshtml");
        }

        [HttpGet("api/mis/programs/IDMGD01/list")]
        public async Task<IActionResult> GetIdmProgramList(string programNo, string employeeId, string displayCode)
        {
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
        [HttpPut("api/mis/programs/IDMGD01/save")]
        public async Task<IActionResult> SaveIdmProgram([FromBody] Dictionary<string, object> payload)
        {
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
    }
}
