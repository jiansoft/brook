using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Brook.Configuration;
using Brook.Utility;

namespace Brook
{
    public sealed class MsSql : DbProvider
    {
        public MsSql(string argStrDbProviderName)
            : base(argStrDbProviderName)
        {
        }

        public MsSql(string argStrHost, string argStrDbName, string argStrUser, string argStrPassword)
            : this(argStrHost, 1433, argStrDbName, argStrUser, argStrPassword)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// 指定連線主機資訊
        /// </summary>
        /// <param name="argStrHost"></param>
        /// <param name="argIntPort"></param>
        /// <param name="argStrUser"></param>
        /// <param name="argStrPassword"></param>
        /// <param name="argStrDbName"></param>
        public MsSql(string argStrHost, int argIntPort, string argStrDbName, string argStrUser, string argStrPassword)
            : this(new ConnectionStringSettings
            {
                ConnectionString =
                    $"server={argStrHost},{argIntPort};database={argStrDbName};uid={argStrUser};pwd={argStrPassword};Connection Timeout=5;",
                ProviderName = "System.Data.SqlClient",
                Name = argStrDbName
            })
        {
        }

        /// <summary>
        /// 指定連線主機資訊
        /// </summary>
        /// <param name="argDbConfig"></param>
        public MsSql(ConnectionStringSettings argDbConfig)
            : base(argDbConfig)
        {
        }


        /// <summary>
        /// 變更連線的資料庫來源
        /// </summary>
        /// <param name="argStrDbProviderName"></param>
        public void ChangeConnection(string argStrDbProviderName)
        {
            InitDbConfig(argStrDbProviderName);
        }

        public void Dispose(bool disposing)
        {
            if (!disposing || Disposed) return;

            if (null != Conn)
            {
                Conn.Close();
                Conn.Dispose();
                Conn = null;
            }
            Disposed = true;
        }

        public override void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 執行查詢後只回傳第一列的資料 V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public T First<T>(string sqlCmd, DbParameter[] parameters = null)
        {
            return First<T>(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行查詢後只回傳第一列的資料 V2.0
        /// </summary>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public T First<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return First<T>(Timeout, commandType, sqlCmd, parameters);
        }
        
        /// <summary>
        /// 執行查詢後只回傳第一列的資料 V2.0
        /// </summary>
        /// <param name="timeOut">指令逾時時間</param>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <returns></returns>
        public T First<T>(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            var classobj = default(T);
            using (var reader = Reader(timeOut, commandType, sqlCmd, parameters))
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
        /// 執行SQL指令後回傳 DataTable V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public DataTable Table(string sqlCmd, DbParameter[] parameters = null)
        {
            return Table(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行SQL指令後回傳 DataTable V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <returns></returns>
        public DataTable Table(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Table(Timeout, commandType, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行SQL指令後回傳 DataTable V2.0
        /// </summary>
        /// <param name="timeOut">指令逾時時間</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>

        /// <returns></returns>
        public DataTable Table(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                using (var adapter = Provider.CreateDataAdapter())
                {
                    if (adapter == null) return new DataTable();
                    adapter.SelectCommand = GetCommand(timeOut, commandType, sqlCmd, parameters);
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


        public T[] Query<T>(string sqlCmd, DbParameter[] parameters = null)
        {
            return Query<T>(CommandType.Text, sqlCmd, parameters);
        }


        public T[] Query<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Query<T>(Timeout, commandType, sqlCmd, parameters);
        }

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
        /// 執行查詢後回傳資料庫的回傳值，預設模式 StoredProcedure
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public T Value<T>(string sqlCmd, DbParameter[] parameters = null)
        {
            return Value<T>(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行查詢後回傳資料庫的回傳值，預設模式 StoredProcedure
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <param name="argEmuCommandType">SQL 執行模式 SP、Text</param>
        /// <returns></returns>
        public T Value<T>(CommandType argEmuCommandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Value<T>(Timeout, argEmuCommandType, sqlCmd, parameters);
        }

        public T Value<T>(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                //準備接收回傳值的參數
                var parame = Provider.CreateParameter();
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
        /// 執行查詢後只回傳影響的資料筆數 V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public int Execute(string sqlCmd, DbParameter[] parameters = null)
        {
            return Execute(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行查詢後只回傳影響的資料筆數 V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <returns></returns>
        public int Execute(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Execute(Timeout, commandType, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行查詢後只回傳影響的資料筆數 V2.0
        /// </summary>
        /// <param name="timeOut">指令逾時時間</param>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <returns></returns>
        public int Execute(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                return GetCommand(timeOut, commandType, sqlCmd, parameters).ExecuteNonQuery();
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
        /// 執行SQL指令後回傳 DataReader V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        private DbDataReader Reader(string sqlCmd, DbParameter[] parameters = null)
        {
            return Reader(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行SQL指令後回傳 DataReader V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <returns></returns>
        private DbDataReader Reader(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return Reader(Timeout, commandType, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行SQL指令後回傳 DataReader V2.0
        /// </summary>
        /// <param name="timeOut">指令逾時時間</param>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <returns></returns>
        private DbDataReader Reader(int timeOut, CommandType commandType, string sqlCmd,
            DbParameter[] parameters = null)
        {
            try
            {
                return GetCommand(timeOut, commandType, sqlCmd, parameters).ExecuteReader();
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

        public T One<T>(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return (T) Conversion.ConvertTo<T>(GetCommand(timeOut, commandType, sqlCmd, parameters).ExecuteScalar());
        }

        /// <summary>
        /// 執行SQL指令後回傳 DataSet V2.0
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public DataSet DataSet(string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(CommandType.Text, sqlCmd, parameters);
        }


        /// <summary>
        /// 執行SQL指令後回傳 DataSet V2.0
        /// </summary>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public DataSet DataSet(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(Timeout, commandType, sqlCmd, parameters);
        }

        /// <summary>
        /// 執行SQL指令後回傳 DataSet V2.0
        /// </summary>
        /// <param name="timeOut">指令逾時時間</param>
        /// <param name="commandType">SQL 執行模式 SP、Text</param>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public DataSet DataSet(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            try
            {
                using (var adapter = Provider.CreateDataAdapter())
                {
                    if (adapter == null) return new DataSet();
                    using (var ds = new DataSet {Locale = Section.Get.Common.Culture})
                    {
                        adapter.SelectCommand = GetCommand(timeOut, commandType, sqlCmd, parameters);
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
    }
}
