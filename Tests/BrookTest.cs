using IdGen;
using jIAnSoft.Brook;
using jIAnSoft.Brook.Mapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Tests
{
    [TestFixture]
    public class BrookTest
    {
        private static readonly IdGenerator Generator = new IdGenerator(0);

        private readonly string[] _sqlType =
        {
            "mysql",
            //"posql",
            //"sqlite",
            "mssql"
        };

        private const string Query = "SELECT TOP 5 {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} ORDER BY {id} DESC LIMIT 5;";
        private const string Insert = "insert into {account} ({name},{email})values(@name, @email) ";
        private const string Update = "Update {account} SET {name} = '';";
        private const string Delete = "DELETE FROM {account} WHERE {id} = @id;";

        private const string FindById =
            "SELECT {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} where {id} = @id;";

        private const string FindByName =
            "SELECT {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} where {name} = @name;";

        private static string ConvertSeparate(string sql, DatabaseType dt = DatabaseType.SQLite)
        {
            switch (dt)
            {
                case DatabaseType.SQLServer:
                    return sql.Replace("{", "[").Replace("}", "]").Replace("LIMIT 5", "");
                case DatabaseType.PostgreSQL:
                    return sql.Replace("{", "\"").Replace("}", "\"").Replace("TOP 5", "");
                case DatabaseType.MySQL:
                    return sql.Replace("{", "`").Replace("}", "`").Replace("TOP 5", "");
                default:
                    return sql.Replace("{", "").Replace("}", "").Replace("TOP 5", "");
            }
        }

        private static string GetLastId(string sql, DatabaseType dt = DatabaseType.SQLite)
        {
            switch (dt)
            {
                case DatabaseType.SQLServer:
                    return $"{sql}; SELECT IDENT_CURRENT ('account') AS Current_Identity;";
                case DatabaseType.PostgreSQL:
                    return $"{sql} RETURNING id;";
                case DatabaseType.MySQL:
                    return $"{sql}; SELECT LAST_INSERT_ID();";
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
            SqlMapper db = null;

            try
            {
                db = Brook.Load("sqlserver");
                var name = $"我是交易1-{DateTime.Now:HHmmss}";
                db.BeginTransaction();
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                var t = db.Table(ConvertSeparate(FindByName, DatabaseType.SQLServer), new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[Table] Id = {t.Rows[0]["Id"]} Name = {t.Rows[0]["Name"]} Email = {t.Rows[0]["Email"]} ");
                db.CommitTransaction();

                name = $"我是交易2-{DateTime.Now:HHmmss}";
                db.BeginTransaction();
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                var account = db.First<Account>(ConvertSeparate(FindByName, DatabaseType.SQLServer), new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[First] Id = {account.Id} Name = {account.Name} Email = {account.Email}");
                var accounts = db.Query<Account>(ConvertSeparate(FindByName, DatabaseType.SQLServer), new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[Query] Id = {accounts[0].Id} Name = {accounts[0].Name} Email = {accounts[0].Email}");
                
                db.CommitTransaction();

                name = $"我是交易3-{DateTime.Now:HHmmss}";
                db.BeginTransaction();
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                db.RollbackTransaction();

                name = $"我不是交易1-{DateTime.Now:HHmmss}";
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                var ds = db.DataSet(ConvertSeparate(FindByName, DatabaseType.SQLServer), new[] { db.Parameter("@name", name) });
                TestContext.WriteLine($"[DataSet] Id = {ds.Tables[0].Rows[0]["Id"]} Name = {ds.Tables[0].Rows[0]["Name"]} Email = {ds.Tables[0].Rows[0]["Email"]}");

                
                name = $"我是交易4-{DateTime.Now:HHmmss}";
                db.BeginTransaction();
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                db.ChangeDatabase("Order");
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
                using (var db = Brook.Load("sqlserver"))
                {
                    db.Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseType.SQLServer));
                }

                var sw = new Stopwatch();
                sw.Reset();
                sw.Start();
                Brook.Load("sqlserver").Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseType.SQLServer));
                sw.Stop();
                TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");
                
                sw.Reset();
                sw.Start();
                Brook.Load("sqlserver").Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseType.SQLServer));
                sw.Stop();
                TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");
                
                sw.Reset();
                sw.Start();
                Brook.Load("sqlserver").Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseType.SQLServer));
                sw.Stop();
                TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");
                
                sw.Reset();
                sw.Start();
                var l1 = Brook.Load("sqlserver").Query<Account>(15, CommandType.Text, ConvertSeparate(Query, DatabaseType.SQLServer));
                sw.Stop();
                TestContext.WriteLine($"Query 总毫秒:{sw.ElapsedMilliseconds}");
                
                TestContext.WriteLine($"Account : {l1.Count}");
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
                db.Execute(15, CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                var account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseType.SQLServer),
                    new[] {db.Parameter("@name", name)});
                TestContext.WriteLine($"[1] Id = {account.Id} Name = {account.Name} Email = {account.Email}");

                name = $"{Generator.CreateId()}";
                db.Execute(CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseType.SQLServer),
                    new[] {db.Parameter("@name", name)});
                TestContext.WriteLine($"[2] Id = {account.Id} Name = {account.Name} Email = {account.Email}");

                name = $"{Generator.CreateId()}";
                db.Execute(ConvertSeparate(Insert, DatabaseType.SQLServer), new[]
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseType.SQLServer),
                    new[] {db.Parameter("@name", name)});
                TestContext.WriteLine($"[3] Id = {account.Id} Name = {account.Name} Email = {account.Email}");

                name = $"{Generator.CreateId()}";
                db.Execute(ConvertSeparate(Insert, DatabaseType.SQLServer), new List<DbParameter[]>
                {
                    new[]
                    {
                        db.Parameter("@name", name),
                        db.Parameter("@email", $"{name}@sqlserver.com")
                    }
                });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseType.SQLServer),
                    new[] {db.Parameter("@name", name)});
                TestContext.WriteLine($"[4] Id = {account.Id} Name = {account.Name} Email = {account.Email}");

                name = $"{Generator.CreateId()}";
                var count = db.Execute(CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer),
                    new List<DbParameter[]>
                    {
                        new[]
                        {
                            db.Parameter("@name", name),
                            db.Parameter("@email", $"{name}@sqlserver.com")
                        }
                    });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseType.SQLServer),
                    new[] {db.Parameter("@name", name)});
                TestContext.WriteLine(
                    $"[5] Id = {account.Id} Name = {account.Name} Email = {account.Email} count:{JsonConvert.SerializeObject(count)}");

                name = $"{Generator.CreateId()}";
                count = db.Execute(5, CommandType.Text, ConvertSeparate(Insert, DatabaseType.SQLServer),
                    new List<DbParameter[]>
                    {
                        new[]
                        {
                            db.Parameter("@name", name),
                            db.Parameter("@email", $"{name}@sqlserver.com")
                        }
                    });
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseType.SQLServer),
                    new[] {db.Parameter("@name", name)});
                TestContext.WriteLine(
                    $"[6] Id = {account.Id} Name = {account.Name} Email = {account.Email} count:{JsonConvert.SerializeObject(count)}");

                name = $"{Generator.CreateId()}";
                db.Execute($"insert into [account] ([name],[email])values('{name}', '{name}@sqlserver.com');");
                account = db.First<Account>(
                    ConvertSeparate(FindByName, DatabaseType.SQLServer),
                    new[] {db.Parameter("@name", name)});
                TestContext.WriteLine($"[7] Id = {account.Id} Name = {account.Name} Email = {account.Email}");
            }
        }

        [Test]
        public void TestLoad()
        {
            foreach (var dbName in _sqlType)
            {
                DatabaseType dt;
                switch (dbName)
                {
                    case "mssql":
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
                        break;
                }

                var providerNme = Enum.GetName(typeof(DatabaseType), dt);
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
                DatabaseType dt;
                switch (dbName)
                {
                    case "mssql":
                        continue;
                    /*dt = DatabaseType.SQLServer;
                    break;*/
                    case "posql":
                        dt = DatabaseType.PostgreSQL;
                        break;
                    case "mysql":
                        dt = DatabaseType.MySQL;
                        break;
                    default:
                        continue;
                    /*dt = DatabaseType.SQLite;
                    using (var db = jIAnSoft.Brook.Mapper.Brook.Load(dbName))
                    {
                        db.Execute(
                            @"CREATE TABLE IF NOT EXISTS account (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT not null, email TEXT not null);");
                    }

                    break;*/
                }

                var providerNme = Enum.GetName(typeof(DatabaseType), dt);
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
                DatabaseType dt;
                switch (dbName)
                {
                    case "mssql":
                        //continue;
                        dt = DatabaseType.SQLServer;
                        break;
                    case "posql":
                        dt = DatabaseType.PostgreSQL;
                        break;
                    case "mysql":
                        dt = DatabaseType.MySQL;
                        break;
                    default:
                        continue;
                    /*dt = DatabaseType.SQLite;
                    using (var db = jIAnSoft.Brook.Mapper.Brook.Load(dbName))
                    {
                        db.Execute(@"CREATE TABLE IF NOT EXISTS account (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT not null, email TEXT not null);");
                    }

                    break;*/
                }

                var providerNme = Enum.GetName(typeof(DatabaseType), dt);
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
                        new[] {db.Parameter("@id", id, DbType.Int32)});
                    TestContext.WriteLine($"{providerNme} first  {first.Id} {first.Name} {first.Email}");

                    var query = db.Query<Account>(ConvertSeparate(Query, dt));
                    foreach (var acc in query)
                    {
                        TestContext.WriteLine($"{providerNme} query  {acc.Id} {acc.Name} {acc.Email}");
                    }

                    var ds = db.DataSet(
                        ConvertSeparate(FindById, dt),
                        new[] {db.Parameter("@id", 1, DbType.Int32)});
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        TestContext.WriteLine($"{providerNme} dataSet  {row["Id"]} {row["name"]} {row["email"]}");
                    }

                    var table = db.Table(
                        ConvertSeparate(FindById, dt),
                        new[] {db.Parameter("@id", 1, DbType.Int32)});
                    foreach (DataRow row in table.Rows)
                    {
                        TestContext.WriteLine($"{providerNme} table  {row["Id"]} {row["name"]} {row["email"]}");
                    }

                    var count = db.Execute(
                        CommandType.Text,
                        ConvertSeparate(Delete, dt),
                        new[] {db.Parameter("@id", id, DbType.Int64)});
                    TestContext.WriteLine($"{providerNme} delete id:{id} count:{count}");

                    count = db.Execute(
                        1,
                        CommandType.Text,
                        ConvertSeparate(Delete, dt),
                        new[] {db.Parameter("@id", id, DbType.Int64)});
                    TestContext.WriteLine($"{providerNme} delete id:{id} count:{count}");

                    count = db.Execute(
                        1,
                        CommandType.Text,
                        ConvertSeparate(Delete, dt),
                        new[] {db.Parameter("@id", id, DbType.Int64)});
                    TestContext.WriteLine($"{providerNme} delete id:{id} count:{count}");

                    count = db.Execute(ConvertSeparate(Update, dt));
                    TestContext.WriteLine($"{providerNme} update count:{count}");

                    var counts = db.Execute(
                        ConvertSeparate(Delete, dt),
                        new List<DbParameter[]> {new[] {db.Parameter("@id", id, DbType.Int64)}});
                    foreach (var i in counts)
                    {
                        TestContext.WriteLine($"{providerNme} delete id:{id} count:{i}");
                    }
                }
            }
        }
    }

    public class Account
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}