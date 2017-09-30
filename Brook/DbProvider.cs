using jIAnSoft.Framework.Brook.Configuration;
using jIAnSoft.Framework.Brook.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;

namespace jIAnSoft.Framework.Brook
{
    public abstract class DbProvider : ProviderBase, IDisposable
    {
        protected bool Disposed;

        /// <summary>
        /// 連線逾時時間限制
        /// </summary>
        private int _intTimeout = 300;

        [DefaultValue(30)]
        private int Timeout
        {
            get => _intTimeout;
            set => _intTimeout = value < 0 ? 300 : value;
        }

        /// <summary>
        /// 
        /// </summary>
        private DbProviderFactory _provider;

        /// <summary>
        /// 
        /// </summary>
        protected ConnectionStringSettings DbConfig;

        /// <summary>
        /// 資料庫連線資源
        /// </summary>
        protected DbConnection Conn { get; set; }

        private DatabaseSet DbSet { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// 存放查詢時的參數
        /// </summary>

        protected DbProvider(string argStrDbProviderName)
        {
            InitDbProvider(InitDbConfig(argStrDbProviderName));
        }

        protected DbProvider(ConnectionStringSettings argDbConfig)
        {
            InitDbProvider(argDbConfig);
        }

        /// <summary>
        /// Initial DbConfig 
        /// </summary>
        protected ConnectionStringSettings InitDbConfig(string argStrDbProviderName)
        {
            DbSet = Section.Get.Database.Which[argStrDbProviderName];
            return new ConnectionStringSettings
            {
                ConnectionString = DbSet.Connection,
                ProviderName = DbSet.ProviderName,
                Name = DbSet.Name
            };
        }

        /// <summary>
        /// Initial Db connect
        /// </summary>
        /// <param name="argStrDbProviderName"></param>
        private void InitDbProvider(string argStrDbProviderName)
        {
            InitDbProvider(InitDbConfig(argStrDbProviderName));
        }

        /// <summary>
        /// Initial Db connect
        /// </summary>
        /// <param name="argConfig"></param>
        private void InitDbProvider(ConnectionStringSettings argConfig)
        {
            DbConfig = argConfig;
            _provider = DbProviderFactories.GetFactory(DbConfig.ProviderName);
            Timeout = DbSet.CommandTimeOut;
        }

        /// <summary>
        /// 取回指定資料庫的資料庫連線物件
        /// </summary>
        /// <param name="argStrDbProviderName"></param>
        /// <returns></returns>
        public DbConnection GetNewSqlConnection(string argStrDbProviderName)
        {
            InitDbProvider(argStrDbProviderName);
            return GetConnection;
        }

        /*
        /// <summary>
        /// 取回目前連線的資料庫連線物件
        /// </summary>
        /// <returns></returns>
        public DbConnection GetSqlConnection()
        {
            return Conn;
        }
        */

        /// <summary>
        /// 目前連線的資料庫位置
        /// </summary>
        public string ConnectionSource => DbConfig.ConnectionString;

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
                        $"Cannot connect to specified sql server({DbConfig.Name} => {DbConfig.ConnectionString})."
                    ));
                con.ConnectionString = DbConfig.ConnectionString;
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
                t[i] = $"{{Key:'{p.ParameterName}',Val:'{p.Value.ToString().Replace("\"", "\\\"")}'}}";
                i++;
            }
            return $"[{string.Join(" ,", t)}]";
        }

        /// <summary>
        /// 當查詢結束後進行的動作
        /// </summary>
        private void QueryCompleted()
        {
            //移除參數設定
            Conn?.Close();
            //Command?.Dispose();
        }

        private DbCommand GetCommand(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters)
        {
            Conn = GetConnection;
            var command = Conn.CreateCommand();
            command.CommandTimeout = timeout;
            command.CommandText = sqlCmd;
            command.CommandType = commandType;
            if (null != parameters)
            {
                command.Parameters.AddRange(parameters);
            }
            return command;
        }


        /// <summary>
        ///  Execute SQL and return a DataTable <see cref="DataTable"/>.
        /// </summary>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public DataTable Table(string sqlCmd, DbParameter[] parameters = null)
        {
            return Table(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a DataTable <see cref="DataTable"/>.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        /// <returns></returns>
        public DataTable Table(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Table(Timeout, commandType, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a DataTable <see cref="DataTable"/>.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public DataTable Table(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                using (var adapter = _provider.CreateDataAdapter())
                {
                    if (adapter == null) return new DataTable();
                    adapter.SelectCommand = GetCommand(timeout, commandType, sqlCmd, parameters);
                    using (var ds = new DataSet {Locale = Section.Get.Common.Culture})
                    {
                        adapter.Fill(ds);
                        var t = ds.Tables[0];
                        return t;
                    }
                }
            }
            catch (Exception sqlEx)
            {
                throw new SqlException(
                    string.Format(
                        new CultureInfo(Section.Get.Common.Culture.Name),
                        "{0} Source = {1}\n Cmd = {2}\n Param = {3}",
                        sqlEx.Message,
                        DbConfig.Name,
                        sqlCmd,
                        PrintDbParameters(parameters)),
                    sqlEx);
            }
            finally
            {
                QueryCompleted();
            }
        }

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T First<T>(string sqlCmd, DbParameter[] parameters = null)
        {
            return First<T>(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T First<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return First<T>(Timeout, commandType, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T First<T>(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            var classobj = default(T);
            using (var reader = Reader(timeout, commandType, sqlCmd, parameters))
            {
                if (reader.Read())
                {
                    classobj = Activator.CreateInstance<T>();
                    for (var i = reader.FieldCount - 1; i >= 0; i--)
                    {
                        ReflectionHelpers.SetValue(classobj, reader.GetName(i), reader.GetValue(i));
                    }
                }
                reader.Dispose();
                QueryCompleted();
            }
            return classobj;
        }


        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>

        public T[] Query<T>(string sqlCmd, DbParameter[] parameters = null)
        {
            return Query<T>(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T[] Query<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Query<T>(Timeout, commandType, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T[] Query<T>(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            var re = new List<T>();
            using (var reader = Reader(timeout, commandType, sqlCmd, parameters))
            {
                while (reader.Read())
                {
                    var classobj = Activator.CreateInstance<T>();
                    for (var i = reader.FieldCount - 1; i >= 0; i--)
                    {
                        ReflectionHelpers.SetValue(classobj, reader.GetName(i), reader.GetValue(i));
                    }
                    re.Add(classobj);
                }
                reader.Close();
                QueryCompleted();
            }
            return re.ToArray();
        }

        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="sqlCmd">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T Value<T>(string sqlCmd, DbParameter[] parameters = null)
        {
            return Value<T>(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="sqlCmd">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <param name="argEmuCommandType">SQL command type SP、Text</param>
        /// <returns></returns>
        public T Value<T>(CommandType argEmuCommandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Value<T>(Timeout, argEmuCommandType, sqlCmd, parameters);
        }

        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="timeOut">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T Value<T>(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                //準備接收回傳值的參數
                var parame = _provider.CreateParameter();
                if (parame == null) return default(T);
                parame.ParameterName = "@ReturnValue";
                parame.DbType = DbType.String;
                parame.Direction = ParameterDirection.ReturnValue;
                parame.IsNullable = true;
                parame.SourceColumn = string.Empty;
                parame.SourceVersion = DataRowVersion.Default;
                var parames = new List<DbParameter> {parame};
                if (parameters != null)
                {
                    parames.AddRange(parameters);
                }
                Execute(timeOut, commandType, sqlCmd, parames.ToArray());
                if (null == parame.Value)
                {
                    return default(T);
                }
                return (T) Conversion.ConvertTo<T>(parame.Value);
            }
            catch (Exception sqlEx)
            {
                throw new SqlException(
                    string.Format(
                        new CultureInfo(Section.Get.Common.Culture.Name),
                        "{0} Source = {1}\n Cmd = {2}\n Param = {3}",
                        sqlEx.Message,
                        DbConfig.Name,
                        sqlCmd,
                        PrintDbParameters(parameters)),
                    sqlEx);
            }
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public int Execute(string sqlCmd, DbParameter[] parameters = null)
        {
            return Execute(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="sqlCmd">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <returns></returns>
        public int Execute(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Execute(Timeout, commandType, sqlCmd, parameters);
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
                return GetCommand(timeout, commandType, sqlCmd, parameters).ExecuteNonQuery();
            }
            catch (Exception sqlEx)
            {
                throw new SqlException(
                    string.Format(
                        new CultureInfo(Section.Get.Common.Culture.Name),
                        "{0} Source = {1}\n Cmd = {2}\n Param = {3}",
                        sqlEx.Message,
                        DbConfig.Name,
                        sqlCmd,
                        PrintDbParameters(parameters)),
                    sqlEx);
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
                return GetCommand(timeout, commandType, sqlCmd, parameters).ExecuteReader();
            }
            catch (Exception sqlEx)
            {
                throw new SqlException(
                    string.Format(
                        new CultureInfo(Section.Get.Common.Culture.Name),
                        "{0} Source = {1}\n Cmd = {2}\n Param = {3}",
                        sqlEx.Message,
                        DbConfig.Name,
                        sqlCmd,
                        PrintDbParameters(parameters)),
                    sqlEx);
            }
        }

        /// <summary>
        /// 執行查詢後回傳第一列第一欄的的資料 V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public T One<T>(string sqlCmd, DbParameter[] parameters = null)
        {
            return One<T>(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行查詢後回傳第一列第一欄的的資料 V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <returns></returns>
        public T One<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return One<T>(Timeout, commandType, sqlCmd, parameters);
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
            var result = GetCommand(timeout, commandType, sqlCmd, parameters).ExecuteScalar();
            if (null == result)
            {
                return default(T);
            }
            return (T) Conversion.ConvertTo<T>(result);
        }

        /// <summary>
        /// Execute SQL and return an <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public DataSet DataSet(string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(CommandType.Text, sqlCmd, parameters);
        }


        /// <summary>
        ///  Execute SQL and return an <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public DataSet DataSet(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(Timeout, commandType, sqlCmd, parameters);
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
                    if (adapter == null) return new DataSet();
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
                throw new SqlException(
                    string.Format(
                        new CultureInfo(Section.Get.Common.Culture.Name),
                        "{0} Source = {1}\n Cmd = {2}\n Param = {3}",
                        sqlEx.Message,
                        DbConfig.Name,
                        sqlCmd,
                        PrintDbParameters(parameters)),
                    sqlEx);
            }
            finally
            {
                QueryCompleted();
            }
        }

        public abstract void Dispose();
    }
}