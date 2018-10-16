using jIAnSoft.Brook.Configuration;

namespace jIAnSoft.Brook.Mapper
{
    public static class Brook
    {
        public static SqlMapper Load(string db)
        {
            return new SqlMapper(db);
        }

        public static SqlMapper LoadFromConnectionString(string dbConnectionString,
            DatabaseType dt = DatabaseType.MySql,int timeout = 5)
        {
            var dbConfig = new DatabaseConfiguration
            {
                Connection = dbConnectionString,
                Name = dt.ToString(),
                CommandTimeOut = timeout
            };
            switch (dt)
            {
                case DatabaseType.SqlServer:
                    dbConfig.ProviderName = "System.Data.SqlClient";
                    break;
                case DatabaseType.PostgreSql:
                    dbConfig.ProviderName = "Npgsql";
                    break;
                case DatabaseType.MySql:
                    dbConfig.ProviderName = "MySql.Data.MySqlClient";
                    break;
            }

            return new SqlMapper(dbConfig);
        }
    }
}