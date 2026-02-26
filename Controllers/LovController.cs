using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Web_EIP_Csharp.Helpers;

namespace Web_EIP_Csharp.Controllers
{
    [ApiController]
    [Route("api/lov")]
    public class LovController : ControllerBase
    {
        private bool CheckSession(out string username, out string password, out string tns)
        {
            username = HttpContext.Session.GetString("username");
            password = HttpContext.Session.GetString("password");
            tns = HttpContext.Session.GetString("tns");

            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(tns);
        }

        [HttpGet("hrm/leave-types")]
        public async Task<IActionResult> GetLeaveTypes([FromQuery] string query = "")
        {
            if (!CheckSession(out string username, out string password, out string tns))
            {
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            }

            try
            {
                var types = await OracleDbHelper.GetLeaveTypesAsync(username, password, tns, query);
                return Ok(new { status = "success", data = types });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("hrm/employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] string query = "")
        {
            if (!CheckSession(out string username, out string password, out string tns))
            {
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            }

            try
            {
                var employees = await OracleDbHelper.GetEmployeesAsync(username, password, tns, query);
                return Ok(new { status = "success", data = employees });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("cmm/departments")]
        public async Task<IActionResult> GetDepartments([FromQuery] string query = "")
        {
            if (!CheckSession(out string username, out string password, out string tns))
            {
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            }

            try
            {
                var departments = await OracleDbHelper.GetDepartmentsAsync(username, password, tns, query);
                return Ok(new { status = "success", data = departments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("hrm/booking-departments")]
        public async Task<IActionResult> GetBookingDepartments([FromQuery] string query = "", [FromQuery] string employeeId = "")
        {
            if (!CheckSession(out string username, out string password, out string tns))
            {
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });
            }

            try
            {
                var departments = await OracleDbHelper.GetBookingDepartmentsAsync(username, password, tns, query, employeeId);
                return Ok(new { status = "success", data = departments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
