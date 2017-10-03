using System;
using System.Data;
using System.Data.Common;
using jIAnSoft.Framework.Brook;
using MySql.Data.MySqlClient;

namespace DemoNetFramework
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                using (var db = new MsSql("Main"))
                {
                    var t = db.Table(
                        "SELECT TOP (10) [AccountID],[AccountSN],[AccountType],[Account],[Password],[Status],[Verify],[Agent],[VIPLv],[CreateTime] FROM [MainDB].[dbo].[TB_Account]",
                        new DbParameter[]
                        {
                           
                        });
                    foreach (DataRow row in t.Rows)
                    {
                        Console.WriteLine($"{row[0]} {row[1]} {row[2]}");
                    }
                }

                using (var db = new jIAnSoft.Framework.Brook.MySql("Mr4"))
                {
                    var t = db.Table(
                       "SELECT `id`,`email`,`name`FROM `mr4`.`pet_user` WHERE `id` = @id",
                       new DbParameter[]
                       {
                           new MySqlParameter("@id",MySqlDbType.Int32)
                           {
                               Value = 1
                           }
                       });
                    foreach (DataRow row in t.Rows)              {
                        Console.WriteLine($"{row[0]} {row[1]} {row[2]}");
                    }
                    var p = db.First<PetUser>("SELECT `id` AS `Id`,`email` AS `Email`,`name`FROM `mr4`.`pet_user` WHERE `id` = @id",
                        new DbParameter[]
                        {
                            new MySqlParameter("@id", MySqlDbType.Int32)
                            {
                                Value = 1
                            }
                        });
                    Console.WriteLine($"Id:{p.Id} Email:{p.Email} Name:{p.Name}");

                    var ds = db.DataSet("SELECT `id` AS `Id`,`email` AS `Email`,`name`FROM `mr4`.`pet_user` WHERE `id` = @id;SELECT `id` AS `Id`,`email` AS `Email`,`name`FROM `mr4`.`pet_user` WHERE `id` = @id",
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
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }

    public class PetUser
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
