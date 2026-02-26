using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Web_EIP_Csharp.Helpers;

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
                        ORDER BY program_no";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
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
    }
}
