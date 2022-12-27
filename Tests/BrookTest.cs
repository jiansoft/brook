using IdGen;
using jIAnSoft.Brook;
using jIAnSoft.Brook.Mapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;

namespace Tests
{
    [TestFixture]
    public class BrookTest
    {
        private static readonly IdGenerator Generator = new IdGenerator(0);

        private readonly string[] _sqlType =
        {
            "mysql",
            "posql",
            //"sqlite",
            "mssql"
        };

        private const string Query =
            "SELECT TOP 5 {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} ORDER BY {id} DESC LIMIT 5;";

        private const string Insert = "insert into {account} ({name},{email})values(@name, @email);";
        private const string Update = "Update {account} SET {name} = '';";
        private const string Delete = "DELETE FROM {account} WHERE {id} = @id;";

        private const string FindById =
            "SELECT {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} where {id} = @id;";

        private const string FindByName =
            "SELECT {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} where {name} = @name;";

        private static string ConvertSeparate(string sql, DatabaseProviderName dt = DatabaseProviderName.SQLite)
        {
            switch (dt)
            {
                case DatabaseProviderName.SQLServer:
                case DatabaseProviderName.MicrosoftSQLServer:
                    return sql.Replace("{", "[").Replace("}", "]").Replace("LIMIT 5", "");
                case DatabaseProviderName.PostgreSQL:
                    return sql.Replace("{", "\"").Replace("}", "\"").Replace("TOP 5", "");
                case DatabaseProviderName.MySQL:
                    return sql.Replace("{", "`").Replace("}", "`").Replace("TOP 5", "");
                default:
                    return sql.Replace("{", "").Replace("}", "").Replace("TOP 5", "");
            }
        }

        private static string GetLastId(string sql, DatabaseProviderName dt = DatabaseProviderName.SQLite)
        {
            switch (dt)
            {
                case DatabaseProviderName.SQLServer:
                    return $"{sql} SELECT IDENT_CURRENT ('account') AS Current_Identity;";
                case DatabaseProviderName.PostgreSQL:
                    return $"{sql} RETURNING id;";
                case DatabaseProviderName.MySQL:
                    return $"{sql} SELECT LAST_INSERT_ID();";
                default:
                    return $"{sql}";
            }
        }

        [Test]
        public void TestValue()
        {
            using (var db = Brook.Load("sqlserver"))
            {
                var maxId = db.Value<long>("[dbo].[sp_MaxAccountId_Sel]");
                TestContext.WriteLine($"MaxId = {maxId}");
                maxId = db.Value<long>(CommandType.StoredProcedure, "[dbo].[sp_MaxAccountId_Sel]");
                TestContext.WriteLine($"MaxId = {maxId}");
                maxId = db.Value<long>(15, CommandType.StoredProcedure, "[dbo].[sp_MaxAccountId_Sel]");
                TestContext.WriteLine($"MaxId = {maxId}");
            }
        }

        [Test]
        public void TestOne()
        {
            using (var db = Brook.Load("sqlserver"))
            {
                var maxId = db.One<long>(
                    @"DECLARE @intId BIGINT;SELECT @intId = MAX(id) FROM [test].[dbo].[account]; SELECT @intId;");
                TestContext.WriteLine($"MaxId = {maxId}");
                maxId = db.One<long>(15, CommandType.Text,
                    @"DECLARE @intId BIGINT;SELECT @intId = MAX(id) FROM [test].[dbo].[account]; SELECT @intId;");
                TestContext.WriteLine($"MaxId = {maxId}");
            }
        }

