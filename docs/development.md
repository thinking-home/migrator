# Разработка

## Как собрать проект

Для сборки проекта вам нужно установить [.NET Core 2.1 SDK](https://www.microsoft.com/net/download).

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
    microsoft/mssql-server-linux

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
    -v $(pwd)/bash/init-postgres.sh:/init-postgres.sh\
    postgres

docker exec postgres /init-postgres.sh
```

### Запуск тестов

После запуска всех нужных СУБД вы можете запустить тесты командой `dotnet test`:

```bash
dotnet test ./ThinkingHome.Migrator.Tests -c Release -f netcoreapp2.1
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

### Фабрика провайдеров

### Как запустить

### Тесты