# Как запустить

Миграции можно выполнить при помощи консольного приложения `migrate-database` и через API. При запуске нужно указать СУБД, строку подключения и сборку (`.dll`) с миграциями.

При запуске можно опционально указать версию БД. Мигратор по очереди будет выполнять метод `Apply` у миграций в порядке возрастания номера версии, пока не дойдет до заданной. Если целевая версия меньше текущей, то произойдет откат БД с использованием метода `Revert` соответствующих миграций.

Если не указывать номер версии при старте, будет выполнена миграция до последней доступной версии. Также для миграции до последней версии можно указать значение `-1`.

## Консольное приложение

Самый простой способ выполнить миграции — использовать консольное приложение `migrate-database`. Вы можете установить его из NuGet пакета [ThinkingHome.Migrator.CLI](https://www.nuget.org/packages/ThinkingHome.Migrator.CLI) как глобальную утилиту .NET Core.

```bash
dotnet tool install -g thinkinghome.migrator.cli --version 3.0.0-alpha9
```

При запуске нужно указать три обязательных параметра: тип СУБД, строку подключения и путь к сборке (файлу `.dll`) с миграциями: 

```bash
migrate-database <db_provider> <conntection_string> <migrations_dll_path> 
```

Например:

```bash
migrate-database postgres "host=localhost;port=5432;database=migrations;" /path/to/migrations.dll 
```

Список доступных провайдеров смотрите в разделе [Поддерживаемые СУБД](dialects.md).

Вы можете также указать дополнительные (необязательные) параметры:

- **--list** — вывести список доступных миграций, не выполняя их.
- **--version <version>** — целевая версия БД. Значение по умолчанию — `-1` (обновить БД до последней доступной версии)  
- **--timeout <timeout>** — таймаут на выполнение SQL запросов (в секундах).
- **--verbose** — выводить в консоль текст выполняемых SQL запросов.
- **-?** | **-h** | **--help** — вывести справку.

## API

Вы можете выполнять миграции из своего приложения через API мигратора. Например, вы можете написать приложение, которое при запуске само себе создает нужную структуру БД. 

Сначала подключите в свой проект пакет [ThinkingHome.Migrator](https://www.nuget.org/packages/ThinkingHome.Migrator) из NuGet и пакет с [провайдером трансформации для нужной СУБД](https://www.nuget.org/packages?q=ThinkingHome.Migrator.Providers). 

После этого создайте экземпляр класса `ThinkingHome.Migrator.Migrator` и вызовите его метод `Migrate`, передав в качестве параметра целевую версию БД.

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

В конструктор класса `Migrator` вы можете передать последним аргументом экземпляр `ILogger` ([Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging/)). В этот логгер будут записана вся информация во время выполнения миграций.

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

## Далее

[Поддерживаемые СУБД](dialects.md).
