using jIAnSoft.Brook.Configuration;
using jIAnSoft.Brook.Utility;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;


namespace jIAnSoft.Brook
{
    internal sealed class DbProvider : ProviderBase, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Db factory
        /// </summary>
        private DbProviderFactory _provider;

        /// <summary>
        /// DatabaseConfiguration
        /// </summary>
        internal DatabaseConfiguration DbConfig;

        private DbConnection _conn;

        private DbTransaction _transaction;

#if NETSTANDARD2_1 || NET5_0_OR_GREATER
        static DbProvider()
        {
            foreach (var (_, value) in Utility.DbProviderFactories.Providers)
            {
                System.Data.Common.DbProviderFactories.RegisterFactory(value.Invariant, value.Type);
            }
        }
#endif

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dbConfig"></param>
        internal DbProvider(DatabaseConfiguration dbConfig)
        {
#if NETSTANDARD2_0
            _provider = DbProviderFactories.GetFactory(dbConfig.ProviderName);
#else
            _provider = System.Data.Common.DbProviderFactories.GetFactory(dbConfig.ProviderName);
#endif
            if (_provider == null)
            {
                throw new SqlException(
                    $"Can`t create a specified server({dbConfig.Name} factory. Please check the ProviderName.");
            }

            DbConfig = dbConfig;
            _conn = CreateConnection();
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
                var key = p.ParameterName;
                var value = p.Value ?? "null";
                t[i] = $"{{Key:'{key}',Val:'{value.ToString()?.Replace("\"", "\\\"")}'}}";
                i++;
            }

            return $"[{string.Join(" ,", t)}]";
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
                throw new SqlException($"Can't create a new instance of the provider's Connection ({DbConfig.Name}).");
            }

            con.ConnectionString = DbConfig.Connection;
            return con;
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
                throw new SqlException($"Can't create a new instance of the provider's Command ({DbConfig.Name}).");
            }

            cmd.CommandType = type;
            cmd.Connection = _conn;
            cmd.CommandTimeout = timeout;
            cmd.CommandText = sql;

            if (null != parameters)
            {
                cmd.Parameters.AddRange(parameters);
            }

            if (_conn.State == ConnectionState.Closed)
            {
                _conn.Open();
            }

            if (null == _transaction)
            {
                return cmd;
            }

            cmd.Transaction = _transaction;

            return cmd;
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
                throw new SqlException($"Can't create a new instance of the provider's DataAdapter ({DbConfig.Name}).");
            }

            adapter =
                Assembly.Load("MySql.Data").CreateInstance("MySql.Data.MySqlClient.MySqlDataAdapter") as DbDataAdapter;
            if (adapter == null)
            {
                throw new SqlException($"Can't create a new instance of the provider's DataAdapter ({DbConfig.Name}).");
            }

            return adapter;
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
                throw new SqlException($"Can't create a new instance of the provider's parameter ({DbConfig.Name}.");
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
        /// Begins a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolation">The isolation level under which the transaction should run.</param>
        internal void Begin(IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            if (_conn == null)
            {
                _conn = CreateConnection();
            }

            if (_transaction != null)
            {
                return;
            }

            if (_conn.State == ConnectionState.Closed)
            {
                _conn.Open();
            }

            _transaction = _conn.BeginTransaction(isolation);
        }

        /// <summary>
        /// Commits the database transaction
        /// </summary>
        internal void Commit()
        {
            if (_transaction == null)
            {
                return;
            }

            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        /// <summary>
        /// Rolls back a transaction.
        /// </summary>
        internal void Rollback()
        {
            if (_transaction == null)
            {
                return;
            }

            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        /// <summary>
        /// Changes the current database for an open connection.
        /// </summary>
        /// <param name="database">The name of the database to use.</param>
        internal void Change(string database)
        {
            _conn.ChangeDatabase(database);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal List<T> Query<T>(int timeout, CommandType type, string sql, DbParameter[] parameters)
        {
            var re = new List<T>();
            DbCommand cmd = null;

            try
            {
                cmd = CreateCommand(timeout, type, sql, parameters);
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (r.HasRows && r.Read())
                    {
                        var instance = ReflectionHelpers.ConvertAs<T>(r);
                        re.Add(instance);
                    }

                    r.Close();
                }
            }
            catch (Exception e)
            {
                throw SqlException(e, sql, parameters);
            }
            finally
            {
                cmd?.Dispose();
                if (null == _transaction)
                {
                    if (_conn.State == ConnectionState.Open)
                    {
                        _conn.Close();
                    }
                }
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
        internal T Value<T>(int timeout, CommandType type, string sql, DbParameter[] parameters)
        {
            try
            {
                var p = CreateParameter("@ReturnValue", null, DbType.String, 0, ParameterDirection.ReturnValue);
                if (p == null)
                {
                    return default;
                }

                p.IsNullable = true;
                var dbParameters = new[] {p};
                if (parameters != null)
                {
                    dbParameters = dbParameters.Concat(parameters).ToArray();
                }

                Execute(timeout, type, sql, new[] {dbParameters});

                if (null == p.Value)
                {
                    return default;
                }

                return (T) Conversion.ConvertTo<T>(p.Value);
            }
            catch (Exception e)
            {
                throw SqlException(e, sql, parameters);
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
        internal T One<T>(int timeout, CommandType type, string sql, DbParameter[] parameters)
        {
            DbCommand cmd = null;

            try
            {
                cmd = CreateCommand(timeout, type, sql, parameters);
                var result = cmd.ExecuteScalar();
                return null == result ? default : (T) Conversion.ConvertTo<T>(result);
            }
            catch (Exception e)
            {
                throw SqlException(e, sql, parameters);
            }
            finally
            {
                cmd?.Dispose();
                if (null == _transaction)
                {
                    if (_conn.State == ConnectionState.Open)
                    {
                        _conn.Close();
                    }
                }
            }
        }

        /// <summary>
        ///  Execute SQL and return an <see cref="System.Data.DataTable"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal DataTable Table(int timeout, CommandType type, string sql, DbParameter[] parameters)
        {
            DbCommand cmd = null;

            try
            {
                cmd = CreateCommand(timeout, type, sql, parameters);
                using (var adapter = CreateDataAdapter())
                {
                    adapter.SelectCommand = cmd;
                    using (var t = new DataTable {Locale = Section.Get.Common.Culture})
                    {
                        adapter.Fill(t);
                        return t;
                    }
                }
            }
            catch (Exception e)
            {
                throw SqlException(e, sql, parameters);
            }
            finally
            {
                cmd?.Dispose();
                if (null == _transaction)
                {
                    if (_conn.State == ConnectionState.Open)
                    {
                        _conn.Close();
                    }
                }
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
        internal DataSet DataSet(int timeout, CommandType type, string sql, DbParameter[] parameters)
        {
            DbCommand cmd = null;

            try
            {
                cmd = CreateCommand(timeout, type, sql, parameters);
                using (var adapter = CreateDataAdapter())
                {
                    adapter.SelectCommand = cmd;
                    using (var ds = new DataSet {Locale = Section.Get.Common.Culture})
                    {
                        adapter.Fill(ds);
                        return ds;
                    }
                }
            }
            catch (Exception e)
            {
                throw SqlException(e, sql, parameters);
            }
            finally
            {
                cmd?.Dispose();
                if (null == _transaction)
                {
                    if (_conn.State == ConnectionState.Open)
                    {
                        _conn.Close();
                    }
                }
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
        internal int[] Execute(int timeout, CommandType type, string sql, DbParameter[][] parameters)
        {
            var parameterIsNull = parameters == null;
            var returnValue = new int[parameterIsNull ? 1 : parameters.Length];
            DbParameter[] currentDbParameter = null;
            DbCommand cmd = null;

            try
            {
                cmd = CreateCommand(timeout, type, sql, null);
                if (parameterIsNull)
                {
                    var r = cmd.ExecuteNonQuery();
                    returnValue[0] = r;
                }
                else
                {
                    for (var index = 0; index < parameters.Length; index++)
                    {
                        cmd.Parameters.Clear();
                        if (null != parameters[index] && parameters[index].Any())
                        {
                            cmd.Parameters.AddRange(parameters[index]);
                            currentDbParameter = parameters[index];
                        }

                        var r = cmd.ExecuteNonQuery();
                        returnValue[index] = r;
                    }
                }
            }
            catch (Exception e)
            {
                throw SqlException(e, sql, currentDbParameter);
            }
            finally
            {
                cmd?.Dispose();
                if (null == _transaction)
                {
                    if (_conn.State == ConnectionState.Open)
                    {
                        _conn.Close();
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal T First<T>(int timeout, CommandType type, string sql, DbParameter[] parameters)
        {
            var instance = default(T);
            DbCommand cmd = null;

            try
            {
                cmd = CreateCommand(timeout, type, sql, parameters);
                using (var r = cmd.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow))
                {
                    if (r.HasRows && r.Read())
                    {
                        instance = ReflectionHelpers.ConvertAs<T>(r);
                    }

                    r.Close();
                }
            }
            catch (Exception e)
            {
                throw SqlException(e, sql, parameters);
            }
            finally
            {
                cmd?.Dispose();
                //無交易不論有無異常關連線
                if (null == _transaction)
                {
                    if (_conn.State == ConnectionState.Open)
                    {
                        _conn.Close();
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Wrap a transaction operation 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="type"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="isolation"></param>
        /// <returns></returns>
        public QueryResult Transaction(int timeout, CommandType type, string sql, DbParameter[][] parameters,
            IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            try
            {
                Begin(isolation);

                Execute(timeout, type, sql, parameters);

                Commit();

                return new QueryResult {Ok = true};
            }
            catch (Exception e)
            {
                Rollback();

                return new QueryResult {Ok = false, Err = SqlException(e, sql, parameters)};
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
            }
        }

        private SqlException SqlException(Exception ex, string sql, IReadOnlyList<DbParameter[]> parameters)
        {
            string[] p;

            if (null == parameters)
            {
                p = Array.Empty<string>();
            }
            else
            {
                p = new string[parameters.Count];

                for (var index = 0; index < parameters.Count; index++)
                {
                    var parameter = parameters[index];
                    p[index] = PrintDbParameters(parameter);
                }
            }

            var errStr = $"Source = {DbConfig.Connection}\nCmd = {sql}\nParam = {string.Join(",", p)}\nMessage = {ex}";
            return new SqlException(errStr, ex);
        }

        private SqlException SqlException(Exception ex, string sql, DbParameter[] parameters = null)
        {
            return SqlException(ex, sql, new[] {parameters});
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            _disposed = true;

            if (null != _transaction)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (null != _conn)
            {
                _conn.Close();
                _conn.Dispose();
                _conn = null;
            }

            _provider = null;
            DbConfig = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}