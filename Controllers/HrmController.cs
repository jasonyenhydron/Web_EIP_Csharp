using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Web_EIP_Csharp.Helpers;
using Web_EIP_Csharp.Models;

namespace Web_EIP_Csharp.Controllers
{
    public class HrmController : Controller
    {
        [HttpGet("Hrm/HRMGD47")]
        public IActionResult HRMGD47()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserId = username;
            ViewBag.UserName = HttpContext.Session.GetString("user_name") ?? username;

            var numericId = new string(username.Where(char.IsDigit).ToArray());
            ViewBag.NumericUserId = string.IsNullOrEmpty(numericId) ? username : numericId;

            return View("~/Views/MisPrograms/HRMGD47.cshtml");
        }

        [HttpGet("Hrm/leave-types")]
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

        [HttpPost("Hrm/submit")]
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
    }
}
