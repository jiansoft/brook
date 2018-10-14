using jIAnSoft.Brook.Configuration;
using jIAnSoft.Brook.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Globalization;

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
        private readonly ConnectionStringSettings _connStringSetting;

        /// <summary>
        /// 
        /// </summary>
        internal readonly DatabaseConfiguration DbConfiguration;

        /// <summary>
        /// Connection resource
        /// </summary>
        private DbConnection Conn { get; set; }

        /// <inheritdoc />
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
        }

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
            DbConfiguration = dbConfig;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="connStringStringSettings"></param>
        private DbProvider(ConnectionStringSettings connStringStringSettings)
        {
            _connStringSetting = connStringStringSettings;

#if NET451
            _provider = System.Data.Common.DbProviderFactories.GetFactory(_connStringSetting.ProviderName);
#elif NETSTANDARD2_0

            _provider = DbProviderFactories.GetFactory(_connStringSetting.ProviderName);
#endif
        }

        /// <summary>
        /// 目前連線的資料庫位置
        /// </summary>
        public string ConnectionSource => _connStringSetting.ConnectionString;

        /// <summary>
        /// 取得資料庫連線
        /// </summary>
        private DbConnection CreateConnection
        {
            get
            {
                var con = _provider.CreateConnection();
                if (con == null)
                    throw new SqlException(string.Format(
                        new CultureInfo(Section.Get.Common.Culture.Name),
                        $"Cannot connect to specified sql server({_connStringSetting.Name} => {_connStringSetting.ConnectionString})."
                    ));
                con.ConnectionString = _connStringSetting.ConnectionString;
                con.Open();
                return con;
            }
        }


        /*
        /// <summary>
        /// Escapes a string for use in a fulltext query
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        public string EscapeStringFulltext(string sourceValue)
        {
            if (sourceValue == null)
                throw new ArgumentNullException(nameof(sourceValue));
            return
                EscapeString(Regex.Replace(sourceValue, @"""*\(*\)*\!*\[*\]*\.*\{*\}*\~*\s*", "",
                    RegexOptions.IgnoreCase));
        }
       

        /// <summary>
        /// Escapes a string for use in a query
        ///  <param name="argStrValue">SQL指令.</param>
        /// </summary>
        public static string EscapeString(string sourceValue)
        {
            if (sourceValue == null)
                throw new ArgumentNullException(nameof(sourceValue));

            return sourceValue.Replace("'", "''");
        }
         */

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

        private DbCommand CreateCommand(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters)
        {
            Conn = CreateConnection;
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

        internal DbParameter CreateParameter(string name, object value, DbType dbType, int size,
            ParameterDirection direction)
        {
            var p = _provider.CreateParameter();
            if (p == null)
                throw new SqlException(string.Format(
                    new CultureInfo(Section.Get.Common.Culture.Name),
                    $"Cannot create a sql parameter ({_connStringSetting.Name}."
                ));
            p.ParameterName = name;
            p.DbType = dbType;
            p.Value = value;
            p.Size = size;
            p.Direction = direction;
            p.SourceVersion = DataRowVersion.Current;
            p.SourceColumn = string.Empty;
            return p;
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
                var dbParameter =
                    CreateParameter("@ReturnValue", null, DbType.String, 0, ParameterDirection.ReturnValue);
                if (dbParameter == null) return default(T);
                //dbParameter.ParameterName = "@ReturnValue";
                //dbParameter.DbType = DbType.String;
                //dbParameter.Direction = ParameterDirection.ReturnValue;
                dbParameter.IsNullable = true;
                // dbParameter.SourceColumn = string.Empty;
                // dbParameter.SourceVersion = DataRowVersion.Default;
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
                var cmd = CreateCommand(timeout, commandType, sqlCmd, parameters);
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
                var cmd = CreateCommand(timeout, commandType, sqlCmd, parameters);
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
                var cmd = CreateCommand(timeout, commandType, sqlCmd, parameters);
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
                        adapter.SelectCommand = CreateCommand(timeout, commandType, sqlCmd, parameters);
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
                            ConnStringSetting.Name,
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
                $"{sqlEx.Message}\nSource = {_connStringSetting.Name}\nCmd = {sqlCmd}\nParam = {PrintDbParameters(parameters)}";
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
            _provider = null;
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}