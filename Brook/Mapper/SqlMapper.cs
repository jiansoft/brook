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

        internal SqlMapper(string dbName)
        {
            var dbConfig = Section.Get.Database.Which.ContainsKey(dbName)
                ? Section.Get.Database.Which[dbName]
                : new DatabaseConfiguration {Name = dbName};
            _db = new DbProvider(dbConfig);
        }

        public DbParameter Parameter(string name, DbType dbType, int size = 0, object value = null)
        {
            return Parameter(name, value, dbType, size);
        }

        public DbParameter Parameter(string name, DbType dbType, ParameterDirection direction, int size = 0)
        {
            return Parameter(name, dbType, size, null, direction);
        }

        public DbParameter Parameter(string name, object value, DbType dbType, int size = 0)
        {
            return Parameter(name, dbType, size, value, ParameterDirection.Input);
        }

        public DbParameter Parameter(string name, DbType dbType, int size, ParameterDirection direction, object value)
        {
            return Parameter(name, dbType, size, value, direction);
        }

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
//            using (var db = new DbProvider(_db.ConnStringSetting.Name))
//            {
            return _db.First<T>(commandType, sqlCmd, parameters);
//            }
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
//            using (var db = new DbProvider(_db))
//            {
            return DataSet(commandType, sqlCmd, parameters).Tables[0];
//            }
        }

        public DataSet DataSet(string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(CommandType.Text, sqlCmd, parameters);
        }

        public DataSet DataSet(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            //using (var db = new DbProvider(_db.ConnStringSetting.Name))
            //{
            return _db.DataSet(commandType, sqlCmd, parameters);
            //}
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
//            using (var db = new DbProvider(_db.ConnStringSetting.Name))
//            {
            return _db.Query<T>(commandType, sqlCmd, parameters);
//            }
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
//            using (var db = new DbProvider(_db.ConnStringSetting.Name))
//            {
            return _db.Value<T>(commandType, sqlCmd, parameters);
//            }
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
//            using (var db = new DbProvider(_db.ConnStringSetting.Name))
//            {
            return _db.Execute(commandType, sqlCmd, parameters);
//            }
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
        ///  Execute SQL and return an <see cref="T"/>.
        /// </summary>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T One<T>(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
//            using (var db = new DbProvider(_db.ConnStringSetting.Name))
//            {
            return _db.One<T>(commandType, sqlCmd, parameters);
//            }
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