using jIAnSoft.Framework.Brook.Mapper;
using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Data;
using System.Data.Common;

namespace DemoNetFramework
{
    internal static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
//            for (var i = 0; i < 2000; i++)
//            {
//                var i1 = i;
//                Cron.Every(1).Seconds().Do(() =>
//                {
//                    Test(i1.ToString());
//                });
//            }
            
            try
            {
               
                var postgreDs = Brook.Load("Eddie").DataSet("select * FROM \"public\".\"User\";select * FROM \"public\".\"User\";");
                var postgreTable= Brook.Load("Eddie").Table("select * FROM \"public\".\"User\"");
                foreach (DataRow row in postgreTable.Rows)
                {
                    Console.WriteLine($"Postgre query {row[0]} {row[1]} {row[2]} {row[3]} {row[4]}");
                }
                var mssqlTable = Brook.Load("Main").Table("SELECT TOP (10) [AccountID],[AccountSN],[AccountType],[Account],[Password],[Status],[Verify],[Agent],[VIPLv],[CreateTime] FROM [MainDB].[dbo].[TB_Account]");

                foreach (DataRow row in mssqlTable.Rows)
                {
                    Console.WriteLine($"{row[0]} {row[1]} {row[2]}");
                }

                
                    var t = Brook.Load("Mr4").Table(
                        "SELECT `id`,`email`,`name`FROM `mr4`.`pet_user` WHERE `id` = @id",
                        new DbParameter[]
                        {
                            new MySqlParameter("@id", MySqlDbType.Int32)
                            {
                                Value = 1
                            }
                        });
                    foreach (DataRow row in t.Rows)
                    {
                        Console.WriteLine($"{row[0]} {row[1]} {row[2]}");
                    }
                    var p = Brook.Load("Mr4").First<PetUser>(
                        "SELECT `id` AS `Id`,`email` AS `Email`,`name`FROM `mr4`.`pet_user` WHERE `id` = @id",
                        new DbParameter[]
                        {
                            new MySqlParameter("@id", MySqlDbType.Int32)
                            {
                                Value = 1
                            }
                        });
                    Console.WriteLine($"Id:{p.Id} Email:{p.Email} Name:{p.Name}");

                    var ds = Brook.Load("Mr4").DataSet(
                        "SELECT `id` AS `Id`,`email` AS `Email`,`name`FROM `mr4`.`pet_user` WHERE `id` = @id;SELECT `id` AS `Id`,`email` AS `Email`,`name`FROM `mr4`.`pet_user` WHERE `id` = @id",
                        new DbParameter[]
                        {
                            new MySqlParameter("@id", MySqlDbType.Int32)
                            {
                                Value = 2
                            }
                        });
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        Console.WriteLine($"我是 ds.Tables {row[0]} {row[1]} {row[2]}");
                    }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }

        public static void Test(string second)
        {
            try
            {
                var tt = Brook.Load("Main").Table("SELECT TOP (10) [AccountID],[AccountSN],[AccountType],[Account],[Password],[Status],[Verify],[Agent],[VIPLv],[CreateTime] FROM [MainDB].[dbo].[TB_Account]");
                Log.Info(second);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class PetUser
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
