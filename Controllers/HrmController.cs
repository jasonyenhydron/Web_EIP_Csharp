using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Web_EIP_Csharp.Helpers;
using Web_EIP_Csharp.Models;

namespace Web_EIP_Csharp.Controllers
{
    public class HrmController : Controller
    {
        private static string BuildDbConnectionString(string tns) =>
            DbHelper.DefaultConnectionString;

        [HttpGet("Hrm/HRMGD47")]
        public IActionResult HRMGD47()
        {
            var username = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login", "Account");

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
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                const string sql = @"
                    SELECT LEAVE_ID, LEAVE_NAME
                    FROM HRM_LEAVE_L
                    WHERE LANGUAGE_ID = 1
                      AND (UPPER(TO_CHAR(LEAVE_ID)) LIKE :q OR UPPER(LEAVE_NAME) LIKE :q)
                    ORDER BY LEAVE_ID";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("q", $"%{query.ToUpper()}%")
                    });

                var data = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    data.Add(new Dictionary<string, string>
                    {
                        ["leave_id"] = row["LEAVE_ID"]?.ToString() ?? string.Empty,
                        ["leave_name"] = row["LEAVE_NAME"]?.ToString() ?? string.Empty
                    });
                }

                return Json(new { status = "success", data });
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
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            if (!ModelState.IsValid)
                return BadRequest(new { status = "error", message = "Data validation failed", errors = ModelState });

            try
            {
                const string sql = @"
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

                var newIdParam = DbHelper.CreateParameter("new_id", null, DbType.Int64, ParameterDirection.Output);

                await DbHelper.ExecuteNonQueryAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("employee_id", model.EmployeeId),
                        DbHelper.CreateParameter("start_time", model.StartTime),
                        DbHelper.CreateParameter("end_time", model.EndTime),
                        DbHelper.CreateParameter("leave_id", model.LeaveId),
                        DbHelper.CreateParameter("system_leave_hours", model.SystemLeaveHours ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("leave_hours", model.LeaveHours ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("leave_days", model.LeaveDays ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("ask_for_leave_reason", model.AskForLeaveReason ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("em_ask_for_leave_status", "00"),
                        DbHelper.CreateParameter("flow_yn", "N"),
                        DbHelper.CreateParameter("agent_employee_id", model.AgentEmployeeId ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("destination_place", model.DestinationPlace ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("talking_about", model.TalkingAbout ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("return_yn", model.ReturnYn ?? (object)DBNull.Value),
                        DbHelper.CreateParameter("entry_id", username.ToUpperInvariant()),
                        newIdParam
                    });

                var generatedId = newIdParam.Value?.ToString();
                return Ok(new { status = "success", message = "Application submitted successfully", id = generatedId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}



