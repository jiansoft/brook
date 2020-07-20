using jIAnSoft.Brook.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace jIAnSoft.Brook.Mapper
{
    public sealed class SqlMapper : IDisposable
    {
        private bool _disposed;

        private DbProvider _db;

        public string ConnectionSource => _db.DbConfig.Connection;

        internal SqlMapper(string dbName) : this(Section.Get.Database.Which.ContainsKey(dbName)
            ? Section.Get.Database.Which[dbName]
            : new DatabaseConfiguration {Name = dbName})
        {
        }

        internal SqlMapper(DatabaseConfiguration databaseConfiguration)
        {
            _db = new DbProvider(databaseConfiguration);
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnection" /> class.
        /// </summary>
        /// <returns></returns>
        public DbConnection NewConnection()
        {
            var conn = _db.CreateConnection();
            return conn;
        }

        /// <summary>
        ///  Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbParameter" /> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public DbParameter Parameter(string name, DbType dbType, int size = 0, object value = null)
        {
            return Parameter(name, value, dbType, size);
        }

        /// <summary>
        ///  Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbParameter" /> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public DbParameter Parameter(string name, DbType dbType, ParameterDirection direction, int size = 0)
        {
            return Parameter(name, dbType, size, null, direction);
        }

        /// <summary>
        ///  Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbParameter" /> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public DbParameter Parameter(string name, object value, DbType dbType = DbType.String, int size = 0)
        {
            return Parameter(name, dbType, size, value, ParameterDirection.Input);
        }

        /// <summary>
        ///  Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbParameter" /> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public DbParameter Parameter(string name, DbType dbType, int size, ParameterDirection direction, object value)
        {
            return Parameter(name, dbType, size, value, direction);
        }

        /// <summary>
        ///  Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbParameter" /> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public DbParameter Parameter(string name, DbType dbType, int size, object value, ParameterDirection direction)
        {
            var p = _db.CreateParameter(name, value, dbType, size, direction);
            return p;
        }

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T First<T>(string sql, DbParameter[] parameters = null)
        {
            return First<T>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T First<T>(CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return First<T>(_db.DbConfig.CommandTimeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return first row data that type is <see cref="T"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T First<T>(int timeout, CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return _db.First<T>(timeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a DataTable <see cref="DataTable"/>.
        /// </summary>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public DataTable Table(string sql, DbParameter[] parameters = null)
        {
            return Table(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a DataTable <see cref="DataTable"/>.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public DataTable Table(CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return Table(_db.DbConfig.CommandTimeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a DataTable <see cref="DataTable"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public DataTable Table(int timeout, CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return _db.Table(timeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet DataSet(string sql, DbParameter[] parameters = null)
        {
            return DataSet(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet DataSet(CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return DataSet(_db.DbConfig.CommandTimeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet DataSet(int timeout, CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return _db.DataSet(timeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public List<T> Query<T>(string sql, DbParameter[] parameters = null)
        {
            return Query<T>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public List<T> Query<T>(CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return Query<T>(_db.DbConfig.CommandTimeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public List<T> Query<T>(int timeout, CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return _db.Query<T>(timeout, commandType, sql, parameters);
        }
        
        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="sql">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T Value<T>(string sql, DbParameter[] parameters = null)
        {
            return Value<T>(CommandType.StoredProcedure, sql, parameters);
        }

        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T Value<T>(CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return Value<T>(_db.DbConfig.CommandTimeout, commandType, sql, parameters);
        }

        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T Value<T>(int timeout, CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return _db.Value<T>(timeout, commandType, sql, parameters);
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public int Execute(string sql, DbParameter[] parameters = null)
        {
            return Execute(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public int Execute(CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return Execute(_db.DbConfig.CommandTimeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public int Execute(int timeout, CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            var r = Execute(timeout, commandType, sql, new [] {parameters});
            return r.Length > 0 ? r[0] : 0;
        }
        
        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public int[] Execute(string sql, List<DbParameter[]> parameters)
        {
            return Execute(sql, parameters.ToArray());
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public int[] Execute(CommandType commandType, string sql, List<DbParameter[]> parameters)
        {
            return Execute(commandType, sql, parameters.ToArray());
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public int[] Execute(int timeout, CommandType commandType, string sql, List<DbParameter[]> parameters)
        {
           return Execute(timeout, commandType, sql, parameters.ToArray());
        }

        /// <summary>
        /// Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public int[] Execute(string sql, DbParameter[][] parameters)
        {
            return Execute(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public int[] Execute(CommandType commandType, string sql, DbParameter[][] parameters)
        {
            return Execute(_db.DbConfig.CommandTimeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public int[] Execute(int timeout, CommandType commandType, string sql, DbParameter[][] parameters)
        {
            return _db.Execute(timeout, commandType, sql, parameters);
        }

        /// <summary>
        /// Execute SQL and return first row and column as a <see cref="T"/>.
        /// </summary>
        /// <param name="sql">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public T One<T>(string sql, DbParameter[] parameters = null)
        {
            return One<T>(CommandType.Text, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return first row and column as a <see cref="T"/>.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T One<T>(CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return One<T>(_db.DbConfig.CommandTimeout, commandType, sql, parameters);
        }

        /// <summary>
        ///  Execute SQL and return first row and column as a <see cref="T"/>.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sql">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T One<T>(int timeout, CommandType commandType, string sql, DbParameter[] parameters = null)
        {
            return _db.One<T>(timeout, commandType, sql, parameters);
        }

        /// <summary>
        /// Begins a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolation">The isolation level under which the transaction should run.</param>
        public void BeginTransaction(IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            _db.Begin(isolation);
        }

        /// <summary>
        /// Commits the database transaction
        /// </summary>
        public void CommitTransaction()
        {
            _db.Commit();
        }

        /// <summary>
        /// Rolls back a transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            _db.Rollback();
        }

        /// <summary>
        /// Changes the current database for an open connection.
        /// </summary>
        /// <param name="database">The name of the database to use.</param>
        public void ChangeDatabase(string database)
        {
            _db.Change(database);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            _disposed = true;

            if (null == _db)
            {
                return;
            }

            _db.Dispose();
            _db = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}