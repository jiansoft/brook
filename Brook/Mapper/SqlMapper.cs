using jIAnSoft.Brook.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace jIAnSoft.Brook.Mapper
{
    public class SqlMapper : IDisposable
    {
        private bool _disposed;

        private DbProvider _db;

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
        public DbParameter Parameter(string name, object value, DbType dbType, int size = 0)
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
            return _db.First<T>(commandType, sqlCmd, parameters);
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
        public DataTable Table(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return _db.Table(commandType, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="sqlCmd"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet DataSet(string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="sqlCmd"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet DataSet(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return _db.DataSet(commandType, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public List<T> Query<T>(string sqlCmd, DbParameter[] parameters = null)
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
        public List<T> Query<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return _db.Query<T>(commandType, sqlCmd, parameters);
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
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T Value<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return _db.Value<T>(commandType, sqlCmd, parameters);
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
        ///  Executes a SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public int Execute(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return _db.Execute(commandType, sqlCmd, parameters);
        }

        /// <summary>
        /// Execute SQL and return first row and column as a <see cref="T"/>.
        /// </summary>
        /// <param name="sqlCmd">SQL 指令</param>
        /// <param name="parameters">SQL 指令參數</param>
        /// <returns></returns>
        public T One<T>(string sqlCmd, DbParameter[] parameters = null)
        {
            return One<T>(CommandType.Text, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return first row and column as a <see cref="T"/>.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T One<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return _db.One<T>(commandType, sqlCmd, parameters);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;

            if (null != _db)
            {
                _db.Dispose();
                _db = null;
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}