using jIAnSoft.Brook.Mapper;
using jIAnSoft.Nami.Clockwork;
using MySql.Data.MySqlClient;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using System.Data.Common;

namespace DemoNetFramework
{
    internal static class Program
    {
        private static void PostgreSql()
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} From PostgreSQL");
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
                Console.WriteLine($"t    {row[0]} {row[1]} {row[2]}");
            }

            var db = Brook.Load("postgresql");
            var ds = db.DataSet("SELECT id ,name ,email FROM public.account where name = @name;",
                new[]
                {
                    db.Parameter("@name", "許功蓋", DbType.String)
                });
            foreach (var row in ds.Tables[0].AsEnumerable())
            {
                Console.WriteLine($"ds    {row[0]} {row[1]} {row[2]}");
            }
        }

        private static void MsSql()
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} From MsSQL");
            var t = Brook.Load("mssql").Query<Account>("SELECT [Id],[Name] ,[Email] FROM account;");
            foreach (var row in t)
            {
                Console.WriteLine($"    {row.Id} {row.Name} {row.Email}");
            }
        }

        private static void MySql()
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} From MySQL");

            using (var db = Brook.Load("mysql"))
            {
                var t = db.Table(
                    "SELECT `id`,`name`,`email` FROM `account` WHERE `name` = ?name;",
                    new[] {db.Parameter("?name", "Ben Nuttall", DbType.String)});
                foreach (DataRow row in t.Rows)
                {
                    Console.WriteLine($"t    {row[0]} {row[1]} {row[2]}");
                }

                var ds = Brook.Load("mysql").DataSet(
                    "SELECT `id`,`name`,`email` FROM `account` WHERE `name` = ?name;",
                    new DbParameter[]
                    {
                        new MySqlParameter("?name", MySqlDbType.VarChar)
                        {
                            Value = "Ben Nuttall"
                        }
                    });
                foreach (var row in ds.Tables[0].AsEnumerable())
                {
                    Console.WriteLine($"ds    {row[0]} {row[1]} {row[2]}");
                }

                var account = db.First<Account>(
                    "SELECT `id` AS `Id` ,`name` AS `Name`,`email` AS `Email` FROM `account` WHERE `id` = ?id;",
                    new[] {db.Parameter("?id", 1, DbType.Int32)});
                Console.WriteLine($"First   Id:{account.Id} Email:{account.Email} Name:{account.Name}");

                var accounts =
                    db.Query<Account>(
                        "SELECT `id` AS `Id` ,`name` AS `Name`,`email` AS `Email` FROM `account` order by `id` desc;");
                foreach (var a in accounts)
                {
                    Console.WriteLine($"Query   Id:{a.Id} Email:{a.Email} Name:{a.Name}");
                }

                var one = db.One<int>(CommandType.StoredProcedure, "test.ReturnValue",
                    new[] {db.Parameter("@param1", 12, DbType.Int32)});
                Console.WriteLine($"one is {one}");
            }
        }

        private static void Main(string[] args)
        {
            Nami.Every(1).Seconds().Do(() => {
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
            });

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
