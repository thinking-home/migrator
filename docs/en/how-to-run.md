# How to run migrations

Migrations can be executed using the `migrate-database` console application and via the API. When starting, you need to specify the DBMS, connection string and assembly (`.dll`) with migrations.

You can optionally specify the target version of the database. The migrator will execute the `Apply` method on migrations one by one in ascending order of the version number until it reaches the given one. If the target version is less than the current version, then the database will be rolled back using the `Revert` method of the corresponding migrations.

If you don't specify a version number, it will migrate to the latest available version. You can also specify `-1` to migrate to the latest version.

## CLI

The easiest way to perform migrations is to use the `migrate-database` CLI. You can install it from the NuGet package [ThinkingHome.Migrator.CLI](https://www.nuget.org/packages/ThinkingHome.Migrator.CLI) as a global .NET Core utility.

```bash
dotnet tool install -g thinkinghome.migrator.cli
```

You need to specify three required parameters: DBMS type, connection string and path to the assembly (`.dll` file) with migrations: 

```bash
migrate-database <db_provider> <conntection_string> <migrations_dll_path> 
```

Example:

```bash
migrate-database postgres "host=localhost;port=5432;database=migrations;" /path/to/migrations.dll 
```

See the list of available providers in the [Supported DBMS](dialects.md) section.

You can also specify additional (optional) parameters:

- **--list** - list available migrations without running them.
- **--version <version>** — target database version. The default value is `-1` (update the database to the latest available version)
- **--timeout <timeout>** — timeout for executing SQL queries (in seconds).
- **--verbose** — output the text of executed SQL queries to the console.
- **-?** | **-h** | **--help** Display help.

## API

You can perform migrations from your application through the Migrator API. For example, you can write an application that, when launched, creates the necessary database structure on its own. 

First, include in your project the [ThinkingHome.Migrator](https://www.nuget.org/packages/ThinkingHome.Migrator) package from NuGet and the package with the [transformation provider for the desired DBMS](https://www.nuget.org /packages?q=ThinkingHome.Migrator.Providers). 

After that, create an instance of the `ThinkingHome.Migrator.Migrator` class and call its `Migrate` method, passing the target database version as a parameter.

```c#
var version = -1;
var provider = "postgres";
var connectionString = "host=localhost;port=5432;database=migrations;";
var assembly = Assembly.LoadFrom("/path/to/migrations.dll");

using (var migrator = new Migrator(provider, connectionString, assembly))
{
    migrator.Migrate(version);
}
```

You can pass an `ILogger` instance ([Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging/)) as the last argument to the `Migrator` class constructor. All information will be written to this logger during migrations.

```c#
var version = -1;
var provider = "postgres";
var connectionString = "host=localhost;port=5432;database=migrations;";
var assembly = Assembly.LoadFrom("/path/to/migrations.dll");

using (var loggerFactory = new LoggerFactory())
{
    loggerFactory.AddConsole(LogLevel.Trace);

    var logger = loggerFactory.CreateLogger("my-logger-name");

    using (var migrator = new Migrator(provider, connectionString, assembly, logger))
    {
        migrator.Migrate(version);
    }
}

```

## Supported DBMS

[Поддерживаемые СУБД](dialects.md).
