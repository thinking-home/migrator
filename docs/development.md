# Разработка

## Как собрать проект

Для сборки проекта вам нужно установить [.NET Core 7.0 SDK](https://www.microsoft.com/net/download).

```bash
git clone https://github.com/thinking-home/migrator.git

cd migrator

dotnet restore

dotnet build

```

## Как запустить тесты

Базовая логика мигратора и логика провайдеров трансформации покрыты интеграционными тестами. Для запуска тестов нужен доступ к работающим экземплярам СУБД. Текущие настройки тестов рассчитаны на запуск СУБД в [Docker контейнерах](https://guides.hexlet.io/docker/).

### Запуск MS SQL Server для тестов

```sh
docker run --name mssql -d -p 1433:1433\
    -e 'ACCEPT_EULA=Y'\
    -e 'SA_PASSWORD=x987(!)654'\
    -v $(pwd)/bash/init-mssql.sh:/init-mssql.sh\
    mcr.microsoft.com/mssql/server

docker exec mssql /init-mssql.sh
```

### Запуск MySql для тестов

```sh
docker run --name mysql1 -d -p 3306:3306\
    -e 'MYSQL_ROOT_HOST=%'\
    -e 'MYSQL_ALLOW_EMPTY_PASSWORD=true'\
    -v $(pwd)/bash/init-mysql.sh:/init-mysql.sh\
    mysql/mysql-server

docker exec mysql1 /init-mysql.sh
```

### Запуск PostgreSQL для тестов

```sh
docker run --name postgres -d -p 5432:5432\
    -e POSTGRES_PASSWORD=123\
    -v $(pwd)/bash/init-postgres.sh:/init-postgres.sh\
    postgres

docker exec postgres /init-postgres.sh
```

### Запуск Oracle для тестов

```sh
docker run --name orcl -d -p 1521:1521\
    -v $(pwd)/bash/init-oracle.sh:/init-oracle.sh\
    wnameless/oracle-xe-11g-r2

docker exec orcl /init-oracle.sh
```

### Запуск тестов

После запуска всех нужных СУБД вы можете запустить тесты командой `dotnet test`:

```bash
dotnet test ./ThinkingHome.Migrator.Tests -c Release -f net7.0
```

## Собственные провайдеры трансформации

Если вам нужно использовать мигратор с СУБД, для которой нет готового провайдера, вы можете написать его самостоятельно — это несложно.

Подключите из NuGet в свой проект библиотеки [ThinkingHome.Migrator](https://www.nuget.org/packages/ThinkingHome.Migrator) и [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging/).
Также подключите библиотеку, предоставляющую класс, который реализует интерфейс `System.Data.IDbConnection` для нужной вам СУБД.

### Реализация провайдера

Провайдер СУБД — это класс, унаследованнй от `ThinkingHome.Migrator.TransformationProvider<TConnection>`, где `TConnection` — это класс, реализующий интерфейс `System.Data.IDbConnection` для нужной вам СУБД.

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

Большинство отличий СУБД обычно связаны с типами данных и синтаксисом SQL запросов.

#### Фабрика провайдеров

Чтобы выполнять миграции с помощью своего провайдера вам также потребуется *фабрика провайдеров* - общая точка входа для работы с конкретной СУБД. Фабрика — это класс, который создает во время работы мигратора экземпляры вашего провайдера и открывает подключения к БД.

Класс фабрики провайдеров должен быть унаследован от `ProviderFactory<TProvider, TConnection>` и должен реализовывать его абстрактные методы `CreateProviderInternal` и `CreateConnectionInternal`.

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

После этого вы можете выполнять миграции с помощью своего провайдера, указав вместо названия СУБД класс своей **фабрики провайдеров** (полное название класса с названием сборки):

```bash
migrate-database "MyAssembly.MyNamespace.MyTransformationProvider, MyAssembly" "my-connection-string" /path/to/migrations.dll
```

#### Типы данных

В разных СУБД одни и те же типы данных обозначаются разными ключевыми словами. Например, для 32-разрядного целого числа в PostgreSQL используется тип `int4`, а в MySQL — тип `INTEGER`.

API мигратора использует для работы с типами колонок перечисление `System.Data.DbType` и каждый провайдер задает собственные правила сопоставления значения `DbType` с типами СУБД.

В классе провайдера в поле `typeMap` доступны настройки сопоставления типов. Вы можете добавить нужные правила сопоставления, например, в конструкторе.

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

API мигратора позволяет указывать размер для значений столбцов. Например, вы можете создать столбец для хранения строк, длина которых не превышает 80 символов:

```c#
Database.AddColumn("my_table",
    new Column("test_string_column", DbType.String.WithSize(80)));
```

Вы можете задать разные типы СУБД, в зависимости от указанного размера. Вместо заглушки `$l` (length) будет подставлен нужный размер.

```c#
// если размер не указан, используем тип NVARCHAR(255)
typeMap.Put(DbType.String, "NVARCHAR(255)");

// если указан размер менее 4 тыс символов, то используем тип "NVARCHAR(<размер>)
typeMap.Put(DbType.String, 4000, "NVARCHAR($l)");

// если указан размер более 4 тыс символов, то используем NVARCHAR(MAX)
typeMap.Put(DbType.String, int.MaxValue, "NVARCHAR(MAX)");
```

Вы можете использовать заглушку `$s` (scale), которая будет заменена на указанное значение точности для типа. Также вы можете указать при вызове метода `Put` последний необязательный параметр — значение точности по умолчанию.

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

#### Генерация SQL запросов

Синтаксис SQL запросов в разных СУБД иногда совпадает и иногда отличается. В базовом классе `ThinkingHome.Migrator.TransformationProvider<T>` уже реализована генерация запросов для всех методов API. Логика формирования SQL вынесена в виртуальные методы. Если для вашей СУБД необходимо формировать SQL запросы по другому, переопределите методы базового класса.

Посмотрите в качестве примера [код провайдера](https://github.com/thinking-home/migrator/blob/master/ThinkingHome.Migrator.Providers.SqlServer/SqlServerTransformationProvider.cs) для MS SQL Server.

### Тесты

API провайдеров трансформации покрыт интеграционными тестами. Тесты — удобный инструмент для проверки работы собственного провайдера.

- опишите класс своего провайдера, унаследованный от `ThinkingHome.Migrator.TransformationProvider<T>`
- опишите класс для тестов, унаследованный от базового класса [TransformationProviderTestBase](https://github.com/thinking-home/migrator/blob/master/ThinkingHome.Migrator.Tests/TransformationProviderTestBase.cs)
- переопределите у класса с тестами виртуальные методы и свойства:
  - метод `CreateProvider` — создайте здесь экземпляр собственного провайдера
  - метод `GetSchemaForCompare` — должен возвращать название схемы по умолчанию для СУБД
  - свойство `BatchSql` — должно возвращать текст запроса, состоящего из нескольких простых запросов, разделенных нужным разделителем (например, `GO` для MS SQL Server)
  - свойство `ResourceSql` — должно возвращать путь к тестовому файлу `.sql` в ресурсах текущей dll (он будет использоваться для проверки выполнения запросов из ресурсов dll)
- запустите тесты, посмотрите, что упало и почините его (обычно для этого нужно переопределить генерацию запроса в провайдере, но иногда нужно переопределить тест и поменять его логику)
