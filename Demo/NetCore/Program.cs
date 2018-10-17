using jIAnSoft.Brook.Configuration;
using jIAnSoft.Brook.Mapper;
using jIAnSoft.Nami.Clockwork;
using MySql.Data.MySqlClient;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;

namespace DemoNetCore
{
    internal static class Program
    {
        public static void GetTimeZone()
        {
            const string OUTPUTFILENAME = @"./TimeZoneInfo.txt";

            DateTimeFormatInfo dateFormats = CultureInfo.CurrentCulture.DateTimeFormat;
            ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
            StreamWriter sw = new StreamWriter(OUTPUTFILENAME, false);

            foreach (TimeZoneInfo timeZone in timeZones)
            {
                bool hasDST = timeZone.SupportsDaylightSavingTime;
                TimeSpan offsetFromUtc = timeZone.BaseUtcOffset;
                TimeZoneInfo.AdjustmentRule[] adjustRules;
                string offsetString;

                sw.WriteLine("ID: {0}", timeZone.Id);
                sw.WriteLine("   Display Name: {0, 40}", timeZone.DisplayName);
                sw.WriteLine("   Standard Name: {0, 39}", timeZone.StandardName);
                sw.Write("   Daylight Name: {0, 39}", timeZone.DaylightName);
                sw.Write(hasDST ? "   ***Has " : "   ***Does Not Have ");
                sw.WriteLine("Daylight Saving Time***");
                offsetString = String.Format("{0} hours, {1} minutes", offsetFromUtc.Hours, offsetFromUtc.Minutes);
                sw.WriteLine("   Offset from UTC: {0, 40}", offsetString);
                adjustRules = timeZone.GetAdjustmentRules();
                sw.WriteLine("   Number of adjustment rules: {0, 26}", adjustRules.Length);
                if (adjustRules.Length > 0)
                {
                    sw.WriteLine("   Adjustment Rules:");
                    foreach (TimeZoneInfo.AdjustmentRule rule in adjustRules)
                    {
                        TimeZoneInfo.TransitionTime transTimeStart = rule.DaylightTransitionStart;
                        TimeZoneInfo.TransitionTime transTimeEnd = rule.DaylightTransitionEnd;

                        sw.WriteLine("      From {0} to {1}", rule.DateStart, rule.DateEnd);
                        sw.WriteLine("      Delta: {0}", rule.DaylightDelta);
                        if (!transTimeStart.IsFixedDateRule)
                        {
                            sw.WriteLine("      Begins at {0:t} on {1} of week {2} of {3}", transTimeStart.TimeOfDay,
                                transTimeStart.DayOfWeek,
                                transTimeStart.Week,
                                dateFormats.MonthNames[transTimeStart.Month - 1]);
                            sw.WriteLine("      Ends at {0:t} on {1} of week {2} of {3}", transTimeEnd.TimeOfDay,
                                transTimeEnd.DayOfWeek,
                                transTimeEnd.Week,
                                dateFormats.MonthNames[transTimeEnd.Month - 1]);
                        }
                        else
                        {
                            sw.WriteLine("      Begins at {0:t} on {1} {2}", transTimeStart.TimeOfDay,
                                transTimeStart.Day,
                                dateFormats.MonthNames[transTimeStart.Month - 1]);
                            sw.WriteLine("      Ends at {0:t} on {1} {2}", transTimeEnd.TimeOfDay,
                                transTimeEnd.Day,
                                dateFormats.MonthNames[transTimeEnd.Month - 1]);
                        }
                    }
                }
            }

            sw.Close();
        }

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

            using (var db = Brook.Load("postgresql"))
            {
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
                Console.WriteLine($"First Id:{account.Id} Email:{account.Email} Name:{account.Name}");

                var accounts =
                    db.Query<Account>(
                        "SELECT `id` AS `Id` ,`name` AS `Name`,`email` AS `Email` FROM `account` ORDER BY `id` DESC;");
                foreach (var a in accounts)
                {
                    Console.WriteLine($"Query   Id:{a.Id} Email:{a.Email} Name:{a.Name}");
                }

                var one = db.One<int>(CommandType.StoredProcedure, "test.ReturnValue",
                    new[] {db.Parameter("@param1", 12, DbType.Int32)});
                Console.WriteLine($"one is {one}");

                using (var db1 = Brook.LoadFromConnectionString(Section.Get.Database.Which["mysql"].Connection))
                {
                    var two = db1.One<int>(CommandType.StoredProcedure, "test.ReturnValue",
                        new[] {db.Parameter("@param1", DateTime.Now.Ticks / 1000 % 10000000, DbType.Int32)});
                    Console.WriteLine($"two is {two}");
                }
            }
        }


        private static void Main(string[] args)
        {
            Nami.Every(1000).Milliseconds().Do(() => {
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