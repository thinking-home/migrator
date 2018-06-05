# Начало работы


## Пример класса миграции

```c#
[Migration(1)]
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

Подробнее о написании миграций — в разделе [Описание классов миграций](writing-migrations.md).

## Выполнение миграций

Установите утилиту `migrate-database` из NuGet пакета [ThinkingHome.Migrator.CLI](https://www.nuget.org/packages/ThinkingHome.Migrator.CLI).

```bash
dotnet tool install -g thinkinghome.migrator.cli --version 3.0.0-alpha9
```

Запустите `migrate-database`, указав нужный тип СУБД, строку подключения и путь к сборке с миграциями.

```bash
migrate-database postgres "host=localhost;port=5432;database=migrations;user name=postgres;password=123" /path/to/migrations.dll 
```
