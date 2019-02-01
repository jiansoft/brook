using jIAnSoft.Brook.Configuration;
using jIAnSoft.Brook.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace jIAnSoft.Brook
{
    public class DbProvider : ProviderBase, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Db factory
        /// </summary>
        private DbProviderFactory _provider;

        /// <summary>
        /// ConnectionStringSettings
        /// </summary>
        private ConnectionStringSettings _connSetting;

        /// <summary>
        /// DatabaseConfiguration
        /// </summary>
        internal DatabaseConfiguration DbConfig;

        /*/// <summary>
        /// Connection resource
        /// </summary>
        internal DbConnection Conn { get; set; }*/

        /// <summary>
        /// 目前連線的資料庫位置
        /// </summary>
        public string ConnectionSource => _connSetting.ConnectionString;

        /*
        /// <summary>
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="dbName"></param>
        /// <param name="providerName"></param>
        public DbProvider(string host, int port, string dbName, string user, string password, string providerName)
            : this(new DatabaseConfiguration
            {
                Connection = $"server={host},{port};database={dbName};uid={user};pwd={password};",
                ProviderName = providerName,
                Name = dbName
            })
        {
        }*/

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dbConfig"></param>
        public DbProvider(DatabaseConfiguration dbConfig) : this(
            new ConnectionStringSettings
            {
                ConnectionString = dbConfig.Connection,
                ProviderName = dbConfig.ProviderName,
                Name = dbConfig.Name
            })
        {
            DbConfig = dbConfig;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="connSettings"></param>
        private DbProvider(ConnectionStringSettings connSettings)
        {
            _connSetting = connSettings;
#if NET461
            _provider = DbProviderFactories.GetFactory(_connSetting.ProviderName);
#elif NETSTANDARD2_0
            _provider = DbProviderFactories.GetFactory(_connSetting.ProviderName);
#endif
        }

        private static string PrintDbParameters(IReadOnlyCollection<DbParameter> parameters)
        {
            if (null == parameters)
            {
                return string.Empty;
            }

            var t = new string[parameters.Count];
            var i = 0;
            foreach (var p in parameters)
            {
                var key = p.ParameterName ?? "null";
                var value = p.Value ?? "null";
                t[i] = $"{{Key:'{key}',Val:'{value.ToString().Replace("\"", "\\\"")}'}}";
                i++;
            }

            return $"[{string.Join(" ,", t)}]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static T CreateInstanceFromReader<T>(IDataRecord reader)
        {
            var instance = Activator.CreateInstance<T>();
            for (var i = reader.FieldCount - 1; i >= 0; i--)
            {
                ReflectionHelpers.SetValue(instance, reader.GetName(i), reader.GetValue(i));
            }

            return instance;
        }
        
        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnection" /> class.
        /// </summary>
        /// <returns>A new instance of <see cref="T:System.Data.Common.DbConnection" />.</returns>
        internal DbConnection CreateConnection()
        {
            var con = _provider.CreateConnection();
            if (con == null)
            {
                throw new SqlException(
                    $"Can`t connect to specified sql server({_connSetting.Name}. Please check the connection string.");
            }

            con.ConnectionString = _connSetting.ConnectionString;
            return con;
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbDataAdapter" /> class.
        /// </summary>
        /// <returns>A new instance of <see cref="T:System.Data.Common.DbDataAdapter" />.</returns>
        private DbDataAdapter CreateDataAdapter()
        {
            var adapter = _provider.CreateDataAdapter();
            if (adapter != null)
            {
                return adapter;
            }

            if (!string.Equals("MySql.Data.MySqlClient", DbConfig.ProviderName, StringComparison.Ordinal))
            {
                throw new SqlException(
                    "DbProviderFactory can't create a new instance of the provider's DataAdapter class.");
            }

            adapter = Assembly.Load("MySql.Data").CreateInstance("MySql.Data.MySqlClient.MySqlDataAdapter") as DbDataAdapter;
            if (adapter == null)
            {
                throw new SqlException(
                    "DbProviderFactory can't create a new instance of the provider's DataAdapter class.");
            }

            return adapter;
        }

        /// <summary>
        /// Creates and returns a <see cref="T:System.Data.Common.DbCommand" /> object associated with the current connection.
        /// </summary>
        /// <returns>A <see cref="T:System.Data.Common.DbCommand" /> object.</returns>
        private DbCommand CreateCommand(int timeout, CommandType type, string sql, DbParameter[] parameters)
        {
            var cmd = _provider.CreateCommand();
            if (cmd == null)
            {
                throw new SqlException($"Cannot create a sql command ({DbConfig.Name}).");
            }
            
            cmd.CommandTimeout = timeout;
            cmd.CommandText = sql;
            cmd.CommandType = type;
            cmd.Connection = CreateConnection(); 

            if (null != parameters)
            {
                cmd.Parameters.AddRange(parameters);
            }

            cmd.Connection.Open();
            return cmd;
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbParameter" /> class.
        /// </summary>
        /// <returns>A new instance of <see cref="T:System.Data.Common.DbParameter" />.</returns>
        internal DbParameter CreateParameter(string n, object v, DbType type, int size, ParameterDirection direction)
        {
            var p = _provider.CreateParameter();
            if (p == null)
            {
                throw new SqlException($"Cannot create a sql parameter ({DbConfig.Name}.");
            }

            p.ParameterName = n;
            p.DbType = type;
            p.Value = v;
            p.Size = size;
            p.Direction = direction;
            p.SourceVersion = DataRowVersion.Current;
            p.SourceColumn = string.Empty;
            return p;
        }

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal T First<T>(int timeout, CommandType type, string sql, DbParameter[] parameters = null)
        {
            var instance = default(T);
            using (var cmd = CreateCommand(timeout, type, sql, parameters))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        instance = CreateInstanceFromReader<T>(reader);
                    }

                    reader.Close();
                }
                cmd.Connection.Close();
            }

            return instance;
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal List<T> Query<T>(int timeout, CommandType type, string sql, DbParameter[] parameters = null)
        {
            var re = new List<T>();
            using (var cmd = CreateCommand(timeout, type, sql, parameters))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.HasRows && reader.Read())
                    {
                        var instance = CreateInstanceFromReader<T>(reader);
                        re.Add(instance);
                    }

                    reader.Close();
                }
                cmd.Connection.Close();
            }

            return re;
        }

        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal T Value<T>(int timeout, CommandType type, string sql, DbParameter[] parameters = null)
        {
            try
            {
                var p = CreateParameter("@ReturnValue", null, DbType.String, 0, ParameterDirection.ReturnValue);
                if (p == null) return default(T);
                p.IsNullable = true;
                var dbParameters = new[] {p};
                if (parameters != null)
                {
                    dbParameters = dbParameters.Concat(parameters).ToArray();
                }

                Execute(timeout, type, sql, dbParameters);
                if (null == p.Value)
                {
                    return default(T);
                }

                return (T) Conversion.ConvertTo<T>(p.Value);
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sql, parameters);
            }
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal int Execute(int timeout, CommandType type, string sql, DbParameter[] parameters = null)
        {
            var r = Execute(timeout, type, sql, new List<DbParameter[]> {parameters});
            return r.Length > 0 ? r[0] : 0;
            /*try
            {
                using (var cmd = CreateCommand(timeout, type, sql, parameters))
                {
                    var r = cmd.ExecuteNonQuery();
                    return r;
                }
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sql, parameters);
            }
            finally
            {
                QueryCompleted();
            }*/
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal int[] Execute(int timeout, CommandType type, string sql, List<DbParameter[]> parameters)
        {
            var returnValue = new int[parameters.Count];
            DbParameter[] currentDbParameter = null;
            try
            {
                using (var cmd = CreateCommand(timeout, type, sql, null))
                {
                    var tmp = parameters.ToArray();
                    for (var index = 0; index < tmp.Length; index++)
                    {
                        cmd.Parameters.Clear();
                        if (null != tmp[index])
                        {
                            cmd.Parameters.AddRange(tmp[index]);
                            currentDbParameter = tmp[index];
                        }

                        var r = cmd.ExecuteNonQuery();
                        returnValue[index] = r;
                    }

                    cmd.Connection.Close();
                    return returnValue;
                }
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sql, currentDbParameter);
            }
        }

        /// <summary>
        ///  Execute SQL and return an <see cref="T"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal T One<T>(int timeout, CommandType type, string sql, DbParameter[] parameters = null)
        {
            try
            {
                using (var cmd = CreateCommand(timeout, type, sql, parameters))
                {
                    var result = cmd.ExecuteScalar();
                    if (null == result)
                    {
                        return default(T);
                    }

                    cmd.Connection.Close();
                    return (T) Conversion.ConvertTo<T>(result);
                }
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sql, parameters);
            }
        }

        /// <summary>
        ///  Execute SQL and return an <see cref="System.Data.DataTable"/>..
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal DataTable Table(int timeout, CommandType type, string sql, DbParameter[] parameters = null)
        {
            try
            {
                using (var t = new DataTable {Locale = Section.Get.Common.Culture})
                {
                    using (var adapter = CreateDataAdapter())
                    {
                        using (var cmd = CreateCommand(timeout, type, sql, parameters))
                        {
                            adapter.SelectCommand = cmd;
                            adapter.Fill(t);
                            cmd.Connection.Close();
                            return t;
                        }
                    }
                }
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sql, parameters);
            }
        }

        /// <summary>
        ///  Execute SQL and return an <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal DataSet DataSet(int timeout, CommandType type, string sql, DbParameter[] parameters = null)
        {
            try
            {
                using (var ds = new DataSet {Locale = Section.Get.Common.Culture})
                {
                    using (var adapter = CreateDataAdapter())
                    {
                        using (var cmd = CreateCommand(timeout, type, sql, parameters))
                        {
                            adapter.SelectCommand = cmd;
                            adapter.Fill(ds);
                            cmd.Connection.Close();
                            return ds;
                        }
                    }
                }
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sql, parameters);
            }
        }

        private SqlException SqlException(Exception sqlEx, string sql,
            IReadOnlyCollection<DbParameter> parameters = null)
        {
            var errStr =
                $"Source = {DbConfig.Name}\nCmd = {sql}\nParam = {PrintDbParameters(parameters)}\n{sqlEx.Message}\n";
            return new SqlException(errStr, sqlEx);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            _provider = null;
            _connSetting = null;
            DbConfig = null;
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}