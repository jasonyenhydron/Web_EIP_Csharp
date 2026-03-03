using System.Data;
using System.Data.Common;
using System.Collections.Concurrent;
using Oracle.ManagedDataAccess.Client;

namespace Web_EIP_Csharp.Helpers
{
    /// <summary>
    /// General ADO.NET helper for SQL execution.
    /// Default provider is Oracle.ManagedDataAccess.
    /// </summary>
    public static class OracleDbHelper
    {
        private static readonly object SyncRoot = new();
        private static readonly ConcurrentDictionary<string, DbParameter[]> ParameterCache = new(StringComparer.OrdinalIgnoreCase);
        private static DbProviderFactory _providerFactory = OracleClientFactory.Instance;
        private static string _defaultConnectionString = string.Empty;

        /// <summary>Retry count upper bound for retry methods.</summary>
        public static int MaxRetryCount { get; set; } = 10;

        /// <summary>Retry wait milliseconds from 2nd attempt.</summary>
        public static int RetryWaitMilliseconds { get; set; } = 6000;

        /// <summary>Default command timeout in seconds. 0 = no timeout.</summary>
        public static int DefaultCommandTimeout { get; set; } = 0;

        public static string DefaultConnectionString => _defaultConnectionString;

        public static void Configure(string defaultConnectionString, DbProviderFactory? providerFactory = null)
        {
            lock (SyncRoot)
            {
                _defaultConnectionString = defaultConnectionString?.Trim() ?? string.Empty;
                _providerFactory = providerFactory ?? OracleClientFactory.Instance;
            }
        }

        public static DbConnection CreateConnection(string connectionString)
        {
            var conn = _providerFactory.CreateConnection() ?? throw new InvalidOperationException("Cannot create DbConnection.");
            conn.ConnectionString = connectionString;
            return conn;
        }

        /// <summary>
        /// Create an OracleConnection using default connection string from appsettings.
        /// </summary>
        public static OracleConnection GetConnection()
        {
            return new OracleConnection(GetDefaultConnectionString());
        }

        /// <summary>
        /// Create an OracleConnection using custom connection string.
        /// </summary>
        public static OracleConnection GetConnection(string connectionString)
        {
            return new OracleConnection(connectionString);
        }

        public static DbParameter CreateParameter(
            string name,
            object? value,
            DbType? dbType = null,
            ParameterDirection direction = ParameterDirection.Input,
            int? size = null)
        {
            var p = _providerFactory.CreateParameter() ?? throw new InvalidOperationException("Cannot create DbParameter.");
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            p.Direction = direction;
            if (dbType.HasValue) p.DbType = dbType.Value;
            if (size.HasValue) p.Size = size.Value;
            return p;
        }

        public static void CacheParameters(string cacheKey, params DbParameter[]? parameters)
        {
            if (string.IsNullOrWhiteSpace(cacheKey) || parameters == null)
                return;

            ParameterCache[cacheKey] = CloneParameters(parameters);
        }

        public static DbParameter[]? GetCachedParameters(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
                return null;

            return ParameterCache.TryGetValue(cacheKey, out var cached)
                ? CloneParameters(cached)
                : null;
        }

        public static int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteNonQuery(GetDefaultConnectionString(), cmdType, cmdText, commandParameters);

        public static int ExecuteNonQueryText(string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteNonQuery(GetDefaultConnectionString(), CommandType.Text, cmdText, commandParameters);

        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            using var conn = CreateConnection(connectionString);
            using var cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            var val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        public static async Task<int> ExecuteNonQueryAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? commandParameters = null, CancellationToken cancellationToken = default)
        {
            await using var conn = CreateConnection(connectionString);
            await using var cmd = conn.CreateCommand();
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, commandParameters, cancellationToken);
            var val = await cmd.ExecuteNonQueryAsync(cancellationToken);
            cmd.Parameters.Clear();
            return val;
        }

        public static int ExecuteNonQueryRetry(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            return Retry(() => ExecuteNonQuery(connectionString, cmdType, cmdText, commandParameters));
        }

        public static object? ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteScalar(GetDefaultConnectionString(), cmdType, cmdText, commandParameters);

