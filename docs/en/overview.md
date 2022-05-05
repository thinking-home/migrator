# Overview

ThinkingHome.Migrator is a database version control system for .NET Core similar to EntityFramework Migrations. The goal is to automate making changes to the database (for example, when deploying or updating an application) and to provide database version control.

## Main idea

All changes to the database are recorded in the code as *migrations* - classes written in a programming language (for example, in C #).

Migrations are compiled into a `.dll` file. After that, you can make changes to the database using the console utility [migrate-database](https://www.nuget.org/packages/ThinkingHome.Migrator.CLI), passing the database connection string and the path to the ` .dll file in the parameters `with migrations.

## Description of changes

Migration classes inherit from the base class `ThinkingHome.Migrator.Framework.Migration` and implement its methods `Apply` (apply changes) and `Revert` (roll back changes).

Inside these methods, using the [special API](writing-migrations.md) the developer describes the actions to be performed on the database. For example, adding a column would look like this:

```c#
public override void Apply()
{
    Database.AddColumn("Table1", new Column("Num", DbType.Int32));
}
```

It is also possible to execute arbitrary SQL.

## Version control

The version number that the database will switch to after making the changes described by the migration has to be indicated for each migration.

Versions are tracked automatically: information about completed migrations is stored in the database in a special table. Having an assembly with migrations in which the `Apply` and `Revert` methods are implemented, you can update the database of any version to any other version (both above and below the current one).

> Unlike EntityFramework migrations, ThinkingHome.Migrator can keep track of versions for several independent applications in one database. For example, this can be useful if you are writing a modular application, each module of which has its own database structure and can be independently updated.

## Supported DBMS

ThinkingHome.Migrator provides the same API for managing different DBMSs. When starting migrations, you need to specify the name of the *transformation provider* that will generate SQL queries for the required DBMS.

The ThinkingHome.Migrator project has already implemented providers for MS SQL Server, PostgreSQL, Oracle, MySQL and SQLite. You can [implement your provider](development.md) for any DBMS you need.

## Next steps

[Getting Started](getting-started.md)
