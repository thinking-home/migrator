# Supported DBMS

Each migration can be executed on any DBMS for which *transformation provider* is implemented - a special class responsible for generating SQL queries for a specific DBMS. The DBMS provider must be specified when running run migrations.

## How it works

The DBMS provider class implements the `ITransformationProvider` interface and encapsulates the implementation of all operations with a specific DBMS. When running migrations, the migrator creates an instance of the provider and passes it to each migration as it runs. The DBMS provider instance is available in the migration methods via the `Database` property. The `Apply` and `Revert` methods of the migration in progress call the methods of the `Database` object, and the created provider instance performs the necessary operations on the database.

## Built-in DBMS providers

Migrator contains built-in providers for MS SQL Server, PostgreSQL, Oracle, MySQL and SQLite.

| **DBMS** | **Provider short name** | **NuGet package**                                                                                                       |
|:-------------|:------------------------|:------------------------------------------------------------------------------------------------------------------------|
| MS SQL Server |sqlserver | [ThinkingHome.Migrator.Providers.SqlServer](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.SqlServer)   |
| PostgreSQL |postgres | [ThinkingHome.Migrator.Providers.PostgreSQL](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.PostgreSQL) |
| Oracle |oracle | [ThinkingHome.Migrator.Providers.Oracle](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.Oracle)         |
| MySQL | mysql | [ThinkingHome.Migrator.Providers.MySql](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.MySql)           |
| SQLite |sqlite | [ThinkingHome.Migrator.Providers.SQLite](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.SQLite)         |

## Next steps

Read in the [Development](development.md) section how to write your own transformation providers.
