# brook
A high performance micro sql helper supporting SQL Server、MySql、PostgreSQL

[![NuGet](https://img.shields.io/nuget/v/jIAnSoft.Brook.svg?label=nuget&style=flat-square)](https://www.nuget.org/packages/jIAnSoft.Brook/)

Supported .NET Framework 4.5.1 and .Net Core 2.0

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
      <section name="framework" type="jIAnSoft.Framework.Brook.Configuration.Section, Brook"/>
    </sectionGroup>
  </configSections>
  <jIAnSoft>
    <framework>
      <common name="BrookDemo" culture="zh-TW" timezone="Tokyo Standard Time"/>
      <database>
        <!-- sql server -->
        <add name="mssql" connection="server=127.0.0.1;database=DbName;uid=sa;pwd=7533967" providerName="System.Data.SqlClient"/>
        <!-- If use MySql, It's need download driver https://www.nuget.org/packages/MySql.Data/6.9.9 -->
        <add name="mysql" connection="server=127.0.0.1;uid=root;pwd=7533967;database=DbName" providerName="MySql.Data.MySqlClient"/>
        <!-- If use PostgreSQL need download driver https://www.nuget.org/packages/Npgsql/-->
        <add name="postgresql" connection="Host=127.0.0.1;uid=postgres;pwd=7533967;database=DbName" providerName="Npgsql"/>
      </database>
    </framework>
  </jIAnSoft>
  <system.data>
    <DbProviderFactories>
        <!-- If use MySQL, Need register MySql's provider factory-->
        <remove invariant="MySql.Data.MySqlClient"/>     
        <add name="MySQL Data Provider" invariant="MySql.Data.MySqlClient" description=".Net Framework Data Provider for MySQL" type="MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Version=6.9.9.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d"/>
        <!-- If use PostgreSQL, Need register Npgsql's provider factory -->
        <remove invariant="Npgsql"/>
        <add name="Npgsql Data Provider" invariant="Npgsql" description="Data Provider for PostgreSQL" type="Npgsql.NpgsqlFactory, Npgsql" />
    </DbProviderFactories>
  </system.data>  
</configuration>

```
3.Use examples
``` csharp
//Get query data as DataSet
var postgreDs = Brook.Load("postgresql").DataSet("select * FROM \"public\".\"User\";select * FROM \"public\".\"User\";");

//Get query data as DataTable
var postgreTable = Brook.Load("postgresql").Table("select * FROM \"public\".\"User\"");
var mysqlTable = Brook.Load("mysql").Table(
                    "SELECT `id`,`email`,`name` FROM `pet_user` WHERE `id` = @id",
                    new DbParameter[]
                    {
                        new MySqlParameter("@id", MySqlDbType.Int32)
                        {
                            Value = 1
                        }
                    });
//Insert、Update、Delete operation 
var count = Brook.Load("mysql").Execute(
                    "INSERT INTO `pet_user`( `id`,`email`,`name`)VALUES(@id,@email,@name);"
                    new DbParameter[]
                    {
                        new MySqlParameter("@id", MySqlDbType.Int32)
                        {
                            Value = 1
                        },
                         new MySqlParameter("@email", MySqlDbType.VarChar)
                        {
                            Value = "Email@3q.com"
                        },
                         new MySqlParameter("@name", MySqlDbType.VarChar)
                        {
                            Value = "Eddie"
                        }
                    });
var count = Brook.Load("mssql").Execute("DELETE FROM User WHERE ststus = 0");
var one = Brook.Load("mssql").One<int>("SELECT COUNT(*) FROM User");
var obj = Brook.Load("mssql").First<UserEntityclass>("SELECT [Id],[Name] FROM User WHERE Id=1");
var objs = Brook.Load("mssql").Query<UserEntityclass>("SELECT [Id],[Name] FROM User");

public class UserEntityclass
{
    public long Id {get;set;}
    public string Name {get;set;}
}
```
You can see the full code in DemoNetFramework.

## License

Copyright (c) 2017

Released under the MIT license:

- [www.opensource.org/licenses/MIT](http://www.opensource.org/licenses/MIT)