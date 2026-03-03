using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Threading.Tasks;

namespace Web_EIP_Csharp.Helpers
{
    public static class OracleDbHelper
    {
        public static OracleConnection GetConnection(string username, string password, string tns)
        {
            // The system now connects using the service account prg / prg7695
            // instead of individual user credentials to the database.
            string connString = $"User Id=prg;Password=prg7695;Data Source={tns};Pooling=false;";
            return new OracleConnection(connString);
        }

        public static async Task<bool> ValidateUserLoginAsync(string username, string password, string tns)
        {
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT IDM.f_idm_check_mis_password(:account, :pwd) FROM DUAL";
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("account", username.ToUpper()));
                    command.Parameters.Add(new OracleParameter("pwd", password));

                    var result = await command.ExecuteScalarAsync();
                    return result != null && result.ToString() == "Y";
                }
            }
        }

        public static async Task<string> GetUserNameAsync(string username, string password, string tns)
        {
            string userName = null;
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT USER_NAME FROM IDM_USER WHERE USER_NO = :user_no";
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("user_no", username.ToUpper()));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            userName = reader["USER_NAME"].ToString();
                        }
                    }
                }
            }
            return userName;
        }

        public static async Task<List<Dictionary<string, string>>> GetProgramSuggestionsAsync(string username, string password, string tns, string query)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT PROGRAM_NO, PROGRAM_NAME
                        FROM idm_program_v
                        WHERE (UPPER(PROGRAM_NO) LIKE :q OR UPPER(PROGRAM_NAME) LIKE :q)
                        AND LANGUAGE_ID = 1
                        AND ROWNUM <= 10
                        ORDER BY PROGRAM_NO ASC";
                    command.BindByName = true;

                    command.Parameters.Add(new OracleParameter("q", $"%{query.ToUpper()}%"));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dict = new Dictionary<string, string>
                            {
                                { "program_no", reader["PROGRAM_NO"].ToString() },
                                { "program_name", reader["PROGRAM_NAME"].ToString() }
                            };
                            suggestions.Add(dict);
                        }
                    }
                }
            }
            return suggestions;
        }

        public static async Task<List<Dictionary<string, string>>> GetLeaveTypesAsync(string username, string password, string tns, string query, int page = 1, int pageSize = 50)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    var offset = Math.Max(0, (page - 1) * pageSize);
                    var endRow = offset + pageSize;
                    command.CommandText = @"
                        SELECT LEAVE_ID, LEAVE_NAME
                        FROM (
                            SELECT LEAVE_ID, LEAVE_NAME,
                                   ROW_NUMBER() OVER (ORDER BY LEAVE_ID ASC) AS RN
                            FROM HRM_LEAVE_L
                            WHERE LANGUAGE_ID = 1
                              AND (UPPER(TO_CHAR(LEAVE_ID)) LIKE :q OR UPPER(LEAVE_NAME) LIKE :q)
                        )
                        WHERE RN > :offset AND RN <= :endRow";
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("q", $"%{query?.ToUpper()}%"));
                    command.Parameters.Add(new OracleParameter("offset", offset));
                    command.Parameters.Add(new OracleParameter("endRow", endRow));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dict = new Dictionary<string, string>
                            {
                                { "leave_id", reader["LEAVE_ID"].ToString() },
                                { "leave_name", reader["LEAVE_NAME"].ToString() }
                            };
                            suggestions.Add(dict);
                        }
                    }
                }
            }
            return suggestions;
        }
        public static async Task<List<Dictionary<string, string>>> GetEmployeesAsync(string username, string password, string tns, string query, int page = 1, int pageSize = 50)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    var offset = Math.Max(0, (page - 1) * pageSize);
                    var endRow = offset + pageSize;
                    command.CommandText = @"
                        SELECT EMPLOYEE_ID, EMPLOYEE_NO, EMPLOYEE_NAME
                        FROM (
                            SELECT EMPLOYEE_ID, EMPLOYEE_NO, EMPLOYEE_NAME,
                                   ROW_NUMBER() OVER (ORDER BY EMPLOYEE_NO ASC) AS RN
                            FROM hrm_employee_v
                            WHERE (UPPER(EMPLOYEE_NO) LIKE :q OR UPPER(EMPLOYEE_NAME) LIKE :q)
                        )
                        WHERE RN > :offset AND RN <= :endRow";
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("q", $"%{query?.ToUpper()}%"));
                    command.Parameters.Add(new OracleParameter("offset", offset));
                    command.Parameters.Add(new OracleParameter("endRow", endRow));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dict = new Dictionary<string, string>
                            {
                                { "employee_id", reader["EMPLOYEE_ID"].ToString() },
                                { "employee_no", reader["EMPLOYEE_NO"].ToString() },
                                { "employee_name", reader["EMPLOYEE_NAME"]?.ToString() ?? "" }
                            };
                            suggestions.Add(dict);
                        }
                    }
                }
            }
            return suggestions;
        }

        public static async Task<List<Dictionary<string, string>>> GetDepartmentsAsync(string username, string password, string tns, string query, int page = 1, int pageSize = 50)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    var offset = Math.Max(0, (page - 1) * pageSize);
                    var endRow = offset + pageSize;
                    command.CommandText = @"
                        SELECT DEPARTMENT_ID, DEPARTMENT_NO, DEPARTMENT_NAME
                        FROM (
                            SELECT DEPARTMENT_ID, DEPARTMENT_NO, DEPARTMENT_NAME,
                                   ROW_NUMBER() OVER (ORDER BY DEPARTMENT_NO ASC) AS RN
                            FROM cmm_department_v
                            WHERE LANGUAGE_ID = 1
                              AND (UPPER(DEPARTMENT_NO) LIKE :q OR UPPER(DEPARTMENT_NAME) LIKE :q)
                        )
                        WHERE RN > :offset AND RN <= :endRow";
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("q", $"%{query?.ToUpper()}%"));
                    command.Parameters.Add(new OracleParameter("offset", offset));
                    command.Parameters.Add(new OracleParameter("endRow", endRow));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dict = new Dictionary<string, string>
                            {
                                { "department_id", reader["DEPARTMENT_ID"].ToString() },
                                { "department_no", reader["DEPARTMENT_NO"].ToString() },
                                { "department_name", reader["DEPARTMENT_NAME"]?.ToString() ?? "" }
                            };
                            suggestions.Add(dict);
                        }
                    }
                }
            }
            return suggestions;
        }

        public static async Task<List<Dictionary<string, string>>> GetBookingDepartmentsAsync(string username, string password, string tns, string query, string employeeId = null, int page = 1, int pageSize = 50)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    var offset = Math.Max(0, (page - 1) * pageSize);
                    var endRow = offset + pageSize;
                    string sql = @"
                        SELECT DEPARTMENT_ID, DEPARTMENT_NO, DEPARTMENT_NAME
                        FROM (
                            SELECT BOOKING_DEPARTMENT_ID AS DEPARTMENT_ID,
                                   BOOKING_DEPARTMENT_NO AS DEPARTMENT_NO,
                                   BOOKING_DEPARTMENT_NAME AS DEPARTMENT_NAME,
                                   ROW_NUMBER() OVER (ORDER BY BOOKING_DEPARTMENT_NO ASC) AS RN
                            FROM HRM_BOOKING_DEPARTMENT_V
                            WHERE LANGUAGE_ID = 1
                              AND (UPPER(BOOKING_DEPARTMENT_NO) LIKE :q OR UPPER(BOOKING_DEPARTMENT_NAME) LIKE :q)";

                    var hasEmployeeIdFilter = long.TryParse(employeeId, out _);
                    if (hasEmployeeIdFilter)
                    {
                        sql += " AND EMPLOYEE_ID = :empId";
                    }
                    sql += @")
                             WHERE RN > :offset AND RN <= :endRow";

                    command.CommandText = sql;
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("q", $"%{query?.ToUpper()}%"));
                    command.Parameters.Add(new OracleParameter("offset", offset));
                    command.Parameters.Add(new OracleParameter("endRow", endRow));

                    if (hasEmployeeIdFilter)
                    {
                        command.Parameters.Add(new OracleParameter("empId", employeeId));
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dict = new Dictionary<string, string>
                            {
                                { "department_id", reader["DEPARTMENT_ID"].ToString() },
                                { "department_no", reader["DEPARTMENT_NO"].ToString() },
                                { "department_name", reader["DEPARTMENT_NAME"]?.ToString() ?? "" }
                            };
                            suggestions.Add(dict);
                        }
                    }
                }
            }
            return suggestions;
        }
    }
}
