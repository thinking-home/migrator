# Создание миграций

Миграции - это классы .NET, в коде которых описаны изменения БД с помощью специального API. Классы компилируются в файл `.dll`, имея который, можно применить изменения для конкретной БД. 

Миграции наследуются от абстрактного базового класса `ThinkingHome.Migrator.Framework.Migration` и реализуют его абстрактный метод `Apply`. В этом методе находится описание всех нужных имзменений БД. Также в миграциях можно переопределить виртуальный метод `Revert` базового класса. Там можно описать действия для отката изменений миграции. Например, если в методе `Apply` была создана новая таблица, то в методе `Revert` можно её удалить. 

### Контроль версий

Для каждой миграции нужно указать номер версии, в которую перейдет БД после применения изменений миграции. Для этого нужно отметить класс миграции атрибутом `[Migration]` и указать номер версии как его параметр.

```c#
using ThinkingHome.Migrator.Framework;

[Migration(12)]
public class MyTestMigration : Migration
{
   // ...
}
```

Номер версии - это целое число (`long`). Можно указывать любое значение на свой выбор, например, порядковый номер или timestamp (временную метку). Главное - помнить, что миграции с меньшим номером будут выполнены раньше, чем миграции с б́ольшим номером.

Если в миграциях реализован метод `Revert`, то можно обновить БД до версии, ниже текущей - миграции будут выполнены в обратном порядке.




### SchemaQualifiedObjectName

### Типы столбцов

### Условные операции

```c#
IConditionByProvider ConditionalExecuteAction();
```

## API провайдеров трансформации

### Операции с таблицами

```c#
void AddTable(SchemaQualifiedObjectName name, params Column[] columns);
```

```c#
bool TableExists(SchemaQualifiedObjectName tableName);
```

```c#
SchemaQualifiedObjectName[] GetTables(string schema = null);
```

```c#
void RenameTable(SchemaQualifiedObjectName oldName, string newName);
```

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