        public static object? ExecuteScalarText(string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteScalar(GetDefaultConnectionString(), CommandType.Text, cmdText, commandParameters);

        public static object? ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            using var conn = CreateConnection(connectionString);
            using var cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            var val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        public static async Task<object?> ExecuteScalarAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? commandParameters = null, CancellationToken cancellationToken = default)
        {
            await using var conn = CreateConnection(connectionString);
            await using var cmd = conn.CreateCommand();
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, commandParameters, cancellationToken);
            var val = await cmd.ExecuteScalarAsync(cancellationToken);
            cmd.Parameters.Clear();
            return val;
        }

        public static object? ExecuteScalarRetry(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            return Retry(() => ExecuteScalar(connectionString, cmdType, cmdText, commandParameters));
        }

        public static DbDataReader ExecuteReader(CommandType cmdType, string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteReader(GetDefaultConnectionString(), cmdType, cmdText, commandParameters);

        public static DbDataReader ExecuteReaderText(string cmdText, params DbParameter[]? commandParameters) =>
            ExecuteReader(GetDefaultConnectionString(), CommandType.Text, cmdText, commandParameters);

        public static DbDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            var conn = CreateConnection(connectionString);
            var cmd = conn.CreateCommand();
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return reader;
            }
            catch
            {
                conn.Close();
                conn.Dispose();
                cmd.Dispose();
                throw;
            }
        }

        public static DataTable GetDataTable(CommandType cmdType, string cmdText, params DbParameter[]? commandParameters) =>
            GetDataTable(GetDefaultConnectionString(), cmdType, cmdText, commandParameters);

        public static DataTable GetDataTableText(string cmdText, params DbParameter[]? commandParameters) =>
            GetDataTable(GetDefaultConnectionString(), CommandType.Text, cmdText, commandParameters);

        public static DataTable GetDataTable(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            var dt = new DataTable();
            using var conn = CreateConnection(connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandTimeout = 0;
            PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
            using var adapter = _providerFactory.CreateDataAdapter() ?? throw new InvalidOperationException("Cannot create DbDataAdapter.");
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            cmd.Parameters.Clear();
            return dt;
        }

        public static async Task<DataTable> GetDataTableAsync(string connectionString, CommandType cmdType, string cmdText, DbParameter[]? commandParameters = null, CancellationToken cancellationToken = default)
        {
            var dt = new DataTable();
            await using var conn = CreateConnection(connectionString);
            await using var cmd = conn.CreateCommand();
            await PrepareCommandAsync(cmd, conn, null, cmdType, cmdText, commandParameters, cancellationToken);
            using var adapter = _providerFactory.CreateDataAdapter() ?? throw new InvalidOperationException("Cannot create DbDataAdapter.");
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            cmd.Parameters.Clear();
            return dt;
        }

        public static DataTable GetDataTableRetry(string connectionString, CommandType cmdType, string cmdText, params DbParameter[]? commandParameters)
        {
            return Retry(() => GetDataTable(connectionString, cmdType, cmdText, commandParameters));
        }

        public static void ExecuteSqlTran(Dictionary<string, DbParameter[]?> sqlStringList) =>
            ExecuteSqlTran(GetDefaultConnectionString(), CommandType.Text, sqlStringList);

        public static void ExecuteSqlTran(string connectionString, CommandType cmdType, Dictionary<string, DbParameter[]?> sqlStringList)
        {
            using var conn = CreateConnection(connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            try
            {
                foreach (var item in sqlStringList)
                {
                    PrepareCommand(cmd, conn, trans, cmdType, item.Key, item.Value);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public static void ExecuteSqlTran(string connectionString, CommandType cmdType, IEnumerable<SqlBatchItem> items)
        {
            using var conn = CreateConnection(connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            try
            {
                foreach (var item in items)
                {
                    PrepareCommand(cmd, conn, trans, cmdType, item.CommandText, item.Parameters);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public static bool Exists(string connectionString, string sql, params DbParameter[]? commandParameters)
        {
            var scalar = ExecuteScalar(connectionString, CommandType.Text, sql, commandParameters);
            if (scalar == null || scalar == DBNull.Value) return false;
            return Convert.ToInt64(scalar) > 0;
        }

        public static bool Exists(string sql, params DbParameter[]? commandParameters) =>
            Exists(GetDefaultConnectionString(), sql, commandParameters);

        private static void PrepareCommand(
            DbCommand cmd,
            DbConnection conn,
            DbTransaction? trans,
            CommandType cmdType,
            string cmdText,
            params DbParameter[]? cmdParams)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            if (trans != null) cmd.Transaction = trans;
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = DefaultCommandTimeout;
            if (cmd is OracleCommand oracleCmd)
                oracleCmd.BindByName = true;
            if (cmdParams != null && cmdParams.Length > 0)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(cmdParams);
            }
        }

        private static async Task PrepareCommandAsync(
            DbCommand cmd,
            DbConnection conn,
            DbTransaction? trans,
            CommandType cmdType,
            string cmdText,
            DbParameter[]? cmdParams,
            CancellationToken cancellationToken)
        {
            if (conn.State != ConnectionState.Open) await conn.OpenAsync(cancellationToken);
            if (trans != null) cmd.Transaction = trans;
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = DefaultCommandTimeout;
            if (cmd is OracleCommand oracleCmd)
                oracleCmd.BindByName = true;
            if (cmdParams != null && cmdParams.Length > 0)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(cmdParams);
            }
        }

        private static string GetDefaultConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_defaultConnectionString))
                throw new InvalidOperationException("OracleDbHelper default connection string not configured.");
            return _defaultConnectionString;
        }

        private static T Retry<T>(Func<T> action)
        {
            Exception? last = null;
            for (var attempt = 1; attempt <= MaxRetryCount; attempt++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (attempt >= MaxRetryCount) break;
                    if (attempt >= 2) Thread.Sleep(RetryWaitMilliseconds);
                }
            }

            throw last ?? new InvalidOperationException("Retry failed with unknown error.");
        }

        private static DbParameter[] CloneParameters(DbParameter[] source)
        {
            var clones = new DbParameter[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                var p = source[i];
                if (p is ICloneable cloneable)
                {
                    clones[i] = (DbParameter)cloneable.Clone();
                    continue;
                }

                var np = _providerFactory.CreateParameter() ?? throw new InvalidOperationException("Cannot create DbParameter.");
                np.ParameterName = p.ParameterName;
                np.DbType = p.DbType;
                np.Direction = p.Direction;
                np.Size = p.Size;
                np.Precision = p.Precision;
                np.Scale = p.Scale;
                np.SourceColumn = p.SourceColumn;
                np.SourceVersion = p.SourceVersion;
                np.IsNullable = p.IsNullable;
                np.Value = p.Value;
                clones[i] = np;
            }
            return clones;
        }
    }

    public sealed class SqlBatchItem
    {
        public string CommandText { get; set; } = string.Empty;
        public DbParameter[]? Parameters { get; set; }
    }

    /// <summary>
    /// Oracle executor wrapper with IDisposable lifecycle.
    /// Connection string defaults to OracleDbHelper configured value (appsettings.json).
    /// </summary>
    public sealed class OracleSqlExecutor : IDisposable
    {
        private readonly OracleConnection _connection;

        public OracleSqlExecutor(string? connectionString = null)
        {
            var conn = string.IsNullOrWhiteSpace(connectionString)
                ? OracleDbHelper.DefaultConnectionString
                : connectionString;
            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("OracleDbHelper default connection string not configured.");
            _connection = new OracleConnection(conn);
        }

        private void EnsureConnectionOpen()
        {
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public int ExecuteNonQuery(string sql, OracleParameter[]? parameters = null)
        {
            EnsureConnectionOpen();
            using var cmd = new OracleCommand(sql, _connection) { BindByName = true };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public DataTable GetDataTable(string sql, OracleParameter[]? parameters = null)
        {
            EnsureConnectionOpen();
            using var cmd = new OracleCommand(sql, _connection) { BindByName = true };
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            using var adapter = new OracleDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public void Dispose()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
            _connection.Dispose();
        }
    }
}

