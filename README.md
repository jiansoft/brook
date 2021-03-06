# brook
A high performance micro sql helper supporting SQL Server、MySql、PostgreSQL

[![NuGet](https://img.shields.io/nuget/v/jIAnSoft.Brook.svg?label=nuget&style=flat-square)](https://www.nuget.org/packages/jIAnSoft.Brook/)

Supported .NET Framework 4.6.1、.NET Standard 2.0 and .NET Standard 2.1

Usage
================

### Quick Start
1. Install Nuget package. [here](https://www.nuget.org/packages/jIAnSoft.Brook/).

2. For .NET Framework project, Add some parameters in web.conf or app.config.
Parameters format like below.
``` xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <sectionGroup name="jIAnSoft">
      <section name="framework" type="jIAnSoft.Brook.Configuration.Section, Brook"/>
    </sectionGroup>
  </configSections>
  <jIAnSoft>
    <framework>
      <common name="BrookDemo" culture="zh-TW" timezone="Tokyo Standard Time"/>
      <database>
        <!-- sql server -->
        <add name="mssql" connection="server=127.0.0.1;database=DbName;uid=sa;pwd=7533967" providerName="System.Data.SqlClient"/>
        <add name="sqlserver" connection="server=127.0.0.1;database=DbName;uid=sa;pwd=7533967" providerName="Microsoft.Data.SqlClient"/>
        <!-- If use MySql, It's need download driver https://www.nuget.org/packages/MySql.Data/6.9.9 -->
        <add name="mysql" connection="server=127.0.0.1;uid=root;pwd=7533967;database=DbName" providerName="MySql.Data.MySqlClient"/>
        <!-- If use PostgreSQL need download driver https://www.nuget.org/packages/Npgsql/-->
        <add name="postgresql" connection="Host=127.0.0.1;uid=postgres;pwd=7533967;database=DbName" providerName="Npgsql"/>
        <add name="sqlite" connection="data source=.\\brook.sqlite" providerName="System.Data.SQLite" commandTimeout="5" />
      </database>
    </framework>
  </jIAnSoft>
  <system.data>
    <DbProviderFactories>
        <!-- If use MySQL, Need register MySql's provider factory-->
        <remove invariant="MySql.Data.MySqlClient" />
        <add name="MySQL Data Provider" invariant="MySql.Data.MySqlClient" description=".Net Framework Data Provider for MySQL" type="MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data" />     
        <!-- If use PostgreSQL, Need register Npgsql's provider factory -->
        <remove invariant="Npgsql"/>
        <add name="Npgsql Data Provider" invariant="Npgsql" description="Data Provider for PostgreSQL" type="Npgsql.NpgsqlFactory, Npgsql" />
        <remove invariant="System.Data.SQLite" />
        <add name="SQLite Data Provider" invariant="System.Data.SQLite" description=".NET Framework Data Provider for SQLite" type="System.Data.SQLite.SQLiteFactory, System.Data.SQLite" />     
        <remove invariant="Microsoft.Data.SqlClient" />
        <add name="Microsoft SqlClient Data Provider" invariant="Microsoft.Data.SqlClient" description=".Net Framework Data Provider for SqlServer" type="Microsoft.Data.SqlClient.SqlClientFactory, Microsoft.Data.SqlClient" />
    </DbProviderFactories>
  </system.data>  
</configuration>

```
3.Use examples
``` csharp
//Get query data as DataSet
var postgreDs = Brook.Load("postgresql").DataSet("select * FROM public.account;select * FROM public.account;");

//Get query data as DataTable
var postgreTable = Brook.Load("postgresql").Table("select * FROM public.account;");
var mysqlTable = Brook.Load("mysql").Table(
    "SELECT `id`,`email`,`name` FROM `account` WHERE `id` = @id;",
    new DbParameter[]
    {
        new MySqlParameter("@id", MySqlDbType.Int32)
        {
            Value = 1
        }
    });

//Insert、Update、Delete operation 
var count = Brook.Load("mysql").Execute(
    "INSERT INTO `account`(`id`,`email`,`name`)VALUES(@id,@email,@name);"
    new DbParameter[]
    {
        new MySqlParameter("@id", MySqlDbType.Int32)
        {
            Value = 1
        },
        new MySqlParameter("@email", MySqlDbType.VarChar)
        {
            Value = "email@3q.com"
        },
        new MySqlParameter("@name", MySqlDbType.VarChar)
        {
            Value = "example"
        }
    });
var count = Brook.Load("mssql").Execute("DELETE FROM [account] WHERE [Id] = 0");
var one = Brook.Load("mssql").One<int>("SELECT COUNT(*) FROM Account");
var obj = Brook.Load("mssql").First<Account>("SELECT [Id],[Name],[Email] FROM [account] WHERE Id=1");
var objs = Brook.Load("mssql").Query<Account>("SELECT [Id],[Name],[Email] FROM [account]");

public class Account
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}
```
You can see more example code in the example projects.

## License

Copyright (c) 2017

Released under the MIT license:

- [www.opensource.org/licenses/MIT](http://www.opensource.org/licenses/MIT)