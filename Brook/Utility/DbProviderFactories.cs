using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
namespace jIAnSoft.Brook.Utility
{
    public static class DbProviderFactories
    {
        internal static readonly Dictionary<string, DbProviderFactoryConfigItem> Providers;
        
#if NETSTANDARD2_0 || NETSTANDARD2_1
        static DbProviderFactories()
        {
            // <add name="MySQL Data Provider" invariant="MySql.Data.MySqlClient" description=".Net Framework Data Provider for MySQL" type="MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Version=6.9.9.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d" />

            Providers = new Dictionary<string, DbProviderFactoryConfigItem>
            {
                {
                    "System.Data.SqlClient",
                    new DbProviderFactoryConfigItem
                    {
                        Name = "SqlClient Data Provider",
                        Invariant = "System.Data.SqlClient",
                        Description = ".Net Framework Data Provider for SqlServer",
                        Type = "System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient"
                    }
                },
                {
                    "Microsoft.Data.SqlClient",
                    new DbProviderFactoryConfigItem
                    {
                        Name = "Microsoft SqlClient Data Provider",
                        Invariant = "Microsoft.Data.SqlClient",
                        Description = ".Net Framework Data Provider for SqlServer",
                        Type = "Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient"
                    }
                },
                {
                    "MySql.Data.MySqlClient",
                    new DbProviderFactoryConfigItem
                    {
                        Name = "MySQL Data Provider",
                        Invariant = "MySql.Data.MySqlClient",
                        Description = ".Net Framework Data Provider for MySQL",
                        Type = "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data"
                    }
                },
                {
                    "Npgsql",
                    new DbProviderFactoryConfigItem
                    {
                        Name = "Npgsql Data Provider",
                        Invariant = "Npgsql",
                        Description = ".Net Framework Data Provider for PostgreSql",
                        Type = "Npgsql.NpgsqlFactory, Npgsql"
                    }
                },
                {
                    "System.Data.SQLite",
                    new DbProviderFactoryConfigItem
                    {
                        Name = "SQLite Data Provider",
                        Invariant = "System.Data.SQLite",
                        Description = ".NET Framework Data Provider for SQLite",
                        Type = "System.Data.SQLite.SQLiteFactory, System.Data.SQLite"
                    }
                }
            };
        }
#endif
        
#if NETSTANDARD2_0
        public static DbProviderFactory GetFactory(string providerInvariantName)
        {
            if (string.IsNullOrWhiteSpace(providerInvariantName))
            {
                throw new ArgumentNullException(nameof(providerInvariantName));
            }

            var dbProvider = Providers[providerInvariantName];
            var type = Type.GetType(dbProvider.Type);

            if (null == type)
            {
                throw new NotImplementedException("Provider not installed");
            }
           
            var field = type.GetField("Instance",
                BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);

            if (null == field || !field.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
            {
                throw new InvalidCastException("Provider invalid");
            }

            var obj = field.GetValue(null);
            if (obj != null)
            {
                return (DbProviderFactory) obj;
            }

            throw new InvalidCastException("Provider invalid");
        }
#endif
    }

}
