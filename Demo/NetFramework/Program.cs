using jIAnSoft.Framework.Brook.Mapper;
using System;
using System.Data;
using System.Data.Common;
namespace DemoNetFramework
{
    internal static class Program
    {
        private static void PostgreSql()
        {
            Console.WriteLine("From PostgreSQL");
            foreach (DataRow row in Brook.Load("postgresql").Table("SELECT id ,name ,email FROM public.account;").Rows)
            {
                Console.WriteLine($" {row[0]} {row[1]} {row[2]}");
            }
        }

        private static void MsSql()
        {
            Console.WriteLine("From MsSQL");
            var t = Brook.Load("mssql")
                .Query<Account>("SELECT [id] AS [Id],[name] AS [Name] ,[email] AS [Email] FROM account;");
            foreach (var row in t)
            {
                Console.WriteLine($" {row.Id} {row.Name} {row.Email}");
            }
        }

        private static void MySql()
        {
            Console.WriteLine("From MySQL");
            var ds = Brook.Load("mysql").DataSet(
                "SELECT `id`,`name`,`email` FROM `account`;",
                new DbParameter[]
                {
                });
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                Console.WriteLine($" {row[0]} {row[1]} {row[2]}");
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
