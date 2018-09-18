using jIAnSoft.Brook.Mapper;
using System;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Npgsql;
using NpgsqlTypes;

namespace DemoNetFramework
{
    internal static class Program
    {
        private static void PostgreSql()
        {
            Console.WriteLine("From PostgreSQL");
            var t = Brook.Load("postgresql").Table("SELECT id ,name ,email FROM public.account where name = @name;",
                new DbParameter[]
                {
                    new NpgsqlParameter("@name", NpgsqlDbType.Varchar)
                    {
                        Value = "許功蓋"
                    }
                });
            foreach (DataRow row in t.Rows)
            {
                Console.WriteLine($"    {row[0]} {row[1]} {row[2]}");
            }
        }

        private static void MsSql()
        {
            Console.WriteLine("From MsSQL");
            var t = Brook.Load("mssql").Query<Account>("SELECT [Id],[Name] ,[Email] FROM account;");
            foreach (var row in t)
            {
                Console.WriteLine($"    {row.Id} {row.Name} {row.Email}");
            }
        }

        private static void MySql()
        {
            Console.WriteLine("From MySQL");
            var ds = Brook.Load("mysql").DataSet(
                "SELECT `id`,`name`,`email` FROM `account` WHERE `name` = @name;",
                new DbParameter[]
                {
                    new MySqlParameter("@name", MySqlDbType.VarChar)
                    {
                        Value = "Ben Nuttall"
                    },
                });
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                Console.WriteLine($"    {row[0]} {row[1]} {row[2]}");
            }
        }

        private static void Main(string[] args)
        {
            try
            {
                MsSql();
                PostgreSql();
                MySql();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }

    public class Account
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
