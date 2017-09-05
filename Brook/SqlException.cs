using System;
using System.Runtime.Serialization;

namespace Brook
{
    /// <summary>
    /// This is a System.Exception class for handling exceptions in  SQL operations.
    /// </summary>
    [Serializable]
    public class SqlException : Exception
    {
        public SqlException() { }
        public SqlException(string message) : base(message) { }
        public SqlException(string message, Exception inner) : base(message, inner) { }
        public SqlException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
