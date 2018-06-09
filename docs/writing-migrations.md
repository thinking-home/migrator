# Создание миграций

Миграции - это классы .NET, в коде которых описаны изменения БД с помощью специального API. Классы компилируются в файл `.dll`, имея который, можно применить изменения для конкретной БД. 

Миграции наследуются от абстрактного базового класса `ThinkingHome.Migrator.Framework.Migration` и реализуют его абстрактный метод `Apply`. В этом методе находится описание всех нужных имзменений БД. Также в миграциях можно переопределить виртуальный метод `Revert` базового класса. Там можно описать действия для отката изменений миграции. Например, если в методе `Apply` была создана новая таблица, то в методе `Revert` можно её удалить. 

### Контроль версий

Для каждой миграции нужно указать номер версии, в которую перейдет БД после применения изменений. Для этого нужно отметить класс миграции атрибутом `[Migration]` и указать номер версии как его параметр.

```c#
using ThinkingHome.Migrator.Framework;

[Migration(12)]
public class MyTestMigration : Migration
{
   // ...
}
```

Номер версии — это 64-разрядное целое число. Вы можете указать в качестве номера версии любое значение на свой выбор. Например, это может быть порядковый номер миграции или timestamp (временная метка). Главное — помнить, что миграции с меньшим номером будут выполнены раньше, чем миграции с бо́льшим номером.

Если в миграциях реализован метод `Revert`, то можно обновить БД до версии, ниже текущей — миграции будут выполнены в обратном порядке.

### Параллельное версионирование

Мигратор может параллельно вести в одной БД учет версий для нескольких независимых приложений. Например, это может быть полезно, когда вы пишете модульное приложение, в которм каждый из модулей имеет отдельную структуру БД и может независимо обновляться.  

При выполнении миграций информация о них записывается в специальную таблицу в БД. Кроме версии миграции, туда записывается идентификатор сборки, в которой она находится. По умолчанию он совпадает с именем файла `.dll`. При запуске мигратор строит план выполнения — список миграций, которые нужно выполнить, чтобы перевести БД из текущей версии в нужную. При построении плана учитываются только те миграции, идентификатор сборки которых совпадает с текущим `.dll` файлом.

Если вам нужно, чтобы у миграций из нескольких сборок был одинаковый идентификатор, отметьте каждый файл `.dll` атрибутом `[MigrationAssembly]`, передав в качестве аргумента одинаковое значение.

```c#
[assembly: MigrationAssembly("my-key")]
```

### Названия объектов БД

При описании изменений часто нужно ссылаться на объекты БД (таблицы, столбцы, индексы и т.д.) по имени. Имя может включать название схемы БД, к которой относится объект.

Для работы с названиями объектов БД в пакете `ThinkingHome.Migrator.Framework` описан специальный класс `SchemaQualifiedObjectName`. У него есть два поля: `Name` — название объекта БД и `Schema` — название схемы (может быть пустым). Многие методы API мигратора принимают аргументы с типом `SchemaQualifiedObjectName`. 

Чтобы было удобно работать с именами объектов БД, сделано автоматическое приведение типов `string` → `SchemaQualifiedObjectName`. Следующие две команды — эквивалентны:

```c#
// удаление таблицы "my_table"
Database.RemoveTable(new SchemaQualifiedObjectName { Name = "my_table" });

// удаление таблицы "my_table"
Database.RemoveTable("my_table");
```

Если нужно указать название схемы, используйте extension method `WithSchema` класса `string`. Следующие записи — эквивалентны:

```c#
// удаление таблицы "test.my_table"
Database.RemoveTable(new SchemaQualifiedObjectName { Name = "my_table", Schema = "test" });

// удаление таблицы "test.my_table"
Database.RemoveTable("my_table".WithSchema("test"));
``` 

### Типы столбцов

Пожожая ситуация — с типами столбцов. Для них часто нужно указывать дополнительную информацию, например, максимальную длину для строк или точность для вещественных чисел.

Для удобной работы с типами столбцов описан специальный класс `ColumnType`. У него есть поля:

- `DataType (System.Data.DbType)` — тип данных
- `Length (int?)` — длина
- `Scale (int?)` — точность

Как и для названий объектов БД, для типов столбцов сделано неявное приведение типов `System.Data.DbType` → `ColumnType` и методы расширения.

```c#
// добавить в таблицу "my_table" колонку "test_integer_column" типа INT
Database.AddColumn("my_table", new Column("test_integer_column", DbType.Int32));

// добавить в таблицу "my_table" колонку "test_string_column" типа NVARCHAR(255)
Database.AddColumn("my_table", new Column("test_string_column", DbType.String.WithSize(255)));

// добавить в таблицу "my_table" колонку "test_string_column" типа NVARCHAR(MAX)
Database.AddColumn("my_table", new Column("test_string_column", DbType.String.WithSize(int.MaxValue)));

// добавить в таблицу "my_table" колонку "test_string_column" типа NVARCHAR(MAX)
Database.AddColumn("my_table", new Column("TestStringColumn", DbType.String.WithSize(7)));
```


### Условные операции

