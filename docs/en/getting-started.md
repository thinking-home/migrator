# Getting Started

To write migrations, include the [ThinkingHome.Migrator.Framework](https://www.nuget.org/packages/ThinkingHome.Migrator.Framework) package from NuGet into your project.

```bash
dotnet add package ThinkingHome.Migrator.Framework
```  

## Migration example

```c#
using ThinkingHome.Migrator.Framework;

[Migration(12)]
public class MyTestMigration : Migration
{
    public override void Apply()
    {
        Database.AddTable("CustomerAddress",
            new Column("customerId", DbType.Int32, ColumnProperty.PrimaryKey),
            new Column("addressId", DbType.Int32, ColumnProperty.PrimaryKey));
    }

    public override void Revert()
    {
        Database.RemoveTable("CustomerAddress");
    }
}
```

Thinks to note:

- The migration class is inherited from the base class `ThinkingHome.Migrator.Framework.Migration`.
- The migration version number is specified in the parameter of the `[Migration (12)]` attribute.
- The migration class **must** implement the abstract `Apply` method, and can implement the virtual `Revert` method. If you do not need to roll back the changes, you can leave the `Revert` method not overridden. In this case, its empty implementation from the base class will be used.
- To change the database, the transformation provider API is used, an instance of which is available through the `Database` property of the `Migration` base class. The API provides methods for performing operations on the database: for example, `AddTable` (add a table) or `ExecuteNonQuery` (execute an arbitrary SQL query).

> You can learn more about the transformation provider API in the [Description of migration classes](writing-migrations.md).

## Performing migrations

Install the `migrate-database` utility from the NuGet package [ThinkingHome.Migrator.CLI](https://www.nuget.org/packages/ThinkingHome.Migrator.CLI).

```bash
dotnet tool install -g thinkinghome.migrator.cli
```

Run `migrate-database` specifying the required type of DBMS, connection string and path to the assembly with migrations.

```bash
migrate-database postgres "host=localhost;port=5432;database=migrations;" /path/to/migrations.dll 
```

> You can also migrate from your application to .NET Core using the migrator API. For example, you can write an application that, when launched, creates the desired database structure for itself. More about this in the [How to run](how-to-run.md) section.

## Next steps

Learn more about [writing migrations] (writing-migrations.md) and the transformation provider API.
