namespace jIAnSoft.Brook
{
    public enum DatabaseProviderName
    {
        /// <summary>
        /// For MySql.Data.MySqlClient
        /// </summary>
        MySQL,

        /// <summary>
        /// For System.Data.SqlClient
        /// </summary>
        SQLServer,

        /// <summary>
        /// For Microsoft.Data.SqlClient
        /// </summary>
        MicrosoftSQLServer,

        /// <summary>
        /// For Npgsql
        /// </summary>
        PostgreSQL,

        /// <summary>
        /// For System.Data.SQLite
        /// </summary>
        SQLite,

        /// <summary>
        /// For Microsoft.Data.Sqlite
        /// </summary>
        MicrosoftSqlite
    }
}