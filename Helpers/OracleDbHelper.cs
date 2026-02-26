using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Threading.Tasks;

namespace Web_EIP_Csharp.Helpers
{
    public static class OracleDbHelper
    {
        public static OracleConnection GetConnection(string username, string password, string tns)
        {
            // Build the connection string using parameters given
            // Ensure no pooling for simple cases, or enable it if preferred
            string connString = $"User Id={username};Password={password};Data Source={tns};Pooling=false;";
            return new OracleConnection(connString);
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

        // Common utility methods can be appended here later
    }
}
