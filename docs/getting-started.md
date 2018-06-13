# Начало работы

Чтобы писать миграции, подключите в свой проект пакет [ThinkingHome.Migrator.Framework](https://www.nuget.org/packages/ThinkingHome.Migrator.Framework) из NuGet.

```bash
dotnet add package ThinkingHome.Migrator.Framework --version 3.0.0-alpha9
```  

## Пример миграции

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

Обратите внимание:

- Класс миграции унаследован от базового класса `ThinkingHome.Migrator.Framework.Migration`.
- Для миграции указан номер версии (параметр атрибута `[Migration(12)]`), в которую перейдет БД после выполнения изменений, описанных в теле миграции.
- Класс миграции должен реализовывать абстрактный метод `Apply` (применить изменения) и может реализовывать виртуальный метод `Revert` (откат изменений). Если откат изменений не нужен, метод `Revert` можно не переопределять. В этом случае будет использоваться его пустая реализация из базового класса.
- Для изменений БД используется API провайдера трансформации, экземпляр которого доступен через свойство `Database` базового класса `Migration`. API предоставляет методы для выполнения операций над БД: например, `AddTable` (добавление таблицы) или `ExecuteNonQuery` (выполнение произвольного SQL-запроса).

> Подробнее об API провайдера трансформации вы можете узнать в разделе [Описание классов миграций](writing-migrations.md).

## Выполнение миграций

Установите утилиту `migrate-database` из NuGet пакета [ThinkingHome.Migrator.CLI](https://www.nuget.org/packages/ThinkingHome.Migrator.CLI).

```bash
dotnet tool install -g thinkinghome.migrator.cli --version 3.0.0-alpha9
```

Запустите `migrate-database`, указав нужный тип СУБД, строку подключения и путь к сборке с миграциями.

```bash
migrate-database postgres "host=localhost;port=5432;database=migrations;" /path/to/migrations.dll 
```

> Также вы можете выполнить миграции из своего приложения на .NET Core, используя API мигратора. Например, вы можете написать приложение, которое при запуске само создает себе нужную структуру БД. Подробнее об этом в разделе [Как запустить](how-to-run.md). 

## Далее

Узнайте подробнее о [написании миграций](writing-migrations.md) и API провайдеров трансформации.