        [Test]
        public void TestTransaction()
        {
            var sqlCmd =
                $"{ConvertSeparate(Insert, DatabaseProviderName.PostgreSQL)}; update account set \"name\" = 'Eddie in transaction' where id in (SELECT currval(pg_get_serial_sequence('account','id')));";
            var result = Brook.Load("posql").Transaction(sqlCmd, new List<DbParameter[]>
            {
                new[]
                {
                    Brook.Load("posql").Parameter("@name", "QQ1"),
                    Brook.Load("posql").Parameter("@email", $"QQ@QQ1.com")

                },
                new[]
                {
                    Brook.Load("posql").Parameter("@name", "QQ2"),
                    Brook.Load("posql").Parameter("@email", $"QQ@QQ2.com")

                }
            });

            if (!result.Ok)
            {
                TestContext.WriteLine($"Why:{result.Err}");
            }

            result = Brook.Load("posql").Transaction("insert into account (name,email)values('Eddie', '@email');");
            if (!result.Ok)
            {
                TestContext.WriteLine($"Why:{result.Err}");
            }

            var qq = Brook.Load("posql")
                .First<Account>(
                    "SELECT id AS \"Id\", name AS \"Name\", email AS \"Email\" FROM public.account order by id desc limit 1;");


            SqlMapper db = null;

            try
            {
                db = Brook.Load("posql");
                var name = $"我是交易1-{DateTime.Now:HHmmss}";
                db.BeginTransaction();
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.PostgreSQL), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                var t = db.Table(ConvertSeparate(FindByName, DatabaseProviderName.PostgreSQL),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine(
                    $"[Table] Id = {t.Rows[0]["Id"]} Name = {t.Rows[0]["Name"]} Email = {t.Rows[0]["Email"]} ");
                db.CommitTransaction();

                name = $"我是交易2-{DateTime.Now:HHmmss}";
                db.BeginTransaction();
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.PostgreSQL), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                var account = db.First<Account>(ConvertSeparate(FindByName, DatabaseProviderName.PostgreSQL),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[First] Id = {account.Id} Name = {account.Name} Email = {account.Email}");
                var accounts = db.Query<Account>(ConvertSeparate(FindByName, DatabaseProviderName.PostgreSQL),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine(
                    $"[Query] Id = {accounts[0].Id} Name = {accounts[0].Name} Email = {accounts[0].Email}");

                db.CommitTransaction();

                name = $"我是交易3-{DateTime.Now:HHmmss}";
                db.BeginTransaction();
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.PostgreSQL), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                db.RollbackTransaction();

                name = $"我不是交易1-{DateTime.Now:HHmmss}";
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.PostgreSQL), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                var ds = db.DataSet(ConvertSeparate(FindByName, DatabaseProviderName.PostgreSQL),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine(
                    $"[DataSet] Id = {ds.Tables[0].Rows[0]["Id"]} Name = {ds.Tables[0].Rows[0]["Name"]} Email = {ds.Tables[0].Rows[0]["Email"]}");


                name = $"我是交易4-{DateTime.Now:HHmmss}";
                db.BeginTransaction();
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.PostgreSQL), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                db.ChangeDatabase("postgres");
                //throw new Exception("QQ");
            }
            catch (Exception e)
            {
                TestContext.WriteLine(e);
                if (db != null)
                {
                    db.RollbackTransaction();
                    db.Dispose();
                }
            }
        }

        [Test]
        public void TestQuery()
        {
            try
            {
                var account1 = Brook.Load("mysql")
                    .Query<Account>("select id As Id from account where id in (1,2) FOR UPDATE SKIP LOCKED");

                var account2 = Brook.Load("mysql")
                    .Query<Account>("select id As Id from account where id in (1,2,3,8) FOR UPDATE SKIP LOCKED");

                foreach (var account in account1)
                {
                    TestContext.WriteLine($"account1 {account.Id}");
                }

                foreach (var account in account2)
                {
                    TestContext.WriteLine($"account2 {account.Id}");
                }

                using (var db = Brook.Load("mssql"))
                {
                   var accounts = db.Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseProviderName.MicrosoftSQLServer));
                   TestContext.WriteLine($"accountCount {accounts.Count}");
                }

