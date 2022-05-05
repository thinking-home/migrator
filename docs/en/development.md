# Development

## How to build a project

To build the project you need to install [.NET Core 5.0 SDK](https://www.microsoft.com/net/download).

```bash
git clone https://github.com/thinking-home/migrator.git

cd migrator

dotnet restore

dotnet build

```

## How to run tests

The basic logic of the migrator and the logic of the transformation providers are covered by the integration tests. To run tests, you need access to running instances of the DBMS. The current test settings are designed to run the DBMS in [Docker containers](https://guides.hexlet.io/docker/).

### Running MS SQL Server for tests

```sh
docker run --name mssql -d -p 1433:1433\
    -e 'ACCEPT_EULA=Y'\
    -e 'SA_PASSWORD=x987(!)654'\
    -v $(pwd)/bash/init-mssql.sh:/init-mssql.sh\
    microsoft/mssql-server-linux

docker exec mssql /init-mssql.sh
```

### Running MySql for tests

```sh
docker run --name mysql1 -d -p 3306:3306\
    -e 'MYSQL_ROOT_HOST=%'\
    -e 'MYSQL_ALLOW_EMPTY_PASSWORD=true'\
    -v $(pwd)/bash/init-mysql.sh:/init-mysql.sh\
    mysql/mysql-server

docker exec mysql1 /init-mysql.sh
```

### Running PostgreSQL for tests

```sh
docker run --name postgres -d -p 5432:5432\
    -v $(pwd)/bash/init-postgres.sh:/init-postgres.sh\
    postgres

docker exec postgres /init-postgres.sh
```

### Running Oracle for tests

```sh
docker run --name orcl -d -p 1521:1521\
    -v $(pwd)/bash/init-oracle.sh:/init-oracle.sh\
    wnameless/oracle-xe-11g-r2

docker exec orcl /init-oracle.sh
```

### Running tests

After starting all the necessary DBMS, you can run the tests with the command `dotnet test`:

```bash
dotnet test ./ThinkingHome.Migrator.Tests -c Release -f net5.0
```

## Own transformation providers

If you need to use a migrator with a DBMS for which there is no ready-made provider, you can write it yourself - it's easy.

Connect [ThinkingHome.Migrator](https://www.nuget.org/packages/ThinkingHome.Migrator) and [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging/) libraries from NuGet to your project.
Also include a library that provides a class that implements the `System.Data.IDbConnection` interface for the DBMS you need.

### Provider implementation

A DBMS provider is a class inherited from `ThinkingHome.Migrator.TransformationProvider<TConnection>`, where `TConnection` is a class that implements the System.`Data.IDbConnection` interface for the DBMS you need.

```c#
public class MyTransformationProvider : TransformationProvider<MyConnection>
{
    public MyTransformationProvider(MyConnection connection, ILogger logger)
        : base(connection, logger)
    {
        // ...
    }

    // ...
}
```

Most of the differences between DBMS are usually related to data types and syntax of SQL queries.

#### Provider factory

To perform migrations using your provider, you also need a *provider factory* - a common entry point for working with a specific DBMS. A factory is a class that creates instances of your provider while the migrator is running and opens connections to the database.

The provider factory class must inherit from `ProviderFactory<TProvider, TConnection>` and must implement its `CreateProviderInternal` and `CreateConnectionInternal` abstract methods.

```c#
public class MyProviderFactory :
    ProviderFactory<MyTransformationProvider, MyConnection>
{
    protected override MyTransformationProvider CreateProviderInternal(MyConnection connection, ILogger logger)
    {
        return new MyTransformationProvider(connection, logger);
    }

    protected override MyConnection CreateConnectionInternal(string connectionString)
    {
        return new MyConnection(connectionString);
    }
}
```

After that, you can perform migrations using your provider, specifying the class of your **provider factory** (full class name with the assembly name) instead of the DBMS name:

```bash
migrate-database "MyAssembly.MyNamespace.MyTransformationProvider, MyAssembly" "my-connection-string" /path/to/migrations.dll
```

#### Data tyes

Different DBMSs use different keywords for the same data types. For example, PostgreSQL uses the `int4` type for a 32-bit integer, while MySQL uses the `INTEGER` type.

The migrator API uses the `System.Data.DbType` enumeration to work with column types, and each provider sets its own rules for mapping the `DbType` value to the DBMS types.

Type mapping settings are available in the provider class in the `typeMap` field. You can add matching rules you want, for example, in the constructor.

```c#
public class MyTransformationProvider : TransformationProvider<MyConnection>
{
    public MyTransformationProvider(MyConnection connection, ILogger logger)
        : base(connection, logger)
    {
        // типы данных
        typeMap.Put(DbType.Int16, "SMALLINT");
        typeMap.Put(DbType.Int32, "INTEGER");
        typeMap.Put(DbType.Int64, "BIGINT");
    }
}
```

The migrator API allows you to specify the size for column values. For example, you can create a column to store strings that are no more than 80 characters long:

```c#
Database.AddColumn("my_table",
    new Column("test_string_column", DbType.String.WithSize(80)));
```

You can specify different types of DBMS, depending on the specified size. The stub `$l` (length) will be replaced with the desired size.

```c#
// if the size is not specified, use the NVARCHAR (255) type
typeMap.Put(DbType.String, "NVARCHAR(255)");

// if the specified size is less than 4 thousand characters, then use the type "NVARCHAR (<size>)
typeMap.Put(DbType.String, 4000, "NVARCHAR($l)");

// if the specified size is more than 4 thousand characters, then use NVARCHAR (MAX)
typeMap.Put(DbType.String, int.MaxValue, "NVARCHAR(MAX)");
```

You can use the `$ s` (scale) stub, which will be replaced with the specified precision value for the type. You can also specify the last optional parameter when calling the `Put` method - the default precision value.

```c#
typeMap.Put(DbType.Decimal, "DECIMAL");
typeMap.Put(DbType.Decimal, 38, "DECIMAL($l, $s)", 2);

// ...

// DECIMAL
Database.AddColumn("my_table",
    new Column("test_decimal_column", DbType.Decimal));

// DECIMAL(10, 4)
Database.AddColumn("my_table",
    new Column("test_decimal_column", DbType.Decimal.WithSize(10, 4)));

// DECIMAL(10, 2)
Database.AddColumn("my_table",
    new Column("test_decimal_column", DbType.Decimal.WithSize(10)));
```

#### Generating SQL queries

The syntax of SQL queries in different DBMSs sometimes coincides and sometimes differs. The base class `ThinkingHome.Migrator.TransformationProvider<T>` already implements request generation for all API methods. The SQL formation logic has been moved to virtual methods. If your DBMS needs to form SQL queries differently, override the methods of the base class.

Take a look at [provider code](https://github.com/thinking-home/migrator/blob/master/ThinkingHome.Migrator.Providers.SqlServer/SqlServerTransformationProvider.cs) for MS SQL Server as an example.

### Tests

The transformation providers API is covered with integration tests. Tests are a convenient tool for checking the work of your own provider.

- Describe your provider class inherited from `ThinkingHome.Migrator.TransformationProvider<T>`
- Describe a class for tests, inherited from the base class [TransformationProviderTestBase](https://github.com/thinking-home/migrator/blob/master/ThinkingHome.Migrator.Tests/TransformationProviderTestBase.cs)
- Override the virtual methods and properties of the class with tests:
  - Method `CreateProvider` - create an instance of your own provider here
  - method `GetSchemaForCompare` - should return the default schema name for the DBMS
  - Property `BatchSql` - should return the text of a query, consisting of several simple queries separated by the necessary separator (for example,`GO` for MS SQL Server)
  - Property `ResourceSql` - should return the path to the test file`.sql` in the resources of the current dll (it will be used to check the execution of requests from the resources of the dll)
- Run the tests, see what has fallen and fix it (usually, this requires overriding the generation of the request in the provider, but sometimes you need to override the test and change its logic)
