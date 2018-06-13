# Поддерживаемые СУБД

Каждая миграция может быть выполнена на любой СУБД, для которой реализован *провайдер трансформации* — специальный класс, отвечающий за генерацию SQL запросов для конкретной СУБД. Провайдер СУБД нужно указывать при запуске миграций на выполнение.

## Как это работает

Класс провайдера СУБД реализует интерфейс `ITransformationProvider` и инкапсулирует в себе всю работу с конкретной СУБД. При запуске миграций на выполнение мигратор создает экземпляр провайдера и передает его каждой миграции во время ее выполнения. Экземпляр провайдера СУБД доступен в методах миграции через свойство `Database`. Методы `Apply` и `Revert` выполняемой миграции вызывают методы объекта `Database`, а созданный экземпляр провайдера выполняет нужные операции над базой данных.

## Готовые провайдеры

Есть уже готовые провайдеры для MS SQL Server, PostgreSQL, MySQL и SQLite.

| **СУБД** | **Короткое имя провайдера** | **NuGet пакет** |
|:-------------|:------------------------|:----------------|
| MS SQL Server |mssql |[ThinkingHome.Migrator.Providers.SqlServer](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.SqlServer)|
| PostgreSQL |postgres |[ThinkingHome.Migrator.Providers.PostgreSQL](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.PostgreSQL)|
| MySQL | mysql |[ThinkingHome.Migrator.Providers.MySql](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.MySql)|
| SQLite |sqlite |[ThinkingHome.Migrator.Providers.SQLite](https://www.nuget.org/packages/ThinkingHome.Migrator.Providers.SQLite)|

## Далее

Прочитайте в разделе [Разработка](development.md), как писать собственные провайдеры трансформациив. 
