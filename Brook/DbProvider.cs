using System;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using jIAnSoft.Framework.Brook.Configuration;

namespace jIAnSoft.Framework.Brook
{
    public abstract class DbProvider : ProviderBase, IDisposable
    {
        protected bool Disposed;
       
        /// <summary>
        /// 連線逾時時間限制
        /// </summary>
        private int _intTimeout = 300;

        [DefaultValue(600)]
        protected int Timeout
        {
            get => _intTimeout;
            set => _intTimeout = value < 0 ? 0 : value;
        }

        /// <summary>
        /// 
        /// </summary>
        protected DbProviderFactory Provider;

        /// <summary>
        /// 
        /// </summary>
        protected ConnectionStringSettings DbConfig;

        /// <summary>
        /// 資料庫連線資源
        /// </summary>
        protected DbConnection Conn { get; set; }

        /// <summary>
        /// 存放查詢時的參數
        /// </summary>
       // private DbParameter[] DbParameters;
        protected DbProvider(string argStrDbProviderName)
            : this(InitDbConfig(argStrDbProviderName))
        {
        }

        protected DbProvider(ConnectionStringSettings argDbConfig)
        {
            InitDbProvider(argDbConfig);
        }

        /// <summary>
        /// 初始化 DbConfig 屬性
        /// </summary>
        protected static ConnectionStringSettings InitDbConfig(string argStrDbProviderName)
        {
            var provider = Section.Get.Database.Which[argStrDbProviderName];
            return new ConnectionStringSettings
            {
                ConnectionString = provider.Connection,
                ProviderName = provider.ProviderName,
                Name = provider.Name
            };
        }

        /// <summary>
        /// 初始化資料庫連線
        /// </summary>
        /// <param name="argStrDbProviderName"></param>
        protected void InitDbProvider(string argStrDbProviderName)
        {
            InitDbProvider(InitDbConfig(argStrDbProviderName));
        }

        /// <summary>
        /// 初始化資料庫連線
        /// </summary>
        /// <param name="argConfig"></param>
        protected void InitDbProvider(ConnectionStringSettings argConfig)
        {
            DbConfig = argConfig;
            Provider = DbProviderFactories.GetFactory(DbConfig.ProviderName);
            Timeout = Section.Get.Database.CommandTimeOut;
        }

        /// <summary>
        /// 取回指定資料庫的資料庫連線物件
        /// </summary>
        /// <param name="argStrDbProviderName"></param>
        /// <returns></returns>
        public DbConnection GetSqlConnection(string argStrDbProviderName)
        {
            InitDbProvider(argStrDbProviderName);
            return GetConnection;
        }

        /// <summary>
        /// 取回目前連線的資料庫連線物件
        /// </summary>
        /// <returns></returns>
        public DbConnection GetSqlConnection()
        {
            return Conn;
        }

        /// <summary>
        /// 目前連線的資料庫位置
        /// </summary>
        public string ConnectionSource => DbConfig.ConnectionString;


        /// <summary>
        /// 取得資料庫連線
        /// </summary>
        protected DbConnection GetConnection
        {
            get
            {
                var con = Provider.CreateConnection();
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

        protected static string PrintDbParameters(DbParameter[] parameters)
        {
            if (null == parameters) return string.Empty;
            var t = new string[parameters.Length];
            var i = 0;
            foreach (var p in parameters)
            {
                t[i] = $"@{p.ParameterName}={p.Value}";
                i++;
            }
            return string.Join(" , ", t);
        }

        /// <summary>
        /// 當查詢結束後進行的動作
        /// </summary>
        protected void QueryCompleted()
        {
            //移除參數設定
            Conn?.Close();
            //Command?.Dispose();
        }

      
        /*
        /// <summary>
        /// 設定Cammand物件開啟資料庫連線準備進行查詢
        /// </summary>
        /// <param name="timeOut">指令逾時時間</param>
        /// <param name="commandType">SQL 執行模式</param>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        protected void SetCommand(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters)
        {
            //使用datareader 時需要close conn 
            if (Conn == null || Conn.State == ConnectionState.Closed)
            {
                Conn = GetConnection;
            }

            Command = Conn.CreateCommand();
            //指令愈時時間
            Command.CommandTimeout = timeOut;
            //SQL指令
            Command.CommandText = sqlCmd;
            //SQL執行查詢模式
            Command.CommandType = commandType;
            //參數設定
            if (null == parameters || parameters.Length == 0)
            {
                return;
            }
            Command.Parameters.AddRange(parameters);
        }
        */
        protected DbCommand GetCommand(int timeOut, CommandType commandType, string sqlCmd, DbParameter[] parameters)
        {
            //使用datareader 時需要close conn 
            //if (Conn == null || Conn.State == ConnectionState.Closed)
            //{
                Conn = GetConnection;
            //}

            var command = Conn.CreateCommand();
            //指令愈時時間
            command.CommandTimeout = timeOut;
            //SQL指令
            command.CommandText = sqlCmd;
            //SQL執行查詢模式
            command.CommandType = commandType;
            //參數設定
            if (null != parameters)
            {
                command.Parameters.AddRange(parameters);
            }
            return command;
        }

        public abstract void Dispose();
    }
}
