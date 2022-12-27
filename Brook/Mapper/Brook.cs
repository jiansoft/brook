using System;
using jIAnSoft.Brook.Configuration;

namespace jIAnSoft.Brook.Mapper
{
    public static class Brook
    {
        public static SqlMapper Load(string db)
        {
            return new SqlMapper(db);
        }

        public static SqlMapper LoadFromConnectionString(
            string dbConnectionString,
            DatabaseProviderName databaseProvider = DatabaseProviderName.MySQL,
            int timeout = 5)
        {
            string providerName;
            switch (databaseProvider)
            {
                case DatabaseProviderName.MicrosoftSQLServer:
                    providerName = "Microsoft.Data.SqlClient";
                    break;
                case DatabaseProviderName.SQLServer:
                    providerName = "System.Data.SqlClient";
                    break;
                case DatabaseProviderName.PostgreSQL:
                    providerName = "Npgsql";
                    break;
                case DatabaseProviderName.MySQL:
                    providerName = "MySql.Data.MySqlClient";
                    break;
                case DatabaseProviderName.SQLite:
                    providerName = "System.Data.SQLite";
                    break;
                case DatabaseProviderName.MicrosoftSqlite:
                    providerName = "Microsoft.Data.Sqlite";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, null);
            }

            var dbConfig = new DatabaseConfiguration
            {
                Connection = dbConnectionString,
                Name = databaseProvider.ToString(),
                ProviderName = providerName,
                CommandTimeout = timeout
            };

            return new SqlMapper(dbConfig);
        }
    }
}
