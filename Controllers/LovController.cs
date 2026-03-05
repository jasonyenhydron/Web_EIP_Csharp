// 功能：LOV 查詢 API 控制器，提供可選值資料的條件查詢與分頁取資料。
// 輸入：輸入 sql、query、page、pageSize 與 Session 連線資訊。
// 輸出：輸出 LOV 分頁 JSON 結果、未授權回應或錯誤訊息。
// 依賴：DbHelper、Regex 參數解析、HttpContext.Session、ASP.NET Core Web API。

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Web_EIP_Csharp.Helpers;

namespace Web_EIP_Csharp.Controllers
{
    [ApiController]
    [Route("api/lov")]
    public class LovController : ControllerBase
    {
        private static string BuildDbConnectionString(string tns) => DbHelper.BuildConnectionString(tns);

        private bool CheckSession(out string username, out string password, out string tns)
        {
            username = HttpContext.Session.GetString("username") ?? string.Empty;
            password = HttpContext.Session.GetString("password") ?? string.Empty;
            tns = HttpContext.Session.GetString("tns") ?? string.Empty;
            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(tns);
        }

        private static readonly Regex BindRegex = new(@":([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        [HttpGet("query")]
        public async Task<IActionResult> QueryLov(
            [FromQuery] string sql = "",
            [FromQuery] string query = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (!CheckSession(out _, out _, out var tns))
                return Unauthorized(new { status = "error", message = "Session expired. Please log in again." });

            if (string.IsNullOrWhiteSpace(sql))
                return BadRequest(new { status = "error", message = "Missing sql." });

            var normalized = sql.Trim();
            if (!normalized.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { status = "error", message = "Only SELECT is allowed." });
            if (normalized.Contains(";"))
                return BadRequest(new { status = "error", message = "Semicolon is not allowed." });

            try
            {
                page = page < 1 ? 1 : page;
                pageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);
                var offset = Math.Max(0, (page - 1) * pageSize);
                var endRow = offset + pageSize;

                var foundBinds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (Match m in BindRegex.Matches(normalized))
                {
                    if (m.Success) foundBinds.Add(m.Groups[1].Value);
                }

                var parameters = new List<DbParameter>();

                if (foundBinds.Contains("q"))
                    parameters.Add(DbHelper.CreateParameter("q", $"%{query.ToUpper()}%"));
                if (foundBinds.Contains("offset"))
                    parameters.Add(DbHelper.CreateParameter("offset", offset));
                if (foundBinds.Contains("endRow"))
                    parameters.Add(DbHelper.CreateParameter("endRow", endRow));

                foreach (var bind in foundBinds)
                {
                    if (bind.Equals("q", StringComparison.OrdinalIgnoreCase) ||
                        bind.Equals("offset", StringComparison.OrdinalIgnoreCase) ||
                        bind.Equals("endRow", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var raw = Request.Query[bind].ToString();
                    if (string.IsNullOrEmpty(raw))
                        parameters.Add(DbHelper.CreateParameter(bind, DBNull.Value));
                    else
                        parameters.Add(DbHelper.CreateParameter(bind, raw));
                }

                var dt = await DbHelper.GetDataTableAsync(
                    BuildDbConnectionString(tns),
                    CommandType.Text,
                    normalized,
                    parameters.ToArray());

                var data = new List<Dictionary<string, string>>();
                foreach (DataRow row in dt.Rows)
                {
                    var item = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (DataColumn col in dt.Columns)
                    {
                        item[col.ColumnName.ToLowerInvariant()] = row[col]?.ToString() ?? string.Empty;
                    }
                    data.Add(item);
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
