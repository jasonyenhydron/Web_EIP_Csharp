using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Web_EIP_Csharp.Helpers;
using Web_EIP_Csharp.Models.Lov;
using Web_EIP_Csharp.Models.ViewModels;

namespace Web_EIP_Csharp.Controllers
{
    public class MisProgramsController : Controller
    {
        private static string BuildDbConnectionString(string tns) =>
            DbHelper.BuildConnectionString(tns);

        private static LovInputConfig BuildEmployeeLovConfig()
        {
            var employeeLovSql = Uri.EscapeDataString(@"
SELECT employee_id, employee_no, employee_name
FROM (
    SELECT employee_id, employee_no, employee_name,
           ROW_NUMBER() OVER (ORDER BY employee_no ASC) AS rn
    FROM hrm_employee_v
    WHERE (UPPER(employee_no) LIKE :q OR UPPER(employee_name) LIKE :q)
)
WHERE rn > :offset AND rn <= :endRow");

            return new LovInputConfig
            {
                Title = "尋找員工 (Employee)",
                Api = $"/api/lov/query?sql={employeeLovSql}",
                Columns = "編號,名稱,ID",
                Fields = "employee_no,employee_name,employee_id",
                KeyHidden = "employee_id",
                KeyCode = "employee_id",
                DisplayFormat = "{employee_id} {employee_name}",
                SortEnabled = true,
                BufferView = true,
                PageSize = 50
            };
        }

        [HttpGet("mis/programs")]
        public async Task<IActionResult> Index(string program_no, string employee_id, string display_code = "Y")
        {
            var vm = new MisProgramsViewModel
            {
                EmployeeLov = BuildEmployeeLovConfig()
            };

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return RedirectToAction("Login", "Account");

            try
            {
                const string sql = @"
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

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_no", string.IsNullOrEmpty(program_no) ? string.Empty : program_no),
                        DbHelper.CreateParameter("employee_id", string.IsNullOrEmpty(employee_id) ? (object)DBNull.Value : employee_id),
                        DbHelper.CreateParameter("display_code", string.IsNullOrEmpty(display_code) ? (object)DBNull.Value : display_code)
                    });

                var programs = new List<Dictionary<string, object>>();
                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn c in dt.Columns)
                        dict[c.ColumnName] = row[c] == DBNull.Value ? string.Empty : row[c];
                    programs.Add(dict);
                }

                var categories = new Dictionary<string, List<Dictionary<string, object>>>();
                var systemNames = new Dictionary<string, string>
                {
                    { "HRM", "HRM 系統" }, { "FIN", "FIN 系統" }, { "INV", "INV 系統" }, { "PUR", "PUR 系統" },
                    { "SAL", "SAL 系統" }, { "MFG", "MFG 系統" }, { "SDM", "SDM 系統" }, { "IDM", "IDM 系統" }
                };

                foreach (var program in programs)
                {
                    var progNo = program.ContainsKey("PROGRAM_NO") ? program["PROGRAM_NO"]?.ToString() ?? string.Empty : string.Empty;
                    if (progNo.Length >= 3)
                    {
                        var prefix = progNo.Substring(0, 3);
                        var systemName = systemNames.ContainsKey(prefix) ? systemNames[prefix] : $"{prefix} 系統";
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
                ViewBag.ProgramNoFilter = program_no ?? string.Empty;
                ViewBag.EmployeeIdFilter = employee_id ?? string.Empty;
                ViewBag.DisplayCodeFilter = display_code ?? "Y";
                ViewBag.UserName = HttpContext.Session.GetString("user_name") ?? username;

                return View("MisPrograms", vm);
            }
            catch (Exception e)
            {
                ViewBag.Error = $"系統錯誤: {e.Message}";
                ViewBag.Programs = new List<Dictionary<string, object>>();
                return View("MisPrograms", vm);
            }
        }

        [HttpGet("api/mis/programs/suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 1 || q.Length > 50)
                return Json(new List<object>());

            var username = HttpContext.Session.GetString("username");
            var password = HttpContext.Session.GetString("password");
            var tns = HttpContext.Session.GetString("tns");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tns))
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                const string sql = @"
                    SELECT PROGRAM_NO, PROGRAM_NAME
                    FROM (
                        SELECT PROGRAM_NO, PROGRAM_NAME
                        FROM idm_program_v
                        WHERE (UPPER(PROGRAM_NO) LIKE :q OR UPPER(PROGRAM_NAME) LIKE :q)
                          AND LANGUAGE_ID = 1
                        ORDER BY PROGRAM_NO ASC
                    )
                    WHERE ROWNUM <= 10";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("q", $"%{q.ToUpper()}%")
                    });

                var list = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new Dictionary<string, string>
                    {
                        ["program_no"] = row["PROGRAM_NO"]?.ToString() ?? string.Empty,
                        ["program_name"] = row["PROGRAM_NAME"]?.ToString() ?? string.Empty
                    });
                }
                return Json(list);
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
                return Unauthorized(new { status = "error", message = "Not logged in" });

            try
            {
                const string sql = @"
                    SELECT LANGUAGE_ID, PROGRAM_ID, PROGRAM_NO, PROGRAM_NAME,
                           EMPLOYEE_ID, PURPOSE,
                           PLAN_START_DEVELOP_DATE, PLAN_FINISH_DEVELOP_DATE,
                           REAL_START_DEVELOP_DATE, REAL_FINISH_DEVELOP_DATE,
                           PLAN_WORK_HOURS, REAL_WORK_HOURS,
                           DISPLAY_CODE, PROGRAM_TYPE
                    FROM idm_program_v
                    WHERE program_no = :program_no
                      AND LANGUAGE_ID = 1";

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    sql,
                    new DbParameter[]
                    {
                        DbHelper.CreateParameter("program_no", program_no.ToUpperInvariant())
                    });

                if (dt.Rows.Count == 0)
                    return NotFound(new { status = "error", message = "Program not found" });

                var row = dt.Rows[0];
                var result = new Dictionary<string, object>();
                foreach (DataColumn c in dt.Columns)
                    result[c.ColumnName] = row[c] == DBNull.Value ? string.Empty : row[c];

                return Json(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = "error", message = e.Message });
            }
        }
    }
}



