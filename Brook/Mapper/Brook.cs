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
            DatabaseType dt = DatabaseType.MySQL, 
            int timeout = 5)
        {
            var dbConfig = new DatabaseConfiguration
            {
                Connection = dbConnectionString,
                Name = dt.ToString(),
                CommandTimeout = timeout
            };

            switch (dt)
            {
                case DatabaseType.SQLServer:
                    dbConfig.ProviderName = "System.Data.SqlClient";
                    break;
                case DatabaseType.PostgreSQL:
                    dbConfig.ProviderName = "Npgsql";
                    break;
                case DatabaseType.MySQL:
                    dbConfig.ProviderName = "MySql.Data.MySqlClient";
                    break;
                case DatabaseType.SQLite:
                    dbConfig.ProviderName = "System.Data.SQLite";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dt), dt, null);
            }

            return new SqlMapper(dbConfig);
        }
    }
}