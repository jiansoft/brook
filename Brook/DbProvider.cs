using jIAnSoft.Framework.Brook.Configuration;
using jIAnSoft.Framework.Brook.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;

namespace jIAnSoft.Framework.Brook
{
    public class DbProvider : ProviderBase, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Db factory
        /// </summary>
        private DbProviderFactory _provider;

        /// <summary>
        /// 
        /// </summary>
        protected ConnectionStringSettings ConnectionSetting;

        /// <summary>
        /// Connection resource
        /// </summary>
        private DbConnection Conn { get; set; }

        /// <summary>
        /// Db configuration
        /// </summary>
        private DatabaseConfiguration DbConfiguration { get; set; }

        public DbProvider(string argStrDbProviderName)
        {
            InitDbProvider(InitConnectionStringSetting(argStrDbProviderName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argStrHost"></param>
        /// <param name="argIntPort"></param>
        /// <param name="argStrUser"></param>
        /// <param name="argStrPassword"></param>
        /// <param name="argStrDbName"></param>
        /// <param name="providerName"></param>
        public DbProvider(string argStrHost, int argIntPort, string argStrDbName, string argStrUser,
            string argStrPassword, string providerName)
            : this(new ConnectionStringSettings
            {
                ConnectionString =
                    $"server={argStrHost},{argIntPort};database={argStrDbName};uid={argStrUser};pwd={argStrPassword};",
                ProviderName = providerName,
                Name = argStrDbName
            })
        {
        }

        internal DbProvider(ConnectionStringSettings argDbConfig)
        {
            InitDbProvider(argDbConfig);
        }

        /// <summary>
        /// Initial ConnectionSetting 
        /// </summary>
        protected ConnectionStringSettings InitConnectionStringSetting(string argStrDbProviderName)
        {
            DbConfiguration = Section.Get.Database.Which[argStrDbProviderName];
            return new ConnectionStringSettings
            {
                ConnectionString = DbConfiguration.Connection,
                ProviderName = DbConfiguration.ProviderName,
                Name = DbConfiguration.Name
            };
        }

        /*
        /// <summary>
        /// Initial Db connect
        /// </summary>
        /// <param name="argStrDbProviderName"></param>
        private void InitDbProvider(string argStrDbProviderName)
        {
            InitDbProvider(InitConnectionStringSetting(argStrDbProviderName));
        }
        */
        /// <summary>
        /// Initial Db connect
        /// </summary>
        /// <param name="argConfig"></param>
        private void InitDbProvider(ConnectionStringSettings argConfig)
        {
            ConnectionSetting = argConfig;
#if NET451
            _provider = System.Data.Common.DbProviderFactories.GetFactory(ConnectionSetting.ProviderName);
#elif NETSTANDARD2_0

            _provider = DbProviderFactories.GetFactory(ConnectionSetting.ProviderName);
#endif
            //Timeout = DbConfiguration.CommandTimeOut;
        }
        /*
        /// <summary>
        ///Get new SQL connection
        /// </summary>
        /// <param name="argStrDbProviderName"></param>
        /// <returns></returns>
        public DbConnection GetNewSqlConnection(string argStrDbProviderName)
        {
            InitDbProvider(argStrDbProviderName);
            return GetConnection;
        }*/

        /// <summary>
        /// 目前連線的資料庫位置
        /// </summary>
        public string ConnectionSource => ConnectionSetting.ConnectionString;

        /// <summary>
        /// 取得資料庫連線
        /// </summary>
        private DbConnection GetConnection
        {
            get
            {
                var con = _provider.CreateConnection();
                if (con == null)
                    throw new SqlException(string.Format(
                        new CultureInfo(Section.Get.Common.Culture.Name),
                        $"Cannot connect to specified sql server({ConnectionSetting.Name} => {ConnectionSetting.ConnectionString})."
                    ));
                con.ConnectionString = ConnectionSetting.ConnectionString;
                con.Open();
                return con;
            }
        }

        /// <summary>
        /// Escapes a string for use in a fulltext query
        /// </summary>
        /// <param name="argStrValue"></param>
        /// <returns></returns>
        public string EscapeStringFulltext(string argStrValue)
        {
            if (argStrValue == null)
                throw new ArgumentNullException(nameof(argStrValue));
            return
                EscapeString(Regex.Replace(argStrValue, @"""*\(*\)*\!*\[*\]*\.*\{*\}*\~*\s*", "",
                    RegexOptions.IgnoreCase));
        }

        /// <summary>
        /// Escapes a string for use in a query
        ///  <param name="argStrValue">SQL指令.</param>
        /// </summary>
        public static string EscapeString(string argStrValue)
        {
            if (argStrValue == null)
                throw new ArgumentNullException(nameof(argStrValue));

            return argStrValue.Replace("'", "''");
        }

        private static string PrintDbParameters(IReadOnlyCollection<DbParameter> parameters)
        {
            if (null == parameters) return string.Empty;
            var t = new string[parameters.Count];
            var i = 0;
            //[{ Key: "@MatchID",Val: "12199414"}]
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
        private void QueryCompleted()
        {
            Conn?.Close();
        }

        private DbCommand GetCommand(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters)
        {
            Conn = GetConnection;
            var cmd = Conn.CreateCommand();
            cmd.CommandTimeout = timeout;
            cmd.CommandText = sqlCmd;
            cmd.CommandType = commandType;
            if (null != parameters)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd;
        }

        /*
        /// <summary>
        ///  Execute SQL and return a DataTable <see cref="DataTable"/>.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal DataTable Table(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(timeout, commandType, sqlCmd, parameters).Tables[0];
        }
        */

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal T First<T>(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            var instance = default(T);
            using (var reader = Reader(timeout, commandType, sqlCmd, parameters))
            {
                if (reader.Read())
                {
                    instance = Activator.CreateInstance<T>();
                    for (var i = reader.FieldCount - 1; i >= 0; i--)
                    {
                        ReflectionHelpers.SetValue(instance, reader.GetName(i), reader.GetValue(i));
                    }
                }

                reader.Close();
                QueryCompleted();
            }

            return instance;
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal List<T> Query<T>(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            var re = new List<T>();
            using (var reader = Reader(timeout, commandType, sqlCmd, parameters))
            {
                while (reader.Read())
                {
                    var instance = Activator.CreateInstance<T>();
                    for (var i = reader.FieldCount - 1; i >= 0; i--)
                    {
                        ReflectionHelpers.SetValue(instance, reader.GetName(i), reader.GetValue(i));
                    }

                    re.Add(instance);
                    //                    for (var i = reader.FieldCount - 1; i >= 0; i--)
                    //                    {
                    //                        if (reader.GetValue(i) is T variable)
                    //                        {
                    //                            re.Add(variable);
                    //                            continue;
                    //                        }
                    //                        if (typeof(T).IsPrimitive())
                    //                        {
                    //                            re.Add((T) Convert.ChangeType(reader.GetValue(i), typeof(T)));
                    //                            continue;
                    //                        }
                    //                        if (!typeof(T).IsConstructedGenericType())
                    //                        {
                    //                            re.Add((T) reader.GetValue(i));
                    //                            continue;
                    //                        }
                    //                        if (typeof(T).IsNullable())
                    //                        {
                    //                            var type = typeof(T).GetGenericTypeArguments()[0];
                    //                            if (type.IsPrimitive())
                    //                            {
                    //                                re.Add((T) Convert.ChangeType(reader.GetValue(i), type));
                    //                                continue;
                    //                            }
                    //                        }
                    //                        var classobj = Activator.CreateInstance<T>();
                    //                        ReflectionHelpers.SetValue(classobj, reader.GetName(i), reader.GetValue(i));
                    //                        re.Add(classobj);
                    //                    }
                }

                reader.Close();
                QueryCompleted();
            }

            return re;
        }

        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        internal T Value<T>(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                var dbParameter = _provider.CreateParameter();
                if (dbParameter == null) return default(T);
                dbParameter.ParameterName = "@ReturnValue";
                dbParameter.DbType = DbType.String;
                dbParameter.Direction = ParameterDirection.ReturnValue;
                dbParameter.IsNullable = true;
                dbParameter.SourceColumn = string.Empty;
                dbParameter.SourceVersion = DataRowVersion.Default;
                var dbParameters = new List<DbParameter> {dbParameter};
                if (parameters != null)
                {
                    dbParameters.AddRange(parameters);
                }

                Execute(timeout, commandType, sqlCmd, dbParameters.ToArray());
                if (null == dbParameter.Value)
                {
                    return default(T);
                }

                return (T) Conversion.ConvertTo<T>(dbParameter.Value);
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sqlCmd, parameters);
            }
            finally
            {
                QueryCompleted();
            }
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public int Execute(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                var cmd = GetCommand(timeout, commandType, sqlCmd, parameters);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sqlCmd, parameters);
            }
            finally
            {
                QueryCompleted();
            }
        }

        /// <summary>
        ///  Execute SQL and return an <see cref="System.Data.Common.DbDataReader"/>.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        private DbDataReader Reader(int timeout, CommandType commandType, string sqlCmd,
            DbParameter[] parameters = null)
        {
            try
            {
                var cmd = GetCommand(timeout, commandType, sqlCmd, parameters);
                return cmd.ExecuteReader();
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sqlCmd, parameters);
            }
        }

        /// <summary>
        ///  Execute SQL and return an <see cref="T"/>.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T One<T>(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                var cmd = GetCommand(timeout, commandType, sqlCmd, parameters);
                var result = cmd.ExecuteScalar();
                if (null == result)
                {
                    return default(T);
                }

                return (T) Conversion.ConvertTo<T>(result);
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sqlCmd, parameters);
            }
            finally
            {
                QueryCompleted();
            }
        }

        /// <summary>
        ///  Execute SQL and return an <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public DataSet DataSet(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                using (var adapter = _provider.CreateDataAdapter())
                {
                    if (adapter == null)
                    {
                        throw new SqlException(
                            "DbProviderFactory can't create a new instance of the provider's DataAdapter class.");
                    }

                    using (var ds = new DataSet {Locale = Section.Get.Common.Culture})
                    {
                        adapter.SelectCommand = GetCommand(timeout, commandType, sqlCmd, parameters);
                        adapter.Fill(ds);
                        return ds;
                    }
                }
            }
            catch (Exception sqlEx)
            {
                throw SqlException(sqlEx, sqlCmd, parameters);
            }
            finally
            {
                QueryCompleted();
            }
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="batchSize"></param>
        public void Bulk(DataTable table, int batchSize = 2500)
        {
            using (var bulk = new SqlBulkCopy(ConnectionSource))
            {
                bulk.BatchSize = batchSize;
                bulk.BulkCopyTimeout = 0;
                bulk.DestinationTableName = $"[dbo].[{table.TableName}]";
                foreach (DataColumn column in table.Columns)
                {
                    bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                try
                {
                    // Write from the source to the destination.
                    bulk.WriteToServer(table);
                }
                catch (Exception sqlEx)
                {
                    throw new SqlException(
                        string.Format(
                            new CultureInfo(Section.Get.Common.Culture.Name),
                            "{0} Source = {1}\n Table = {2}\n",
                            sqlEx.Message,
                            ConnectionSetting.Name,
                            table.TableName
                        ),
                        sqlEx);
                }
            }
        }*/

        private SqlException SqlException(Exception sqlEx, string sqlCmd,
            IReadOnlyCollection<DbParameter> parameters = null)
        {
            var errStr =
                $"{sqlEx.Message}\nSource = {ConnectionSetting.Name}\nCmd = {sqlCmd}\nParam = {PrintDbParameters(parameters)}";
            return new SqlException(errStr, sqlEx);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;

            if (null != Conn)
            {
                if (Conn.State != ConnectionState.Open)
                {
                    Conn.Close();
                }

                Conn.Dispose();
                Conn = null;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}