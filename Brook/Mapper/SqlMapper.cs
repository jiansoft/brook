using jIAnSoft.Framework.Brook.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace jIAnSoft.Framework.Brook.Mapper
{
    public class SqlMapper
    {
        private readonly string _db;

        public SqlMapper(string db)
        {
            _db = db;
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
            return First<T>(Section.Get.Database.Which[_db].CommandTimeOut, commandType, sqlCmd, parameters);
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
            using (var db = new DbProvider(_db))
            {
                return db.First<T>(timeout, commandType, sqlCmd, parameters);
            }
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
            return Table(Section.Get.Database.Which[_db].CommandTimeOut, commandType, sqlCmd, parameters);
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
//            using (var db = new DbProvider(_db))
//            {
            return DataSet(timeout, commandType, sqlCmd, parameters).Tables[0];
//            }
        }

        public DataSet DataSet(string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(CommandType.Text, sqlCmd, parameters);
        }

        public DataSet DataSet(CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            return DataSet(Section.Get.Database.Which[_db].CommandTimeOut, commandType, sqlCmd, parameters);
        }

        public DataSet DataSet(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            using (var db = new DbProvider(_db))
            {
                return db.DataSet(timeout, commandType, sqlCmd, parameters);
            }
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
            return Query<T>(Section.Get.Database.Which[_db].CommandTimeOut, commandType, sqlCmd, parameters);
        }

        /// <summary>
        ///  Execute SQL and return a <see cref="T"/> array.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL cmd</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public List<T> Query<T>(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            using (var db = new DbProvider(_db))
            {
                return db.Query<T>(timeout, commandType, sqlCmd, parameters);
            }
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
            return Value<T>(Section.Get.Database.Which[_db].CommandTimeOut, argEmuCommandType, sqlCmd, parameters);
        }

        /// <summary>
        /// Executes a SQL statement, and returns a value that from an operation such as a stored procedure, built-in function, or user-defined function.
        /// </summary>
        /// <param name="timeout">Cmd timeout</param>
        /// <param name="commandType">SQL command type SP、Text</param>
        /// <param name="sqlCmd">SQL command</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        public T Value<T>(int timeout, CommandType commandType, string sqlCmd, DbParameter[] parameters = null)
        {
            using (var db = new DbProvider(_db))
            {
                return db.Value<T>(timeout, commandType, sqlCmd, parameters);
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
            return Execute(Section.Get.Database.Which[_db].CommandTimeOut, commandType, sqlCmd, parameters);
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
            using (var db = new DbProvider(_db))
            {
                return db.Execute(timeout, commandType, sqlCmd, parameters);
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
            return One<T>(Section.Get.Database.Which[_db].CommandTimeOut, commandType, sqlCmd, parameters);
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
            using (var db = new DbProvider(_db))
            {
                return db.One<T>(timeout, commandType, sqlCmd, parameters);
            }
        }
    }
}