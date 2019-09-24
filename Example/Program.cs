using IdGen;
using jIAnSoft.Brook;
using jIAnSoft.Nami.Clockwork;
using NLog;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace Example
{
    internal static class Program
    {
        private static readonly IdGenerator Generator = new IdGenerator(0);
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string GetId = "SELECT max({id}) AS {id} FROM {account};";
        private const string Account = "SELECT TOP 10 {id},{name},{email} FROM {account} ORDER BY {id} DESC LIMIT 10;";

        private const string ForObject =
            "SELECT TOP 10 {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} ORDER BY {id} DESC LIMIT 10;";

        private const string FindByName =
            "SELECT {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} where {name} = @name;";

        private const string FindById =
            "SELECT {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} where {id} = @id;";

        private const string InsertAccount = "insert into {account} ({name},{email})values(@name, @email);";
        private const string DeleteAccount = "DELETE FROM {account} WHERE {id} > @id;";

        private static void RunCmd(string dbName, int count)
        {
            var sw = new Stopwatch();
            sw.Start();

            DatabaseType dt;
            switch (dbName)
            {
                case "mssql":
                case "sqlserver":
                    dt = DatabaseType.SQLServer;
                    break;
                case "posql":
                    dt = DatabaseType.PostgreSQL;
                    break;
                case "mysql":
                    dt = DatabaseType.MySQL;
                    break;
                default:
                    dt = DatabaseType.SQLite;
                    const string dbPath = @".\brook.sqlite";
                    var dbExists = File.Exists(dbPath);
                    if (!dbExists)
                    {
                        var init = File.ReadAllText("./App_Data/sqlite.sql");
                        using (var db = jIAnSoft.Brook.Mapper.Brook.Load(dbName))
                        {
                            db.Execute(init);
                        }
                    }

                    break;
            }

            var providerNme = Enum.GetName(typeof(DatabaseType), dt);
            // Log.Info($"From {providerNme} {count}");

            try
            {
                using (var db = jIAnSoft.Brook.Mapper.Brook.Load(dbName))
                {
                    db.Execute(ConvertSeparate(InsertAccount, dt),
                        new[]
                        {
                            db.Parameter("@name", $"{Generator.CreateId()}"),
                            db.Parameter("@email", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}@{providerNme}.com")
                        });
                    var id = db.One<long>(ConvertSeparate(GetId, dt));
                    var query = db.Query<Account>(ConvertSeparate(ForObject, dt));
                    var table = db.Table(ConvertSeparate(Account, dt));
                    var dataSet = db.DataSet(ConvertSeparate(FindByName, dt), new[] {db.Parameter("@name", "許功蓋")});
                    var account = db.First<Account>(ConvertSeparate(FindById, dt),
                        new[] {db.Parameter("@id", id, DbType.Int32)});


                    foreach (var row in query)
                    {
                        //Log.Info($"{count} {providerNme} Query  {row.Id} {row.Name} {row.Email}");
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        //Log.Info($"{count} {providerNme} table  {row[0]} {row[1]} {row[2]}");
                    }

                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        if (row != null)
                        {
                            //Log.Info($"{count} {providerNme} sdataSet    {row[0]} {row[1]} {row[2]}");
                        }
                    }

                    if (null != account)
                    {
                        Log.Info(
                            $"{count} {providerNme} First  Id:{account.Id} Email:{account.Email} Name:{account.Name}");
                    }

                    if (dt == DatabaseType.MySQL)
                    {
                        var one = db.One<int>(
                            CommandType.StoredProcedure,
                            "test.ReturnValue",
                            new[] {db.Parameter("@param1", DateTime.Now.Ticks / 1000 % 10000000, DbType.Int32)});
                        //Log.Info($"{providerNme} One is {one}");
                    }

                    if (count % 1000 == 0)
                    {
                        db.Execute(ConvertSeparate(DeleteAccount, dt), new[] {db.Parameter("@id", 3, DbType.Int32)});
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            sw.Stop();
            var rand = new Random(Guid.NewGuid().GetHashCode());
            var nextTime = rand.Next(100, 1000);

            /*var diffTicks = (DateTime.Now.Ticks - previousDate.Ticks) / TimeSpan.TicksPerMillisecond;
            Log.Info(
                $"{count} {sw.ElapsedMilliseconds} ms {providerNme} nextTime:{nextTime} previousDelay:{previousDelay} diffTicks:{diffTicks}");
            var newPreviousDate = DateTime.Now;*/
            Nami.Delay(nextTime).Do(() => { RunCmd(dbName, ++count); });
        }

        private static string ConvertSeparate(string sql, DatabaseType dt = DatabaseType.SQLite)
        {
            switch (dt)
            {
                case DatabaseType.SQLServer:
                    return sql.Replace("{", "[").Replace("}", "]").Replace("LIMIT 10", "");
                case DatabaseType.PostgreSQL:
                    return sql.Replace("{", "\"").Replace("}", "\"").Replace("TOP 10", "");
                case DatabaseType.MySQL:
                    return sql.Replace("{", "`").Replace("}", "`").Replace("TOP 10", "");
                default:
                    return sql.Replace("{", "").Replace("}", "").Replace("TOP 10", "");
            }
        }

        private static void Run(int count)
        {
            var sqlType = new[]
            {
                "mysql",
                "posql",
                //  "sqlite",
                "mssql",
                "sqlserver"
            };
            foreach (var s in sqlType)
            {
                Nami.Delay(100).Do(() =>
                {
                    try
                    {
                        RunCmd(s, count);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, e.Message);
                    }
                });
            }
        }

        public static void Main(string[] args)
        {
#if NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0
            if (File.Exists("Example.dll.config"))
            {
                //There need to delete .net framework config file if we run the program as .net core app
                File.Delete("Example.dll.config");
            }
#endif

            Nami.RightNow().Do(() =>
            {
                Run(0);
               // Run(0);
               // Run(0);
            });

            ConsoleKeyInfo cki;
            Console.TreatControlCAsInput = true;
            Console.WriteLine("Press the CTRL + Q key to quit: \n");
            do
            {
                cki = Console.ReadKey();
            } while (cki.Key != ConsoleKey.Q && (cki.Modifiers & ConsoleModifiers.Control) == 0);
        }
    }

    public class Account
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}