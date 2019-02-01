using IdGen;
using jIAnSoft.Brook;
using jIAnSoft.Brook.Mapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Tests
{
    [TestFixture]
    public class BrookTest
    {
        private static readonly IdGenerator Generator = new IdGenerator(0);
        private readonly string[] _sqlType = {
            "mysql",
            "posql",
            "sqlite",
             "mssql"
        };
        private const string Query = "SELECT TOP 5 {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} ORDER BY {id} DESC LIMIT 5;";
        private const string Insert = "insert into {account} ({name},{email})values(@name, @email) ";
        private const string Update = "Update {account} SET {name} = '';";
        private const string Delete = "DELETE FROM {account} WHERE {id} = @id;";
        private const string FindById = "SELECT {id} AS {Id},{name} AS {Name},{email} AS {Email} FROM {account} where {id} = @id;";

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
                        continue;
                        //dt = DatabaseType.SQLServer;
                       // break;
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
                        new[] {db.Parameter("@id", 1, DbType.Int32)});
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
                        new[] { db.Parameter("@id", id, DbType.Int64) });
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