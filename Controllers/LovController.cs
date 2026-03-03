using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Web_EIP_Csharp.Helpers;

namespace Web_EIP_Csharp.Controllers
{
    [ApiController]
    [Route("api/lov")]
    public class LovController : ControllerBase
    {
        private static string BuildDbConnectionString(string tns) =>
            DbHelper.DefaultConnectionString;

        private bool CheckSession(out string username, out string password, out string tns)
        {
            username = HttpContext.Session.GetString("username") ?? string.Empty;
            password = HttpContext.Session.GetString("password") ?? string.Empty;
            tns = HttpContext.Session.GetString("tns") ?? string.Empty;

            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(tns);
        }

        [HttpGet("hrm/leave-types")]
        public async Task<IActionResult> GetLeaveTypes([FromQuery] string query = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                page = page < 1 ? 1 : page;
                pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
                var offset = Math.Max(0, (page - 1) * pageSize);
                var endRow = offset + pageSize;

                const string sql = @"
                    SELECT LEAVE_ID, LEAVE_NAME
                    FROM (
                        SELECT LEAVE_ID, LEAVE_NAME,
                               ROW_NUMBER() OVER (ORDER BY LEAVE_ID ASC) AS RN
                        FROM HRM_LEAVE_L
                        WHERE LANGUAGE_ID = 1
                          AND (UPPER(TO_CHAR(LEAVE_ID)) LIKE :q OR UPPER(LEAVE_NAME) LIKE :q)
                    )
                    WHERE RN > :offset AND RN <= :endRow";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("q", $"%{query.ToUpper()}%"),
                        DbHelper.CreateParameter("offset", offset),
                        DbHelper.CreateParameter("endRow", endRow)
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

                return Ok(new { status = "success", data, page, pageSize, hasMore = data.Count >= pageSize });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("hrm/employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] string query = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                page = page < 1 ? 1 : page;
                pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
                var offset = Math.Max(0, (page - 1) * pageSize);
                var endRow = offset + pageSize;

                const string sql = @"
                    SELECT EMPLOYEE_ID, EMPLOYEE_NO, EMPLOYEE_NAME
                    FROM (
                        SELECT EMPLOYEE_ID, EMPLOYEE_NO, EMPLOYEE_NAME,
                               ROW_NUMBER() OVER (ORDER BY EMPLOYEE_NO ASC) AS RN
                        FROM hrm_employee_v
                        WHERE (UPPER(EMPLOYEE_NO) LIKE :q OR UPPER(EMPLOYEE_NAME) LIKE :q)
                    )
                    WHERE RN > :offset AND RN <= :endRow";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("q", $"%{query.ToUpper()}%"),
                        DbHelper.CreateParameter("offset", offset),
                        DbHelper.CreateParameter("endRow", endRow)
                    });

                var data = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    data.Add(new Dictionary<string, string>
                    {
                        ["employee_id"] = row["EMPLOYEE_ID"]?.ToString() ?? string.Empty,
                        ["employee_no"] = row["EMPLOYEE_NO"]?.ToString() ?? string.Empty,
                        ["employee_name"] = row["EMPLOYEE_NAME"]?.ToString() ?? string.Empty
                    });
                }

                return Ok(new { status = "success", data, page, pageSize, hasMore = data.Count >= pageSize });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("cmm/departments")]
        public async Task<IActionResult> GetDepartments([FromQuery] string query = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                page = page < 1 ? 1 : page;
                pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
                var offset = Math.Max(0, (page - 1) * pageSize);
                var endRow = offset + pageSize;

                const string sql = @"
                    SELECT DEPARTMENT_ID, DEPARTMENT_NO, DEPARTMENT_NAME
                    FROM (
                        SELECT DEPARTMENT_ID, DEPARTMENT_NO, DEPARTMENT_NAME,
                               ROW_NUMBER() OVER (ORDER BY DEPARTMENT_NO ASC) AS RN
                        FROM cmm_department_v
                        WHERE LANGUAGE_ID = 1
                          AND (UPPER(DEPARTMENT_NO) LIKE :q OR UPPER(DEPARTMENT_NAME) LIKE :q)
                    )
                    WHERE RN > :offset AND RN <= :endRow";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("q", $"%{query.ToUpper()}%"),
                        DbHelper.CreateParameter("offset", offset),
                        DbHelper.CreateParameter("endRow", endRow)
                    });

                var data = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    data.Add(new Dictionary<string, string>
                    {
                        ["department_id"] = row["DEPARTMENT_ID"]?.ToString() ?? string.Empty,
                        ["department_no"] = row["DEPARTMENT_NO"]?.ToString() ?? string.Empty,
                        ["department_name"] = row["DEPARTMENT_NAME"]?.ToString() ?? string.Empty
                    });
                }

                return Ok(new { status = "success", data, page, pageSize, hasMore = data.Count >= pageSize });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("hrm/booking-departments")]
        public async Task<IActionResult> GetBookingDepartments([FromQuery] string query = "", [FromQuery] string employeeId = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            try
            {
                page = page < 1 ? 1 : page;
                pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
                var offset = Math.Max(0, (page - 1) * pageSize);
                var endRow = offset + pageSize;

                var sql = @"
                    SELECT DEPARTMENT_ID, DEPARTMENT_NO, DEPARTMENT_NAME
                    FROM (
                        SELECT BOOKING_DEPARTMENT_ID AS DEPARTMENT_ID,
                               BOOKING_DEPARTMENT_NO AS DEPARTMENT_NO,
                               BOOKING_DEPARTMENT_NAME AS DEPARTMENT_NAME,
                               ROW_NUMBER() OVER (ORDER BY BOOKING_DEPARTMENT_NO ASC) AS RN
                        FROM HRM_BOOKING_DEPARTMENT_V
                        WHERE LANGUAGE_ID = 1
                          AND (UPPER(BOOKING_DEPARTMENT_NO) LIKE :q OR UPPER(BOOKING_DEPARTMENT_NAME) LIKE :q)";

                var hasEmployeeId = long.TryParse(employeeId, out _);
                if (hasEmployeeId)
                    sql += " AND EMPLOYEE_ID = :empId";

                sql += @")
                         WHERE RN > :offset AND RN <= :endRow";

                var parameters = new List<DbParameter>
                {
                    DbHelper.CreateParameter("q", $"%{query.ToUpper()}%"),
                    DbHelper.CreateParameter("offset", offset),
                    DbHelper.CreateParameter("endRow", endRow)
                };
                if (hasEmployeeId)
                    parameters.Add(DbHelper.CreateParameter("empId", employeeId));

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    parameters.ToArray());

                var data = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    data.Add(new Dictionary<string, string>
                    {
                        ["department_id"] = row["DEPARTMENT_ID"]?.ToString() ?? string.Empty,
                        ["department_no"] = row["DEPARTMENT_NO"]?.ToString() ?? string.Empty,
                        ["department_name"] = row["DEPARTMENT_NAME"]?.ToString() ?? string.Empty
                    });
                }

                return Ok(new { status = "success", data, page, pageSize, hasMore = data.Count >= pageSize });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}