                var sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                Brook.Load("mssql")
                    .Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseProviderName.SQLServer));
                sw.Stop();
                TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");

                sw.Reset();
                sw.Start();
                Brook.Load("mssql")
                    .Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseProviderName.SQLServer));
                sw.Stop();
                TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");

                sw.Reset();
                sw.Start();
                Brook.Load("mssql")
                    .Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseProviderName.SQLServer));
                sw.Stop();
                TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");

                sw.Reset();
                sw.Start();
                var l1 = Brook.Load("mssql")
                    .Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseProviderName.SQLServer));
                sw.Stop();
                TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");

                TestContext.WriteLine($"Account : {l1.Count}");
                sw.Reset();
                sw.Start();
                var dt = Brook.Load("mssql")
                    .Table(CommandType.Text, ConvertSeparate(Query, DatabaseProviderName.SQLServer));

                for (int i = 0; i < 100000; i++)
                {
                    var a = dt.Rows.Cast<DataRow>().Select(s => new Account
                    {
                        Name = s["Name"].ToString()
                    });
                }

                sw.Stop();
                TestContext.WriteLine($"Cast<DataRow> 总毫秒:{sw.ElapsedMilliseconds}");

                sw.Reset();
                sw.Start();
                for (int i = 0; i < 100000; i++)
                {
                    var a = dt.Select().Select(s => new Account
                    {
                        Name = s["Name"].ToString()
                    });
                }

                sw.Stop();
                TestContext.WriteLine($"Select().Select 总毫秒:{sw.ElapsedMilliseconds}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        [Test]
        public void TestExecute()
        {
            using (var db = Brook.Load("sqlserver"))
            {
                var name = $"{Generator.CreateId()}";
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                var account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseProviderName.SQLServer),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[1] Id = {account.Id} Name = {account.Name} Email = {account.Email}");

                name = $"{Generator.CreateId()}";
                db.Execute(CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseProviderName.SQLServer),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[2] Id = {account.Id} Name = {account.Name} Email = {account.Email}");

                name = $"{Generator.CreateId()}";
                db.Execute(ConvertSeparate(Insert, DatabaseProviderName.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseProviderName.SQLServer),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[3] Id = {account.Id} Name = {account.Name} Email = {account.Email}");

                name = $"{Generator.CreateId()}";
                db.Execute(ConvertSeparate(Insert, DatabaseProviderName.SQLServer), new List<DbParameter[]>
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseProviderName.SQLServer),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[4] Id = {account.Id} Name = {account.Name} Email = {account.Email}");

                name = $"{Generator.CreateId()}";
                var count = db.Execute(CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.SQLServer),
                    new List<DbParameter[]>
                    {
                        new[]
                        {
                            db.Parameter("@name", name),
                            db.Parameter("@email", $"{name}@sqlserver.com")
                        }
                    });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseProviderName.SQLServer),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine(
                    $"[5] Id = {account.Id} Name = {account.Name} Email = {account.Email} count:{JsonConvert.SerializeObject(count)}");

                name = $"{Generator.CreateId()}";
                count = db.Execute(5, CommandType.Text, ConvertSeparate(Insert, DatabaseProviderName.SQLServer),
                    new List<DbParameter[]>
                    {
                        new[]
                        {
                            db.Parameter("@name", name),
                            db.Parameter("@email", $"{name}@sqlserver.com")
                        }
                    });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseProviderName.SQLServer),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine(
                    $"[6] Id = {account.Id} Name = {account.Name} Email = {account.Email} count:{JsonConvert.SerializeObject(count)}");

                name = $"{Generator.CreateId()}";
                db.Execute($"insert into [account] ([name],[email])values('{name}', '{name}@sqlserver.com');");
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseProviderName.SQLServer),
                    new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[7] Id = {account.Id} Name = {account.Name} Email = {account.Email}");
            }
        }

        [Test]
        public void TestLoad()
        {
            foreach (var dbName in _sqlType)
            {
                DatabaseProviderName dt;
                switch (dbName)
                {
                    case "mssql":
                        dt = DatabaseProviderName.SQLServer;
                        break;
                    case "posql":
                        dt = DatabaseProviderName.PostgreSQL;
                        break;
                    case "mysql":
                        dt = DatabaseProviderName.MySQL;
                        break;
                    default:
                        dt = DatabaseProviderName.SQLite;
                        break;
                }

                var providerNme = Enum.GetName(typeof(DatabaseProviderName), dt);
                TestContext.WriteLine($"{providerNme}");

                try
                {
                    Brook.LoadFromConnectionString(Brook.Load(dbName).ConnectionSource, dt);
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }
            }
        }

        [Test]
        public void TestException()
        {
            foreach (var dbName in _sqlType)
            {
                DatabaseProviderName dt;
                switch (dbName)
                {
                    case "mssql":
                        continue;
                    /*dt = DatabaseProviderName.SQLServer;
                    break;*/
                    case "posql":
                        dt = DatabaseProviderName.PostgreSQL;
                        break;
                    case "mysql":
                        dt = DatabaseProviderName.MySQL;
                        break;
                    default:
                        continue;
                    /*dt = DatabaseProviderName.SQLite;
                    using (var db = jIAnSoft.Brook.Mapper.Brook.Load(dbName))
                    {
                        db.Execute(
                            @"CREATE TABLE IF NOT EXISTS account (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT not null, email TEXT not null);");
                    }

                    break;*/
                }

                var providerNme = Enum.GetName(typeof(DatabaseProviderName), dt);
                TestContext.WriteLine($"{providerNme}");
                try
                {
                    Brook.Load("");
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }

                try
                {
                    using (var db = Brook.Load(dbName))
                    {
                        db.Execute("SELECT * FROM A", new[]
                        {
                            db.Parameter("@name", $"{Generator.CreateId()}"),
                            db.Parameter("@email", $"{Generator.CreateId()}@{providerNme}.com")
                        });
                    }
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }

                try
                {
                    Brook.Load(dbName).Table("SELECT * FROM A");
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }

                try
                {
                    Brook.Load(dbName).DataSet("SELECT * FROM A");
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }

                try
                {
                    Brook.Load(dbName).First<Account>("SELECT * FROM A");
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }

                try
                {
                    Brook.Load(dbName).Query<Account>("SELECT * FROM A");
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }

                try
                {
                    Brook.Load(dbName).One<long>("SELECT * FROM A");
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }

                try
                {
                    Brook.Load(dbName).Value<long>("SELECT * FROM A");
                }
                catch (Exception e)
                {
                    TestContext.WriteLine($"{providerNme} {e.Message}");
                }
            }
        }

        [Test]
        public void TestCrud()
        {
            foreach (var dbName in _sqlType)
            {
                DatabaseProviderName dt;
                switch (dbName)
                {
                    case "mssql":
                        //continue;
                        dt = DatabaseProviderName.SQLServer;
                        break;
                    case "posql":
                        dt = DatabaseProviderName.PostgreSQL;
                        break;
                    case "mysql":
                        dt = DatabaseProviderName.MySQL;
                        break;
                    default:
                        continue;
                    /*dt = DatabaseProviderName.SQLite;
                    using (var db = jIAnSoft.Brook.Mapper.Brook.Load(dbName))
                    {
                        db.Execute(@"CREATE TABLE IF NOT EXISTS account (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT not null, email TEXT not null);");
                    }

                    break;*/
                }

                var providerNme = Enum.GetName(typeof(DatabaseProviderName), dt);
                using (var db = Brook.Load(dbName))
                {
                    var id = db.One<long>(
                        ConvertSeparate(GetLastId(Insert, dt), dt),
                        new[]
                        {
                            db.Parameter("@name", $"{Generator.CreateId()}"),
                            db.Parameter("@email", $"{Generator.CreateId()}@{providerNme}.com")
                        });
                    TestContext.WriteLine($"{providerNme} insert id:{id}");

                    var first = db.First<Account>(
                        ConvertSeparate(FindById, dt),
                        new[] { db.Parameter("@id", id, DbType.Int32) });
                    TestContext.WriteLine($"{providerNme} first  {first.Id} {first.Name} {first.Email}");

                    var query = db.Query<Account>(ConvertSeparate(Query, dt));
                    foreach (var acc in query)
                    {
                        TestContext.WriteLine($"{providerNme} query  {acc.Id} {acc.Name} {acc.Email}");
                    }

                    var ds = db.DataSet(
                        ConvertSeparate(FindById, dt),
                        new[] { db.Parameter("@id", 1, DbType.Int32) });
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        TestContext.WriteLine($"{providerNme} dataSet  {row["Id"]} {row["name"]} {row["email"]}");
                    }

                    var table = db.Table(
                        ConvertSeparate(FindById, dt),
                        new[] { db.Parameter("@id", 1, DbType.Int32) });
                    foreach (DataRow row in table.Rows)
                    {
                        TestContext.WriteLine($"{providerNme} table  {row["Id"]} {row["name"]} {row["email"]}");
                    }

                    var count = db.Execute(
                        CommandType.Text,
                        ConvertSeparate(Delete, dt),
                        new[] { db.Parameter("@id", id, DbType.Int64) });
                    TestContext.WriteLine($"{providerNme} delete id:{id} count:{count}");

                    count = db.Execute(
                        1,
                        CommandType.Text,
                        ConvertSeparate(Delete, dt),
                        new[] { db.Parameter("@id", id, DbType.Int64) });
                    TestContext.WriteLine($"{providerNme} delete id:{id} count:{count}");

                    count = db.Execute(
                        1,
                        CommandType.Text,
                        ConvertSeparate(Delete, dt),
                        new[] { db.Parameter("@id", id, DbType.Int64) });
                    TestContext.WriteLine($"{providerNme} delete id:{id} count:{count}");

                    count = db.Execute(ConvertSeparate(Update, dt));
                    TestContext.WriteLine($"{providerNme} update count:{count}");

                    var counts = db.Execute(
                        ConvertSeparate(Delete, dt),
                        new List<DbParameter[]> { new[] { db.Parameter("@id", id, DbType.Int64) } });
                    foreach (var i in counts)
                    {
                        TestContext.WriteLine($"{providerNme} delete id:{id} count:{i}");
                    }
                }
            }
        }

        [Test]
        public void TestQueryLocked()
        {
            try
            {
                /*var result = 1.31;
                var fixOddsMax = 1.32;
                var swapOddsMax = 1.33;
                var sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                for (int i = 0; i < length; i++)
                {

                }
                sw.Stop();*/
                //TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");

                // var account1 = Brook.Load("mysql").Query<Account>("select id As Id from account where id in (1,2) FOR UPDATE SKIP LOCKED");

                //var account2 = Brook.Load("mysql").Query<Account>("select id As Id from account where id in (1,2,3,8) FOR UPDATE SKIP LOCKED");

                var tasks = new List<Task>();
                for (int i = 0; i < 1000; i++)
                {
                    var ii = i;
                    tasks.Add(Task.Run(() =>
                    {
                        var account1 = Brook.Load("mysql")
                            .Query<Account>("select id As Id from account where id in (1,2) LOCK IN SHARE MODE;");
                        if (account1.Count == 0)
                        {
                            TestContext.WriteLine($"({ii}) account1:{account1.Count}");
                        }
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        var account2 = Brook.Load("mysql")
                            .Query<Account>("select id As Id from account where id in (1,2,3,8)  LOCK IN SHARE MODE;");
                        if (account2.Count == 2)
                        {
                            TestContext.WriteLine($"({ii}) account2:{account2.Count}");
                        }
                    }));
                }

                var wa = Task.WhenAll(tasks);
                wa.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [Test]
        public void TestWriteToFile()
        {
            var tables = new[]
            {
                new { name = "英国极速赛车", Source = "england75pk10", Target = "rolling10ex", Type = 1 },
                new { name = "英国极速时时彩", Source = "england75ssc", Target = "penta5ex", Type = 1 },
                new { name = "英国极速快乐彩", Source = "england75klc", Target = "infinity8ex", Type = 1 },
                new { name = "英国极速快乐8", Source = "england75kl8", Target = "cannon20ex", Type = 1 },
                new { name = "英国极速11选5", Source = "england7511x5", Target = "lucky5ex", Type = 1 },
                new { name = "英国极速快3", Source = "england75k3", Target = "gamma3ex", Type = 1 },
                new { name = "極速快3", Source = "jisukuai3", Target = "jisukuai3", Type = 1 },
                new { name = "極速賽車", Source = "jisusaiche", Target = "jisusaiche", Type = 1 },
                new { name = "極速時時彩", Source = "jisushishi", Target = "jisushishi", Type = 1 },
                new { name = "極速快樂十分", Source = "jisukuaile", Target = "jisukuaile", Type = 1 },

                new { name = "英国赛车", Source = "england3pk10", Target = "rolling10", Type = 1 },
                new { name = "英国时时彩", Source = "england3ssc", Target = "penta5", Type = 1 },
                new { name = "英国快乐彩", Source = "england3klc", Target = "infinity8", Type = 1 },
                new { name = "英国快乐8", Source = "england3kl8", Target = "cannon20", Type = 1 },
                new { name = "英国11选5", Source = "england311x5", Target = "lucky5", Type = 1 },
                new { name = "英国快3", Source = "england3k3", Target = "gamma3", Type = 1 },

                new { name = "英国飞艇", Source = "england5lucky10", Target = "surfing10classic", Type = 1 },
                new { name = "幸運五星彩", Source = "england5lucky5", Target = "penta5classic", Type = 1 },
                new { name = "澳洲幸運5", Source = "azxy5", Target = "azxy5", Type = 1 },
                new { name = "澳洲幸運8", Source = "azxy8", Target = "azxy8", Type = 1 },
                new { name = "澳洲幸運10", Source = "azxy10", Target = "azxy10", Type = 1 },

                new { name = "英国六合彩", Source = "uklucky7", Target = "lucky7daily", Type = 2 },
                new { name = "排列三", Source = "pl3", Target = "pl3", Type = 2 },
                new { name = "排列五", Source = "pl5", Target = "pl5", Type = 2 },
                new { name = "香港六合彩", Source = "xglhc", Target = "xglhc", Type = 2 },
                new { name = "七星彩", Source = "qxc", Target = "qxc", Type = 2 }
            };

            try
            {
                var db = Brook.Load("mysql");

                var limitTime = DateTime.Now.AddDays(-730);
                foreach (var t in tables.AsParallel())
                {
                    var filePath = Path.Combine("E:\\mysql\\backup\\wanago", $"{t.Target}.sql");
                    File.Delete(filePath);
                    File.AppendAllText(filePath, $"INSERT INTO `{t.Target}`(`period_no`,`draw_no`,`period_date`,`draw_time`,`created_time`,`updated_time`,`updated_by`)VALUES\r\n");
                    var drawTimeColumn = t.Type == 1 ? "DrawTime" : "draw_time";
                    var dt = db.Table($"Select * from safecenter.{t.Source} WHERE {drawTimeColumn} >= '{limitTime:yyyy:MM:dd HH:mm:ss}'");

                    for (var index = 0; index < dt.Rows.Count; index++)
                    {
                        var row = dt.Rows[index];
                        DateTime drawTime, drawTimeUtc;
                        string periodNo, drawNo, periodDate;
                        string updatedBy, createdTime, updatedTime;
                        if (t.Type == 1)
                        {
                            drawTime = DateTime.SpecifyKind(DateTime.Parse(row["DrawTime"].ToString() ?? string.Empty),
                                DateTimeKind.Local);
                            drawTimeUtc = drawTime.ToUniversalTime();
                            periodDate = drawTime.ToString("yyyy-MM-dd");
                            periodNo = row["PeriodNo"].ToString();
                            drawNo = row["DrawNo"].ToString();
                            createdTime = DateTime.Parse(row["UpdateDateTime"].ToString() ?? string.Empty)
                                .ToString("yyyy-MM-dd HH:mm:ss");
                            updatedTime = createdTime;
                            updatedBy = dt.Columns.Contains("FromWebsite")
                                ? row["FromWebsite"].ToString()
                                : "api.api68.com";
                        }
                        else
                        {
                            drawTime = DateTime.Parse(row["draw_time"].ToString() ?? string.Empty);
                            drawTimeUtc = drawTime;
                            periodNo = row["period_no"].ToString();
                            drawNo = row["draw_no"].ToString();
                            periodDate = drawTime.ToString("yyyy-MM-dd");
                            createdTime = DateTime.Parse(row["created_time"].ToString() ?? string.Empty)
                                .ToString("yyyy-MM-dd HH:mm:ss");
                            updatedTime = DateTime.Parse(row["updated_time"].ToString() ?? string.Empty)
                                .ToString("yyyy-MM-dd HH:mm:ss");
                            updatedBy = row["updated_by"].ToString();
                        }

                        var sql =
                            $"({periodNo},'{drawNo}','{periodDate}','{drawTimeUtc:yyyy-MM-dd HH:mm:ss}','{createdTime}','{updatedTime}','{updatedBy}')";
                        
                        if (index < dt.Rows.Count - 1)
                        {
                            sql = $"{sql},";
                        }
                        
                        sql = $"{sql}\r\n";
                        
                        File.AppendAllText(filePath, sql);
                    }

                    File.AppendAllText(filePath, " ON DUPLICATE KEY UPDATE draw_no=values(draw_no),period_date=values(period_date),draw_time=values(draw_time);");
                    GC.Collect();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public class Account
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        
        public DateTime Time { get; set; }
    }
}