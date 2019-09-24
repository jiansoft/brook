using System;
namespace jIAnSoft.Brook
{
    /// <inheritdoc />
    /// <summary>
    /// This is a System.Exception class for handling exceptions in  SQL operations.
    /// </summary>
    [Serializable]
    public class SqlException : Exception
    {
        public SqlException(string message) : base(message) { }
        public SqlException(string message, Exception inner) : base(message, inner) { }
    }
}