Мигратор предоставляет одинаковый API для работы с разными СУБД. Это очень удобно. Вы можете использовать на разных проектах один и тот же API, вне зависимости от того, с какой СУБД они работают. Еще одна ситуация, где одинаковый API будет полезен — когда в одном проекте нужно поддерживать несколько разных СУБД.

Большинство операций, которые выполняет мигратор, одинаково работают во всех поддерживаемых СУБД. Пример такой операции — создание таблицы. Для каждой СУБД автоматически будут сгенерированы SQL запросы в нужном синтаксисе. Есть также операции, для выполнения которых в разных СУБД нужно сделать разные действия. Пример такой операции — создание хранимой процедуры. SQL запросы для хранимых процедур нельзя матоматически сгенерировать. Вам нужно отдельно реализовать хранимые процедуры для каждой СУБД (например, сделать несколько `.sql` файлов и положить их в ресурсы) и в зависимости от СУБД, внутри миграции использовать нужный SQL запрос.  

Для условного выполнения команд, в зависимости от СУБД, используйте метод `ConditionalExecuteAction`.

```c#
public override void Apply()
{
    // сборка, в которой находится текущая миграция
    var asm = GetType().Assembly;

    // выполнить SQL скрипты из ресурсов dll - hазные для разных СУБД
    Database.ConditionalExecuteAction()
        .For<PostgreSQLTransformationProvider>(db => db.ExecuteFromResource(asm, "file.for.postgres.sql"))
        .For<SqlServerTransformationProvider>(db => db.ExecuteFromResource(asm, "file.for.mssql.sql"))
        .Else(db => db.ExecuteFromResource(asm, "other.sql"));

}
```

## API провайдеров трансформации

### Операции с таблицами

Создание новой таблицы:

```c#
void AddTable(SchemaQualifiedObjectName name, params Column[] columns);
```

> Первый аргумент - название, дальше - список столбцов таблицы. Для каждого столбца нужно указать название и тип, а также можно указать дополнительные свойства (например, `NOT NULL`)

```c#
Database.AddTable("my_table",
    new Column("Id", DbType.Int32, ColumnProperty.Identity),
    new Column("Name", DbType.String.WithSize(50), ColumnProperty.NotNull));
```

Проверить, что существует таблица с заданным именем

```c#
bool TableExists(SchemaQualifiedObjectName tableName);
```

Получить список таблиц в заданной схеме

```c#
SchemaQualifiedObjectName[] GetTables(string schema = null);
```

Переименовать таблицу

```c#
void RenameTable(SchemaQualifiedObjectName oldName, string newName);
```

Удалить таблицу

```c#
void RemoveTable(SchemaQualifiedObjectName tableName);
```

### Операции со столбцами таблиц

```c#
void AddColumn(SchemaQualifiedObjectName table, Column column);
```

```c#
bool ColumnExists(SchemaQualifiedObjectName table, string column);
```

```c#
void RenameColumn(SchemaQualifiedObjectName tableName, string oldColumnName, string newColumnName);
```

```c#
void ChangeColumn(SchemaQualifiedObjectName table, string column, ColumnType columnType, bool notNull);
```

```c#
void ChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue);
```

```c#
void RemoveColumn(SchemaQualifiedObjectName table, string column);
```


### Ограничения

```c#
void AddPrimaryKey(string name, SchemaQualifiedObjectName table, params string[] columns);
```

```c#
void AddForeignKey(
            string name,
            SchemaQualifiedObjectName primaryTable,
            string[] primaryColumns,
            SchemaQualifiedObjectName refTable,
            string[] refColumns,
            ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
            ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction);
```

```c#
void AddUniqueConstraint(string name, SchemaQualifiedObjectName table, params string[] columns);
```

```c#
void AddCheckConstraint(string name, SchemaQualifiedObjectName table, string checkSql);
```

```c#
bool ConstraintExists(SchemaQualifiedObjectName table, string name);
```

```c#
void RemoveConstraint(SchemaQualifiedObjectName table, string name);
```


### Индексы

```c#
void AddIndex(string name, bool unique, SchemaQualifiedObjectName table, params string[] columns);
```

```c#
bool IndexExists(string indexName, SchemaQualifiedObjectName tableName);
```

```c#
void RemoveIndex(string indexName, SchemaQualifiedObjectName tableName);
```


### Операции с данными

```c#
int Insert(SchemaQualifiedObjectName table, string[] columns, string[] values);
```

```c#
int Insert(SchemaQualifiedObjectName table, object row);
```

```c#
int Update(SchemaQualifiedObjectName table, string[] columns, string[] values, string whereSql = null);
```

```c#
int Update(SchemaQualifiedObjectName table, object row, string whereSql = null);
```

```c#
int Delete(SchemaQualifiedObjectName table, string whereSql = null);
```


### Произвольные SQL запросы

```c#
int ExecuteNonQuery(string sql);
```

```c#
IDataReader ExecuteReader(string sql);
```

```c#
object ExecuteScalar(string sql);
```

```c#
void ExecuteFromResource(Assembly assembly, string path);
```

