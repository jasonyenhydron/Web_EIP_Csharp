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

        public static async Task<List<Dictionary<string, string>>> GetLeaveTypesAsync(string username, string password, string tns, string query)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    // Basic Query to HRM_LEAVE_L
                    command.CommandText = @"
                        SELECT LEAVE_ID, LEAVE_NAME
                        FROM HRM_LEAVE_L
                        WHERE LANGUAGE_ID = 1
                        AND (UPPER(TO_CHAR(LEAVE_ID)) LIKE :q OR UPPER(LEAVE_NAME) LIKE :q)
                        ORDER BY LEAVE_ID ASC";
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("q", $"%{query?.ToUpper()}%"));

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
        public static async Task<List<Dictionary<string, string>>> GetEmployeesAsync(string username, string password, string tns, string query)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT EMPLOYEE_ID, EMPLOYEE_NO, EMPLOYEE_NAME
                        FROM hrm_employee_v
                        WHERE (UPPER(EMPLOYEE_NO) LIKE :q OR UPPER(EMPLOYEE_NAME) LIKE :q)
                        AND ROWNUM <= 50
                        ORDER BY EMPLOYEE_NO ASC";
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("q", $"%{query?.ToUpper()}%"));

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

        public static async Task<List<Dictionary<string, string>>> GetDepartmentsAsync(string username, string password, string tns, string query)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT DEPARTMENT_ID, DEPARTMENT_NO, DEPARTMENT_NAME
                        FROM cmm_department_v
                        WHERE LANGUAGE_ID = 1 AND (UPPER(DEPARTMENT_NO) LIKE :q OR UPPER(DEPARTMENT_NAME) LIKE :q)
                        AND ROWNUM <= 50
                        ORDER BY DEPARTMENT_NO ASC";
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("q", $"%{query?.ToUpper()}%"));

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

        public static async Task<List<Dictionary<string, string>>> GetBookingDepartmentsAsync(string username, string password, string tns, string query, string employeeId = null)
        {
            var suggestions = new List<Dictionary<string, string>>();
            using (var connection = GetConnection(username, password, tns))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    string sql = @"
                        SELECT BOOKING_DEPARTMENT_ID AS DEPARTMENT_ID,
                               BOOKING_DEPARTMENT_NO AS DEPARTMENT_NO,
                               BOOKING_DEPARTMENT_NAME AS DEPARTMENT_NAME
                        FROM HRM_BOOKING_DEPARTMENT_V
                        WHERE LANGUAGE_ID = 1
                        AND (UPPER(BOOKING_DEPARTMENT_NO) LIKE :q OR UPPER(BOOKING_DEPARTMENT_NAME) LIKE :q)";

                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        sql += " AND EMPLOYEE_ID = :empId";
                    }
                    sql += " AND ROWNUM <= 50 ORDER BY BOOKING_DEPARTMENT_NO ASC";

                    command.CommandText = sql;
                    command.BindByName = true;
                    command.Parameters.Add(new OracleParameter("q", $"%{query?.ToUpper()}%"));

                    if (!string.IsNullOrEmpty(employeeId))
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
