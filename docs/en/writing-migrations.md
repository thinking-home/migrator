# Writing migrations

Migrations are .NET classes whose code describes changes to the database using a special API. Classes are compiled into a `.dll` file, having which, you can apply changes to a specific database. 

Migrations inherit from the abstract base class `ThinkingHome.Migrator.Framework.Migration` and implement its abstract `Apply` method. This method contains a description of all the necessary changes to the database. You can also override the `Revert` virtual method of the base class in migrations. There you can describe the actions for rolling back migration changes. For example, if a new table was created in the `Apply` method, then it can be deleted in the `Revert` method. 

### Version control

For each migration, you must specify the version number to which the database will move after applying the changes. To do this, you need to mark the migration class with the `[Migration]` attribute and specify the version number as its parameter.

```c#
using ThinkingHome.Migrator.Framework;

[Migration(12)]
public class MyTestMigration : Migration
{
   // ...
}
```

The version number is a 64-bit integer. You can specify any value of your choice as the version number. For example, it can be a migration sequence number or a timestamp (time stamp). Keep in mind that migrations with a lower number will be completed before migrations with a higher number.

If the `Revert` method is implemented in the migrations, then you can update the database to a version lower than the current one - the migrations will be performed in the reverse order.

### Independent versioning

Migrator can simultaneously keep track of versions for several independent applications in one database. For example, this can be useful when you are writing a modular application where each of the modules has a separate database structure and can be updated independently.  

When performing migrations, information about them is recorded in a special table in the database. In addition to the migration version, the identifier of the assembly in which it is located is written there. By default, it is the same as the `.dll` file name. When launched, the migrator builds an execution plan - a list of migrations that need to be performed in order to transfer the database from the current version to the desired one. When building a plan, only those migrations are taken into account, the assembly ID of which matches the current `.dll` file.

If you need migrations from multiple assemblies to have the same ID, mark each `.dll` file with the `[MigrationAssembly]` attribute, passing the same value as an argument.

```c#
[assembly: MigrationAssembly("my-key")]
```

### Names of database objects

When describing changes, it is often necessary to refer to database objects (tables, columns, indexes, etc.) by name. The name may include the name of the database schema to which the object belongs.

To work with the names of database objects, the `ThinkingHome.Migrator.Framework` package describes a special class `SchemaQualifiedObjectName`. It has two fields: `Name` is the name of the database object and `Schema` is the name of the schema (may be empty). Many of the migrator API methods take arguments of type `SchemaQualifiedObjectName`. 

To make it convenient to work with the names of database objects, automatic typecasting `string` → `SchemaQualifiedObjectName` is made. The following two commands are equivalent:

```c#
// delete table "my_table"
Database.RemoveTable(new SchemaQualifiedObjectName { Name = "my_table" });

// delete table "my_table"
Database.RemoveTable("my_table");
```

If you need to specify a schema name, use the `WithSchema` extension method of the `string` class. The following commands are equivalent:

```c#
// delete table "test.my_table"
Database.RemoveTable(new SchemaQualifiedObjectName { Name = "my_table", Schema = "test" });

// delete table "test.my_table"
Database.RemoveTable("my_table".WithSchema("test"));
``` 

### Column types

A similar situation is with column types. They often require additional information, such as the maximum length for strings or the precision for real numbers.

For convenient work with column types, a special class `ColumnType` is described. It has fields:

- `DataType (System.Data.DbType)` — data type
- `Length (int?)` — length
- `Scale (int?)` — precision

As well as for the names of database objects, implicit type casting `System.Data.DbType` → `ColumnType` and extension methods are implemented for column types.

```c#
// add an INT column "test_integer_column" to the "my_table" table
Database.AddColumn("my_table", new Column("test_integer_column", DbType.Int32));

// add to table "my_table" column "test_string_column" of type NVARCHAR(255)
Database.AddColumn("my_table", new Column("test_string_column", DbType.String.WithSize(255)));

// add a column "test_string_column" of type NVARCHAR(MAX) to the table "my_table"
Database.AddColumn("my_table", new Column("test_string_column", DbType.String.WithSize(int.MaxValue)));

// add a column "test_decimal_column" of type DECIMAL(10, 4) to the table "my_table"
Database.AddColumn("my_table", new Column("test_decimal_column", DbType.Decimal.WithSize(10, 4)));
```


### Conditional operations

Migrator provides the same API for working with different DBMS. It is very comfortable. You can use the same API on different projects, regardless of which DBMS they work with. Another situation where the same API will be useful is when you need to support several different DBMS in one project.

Most of the operations performed by the migrator work the same in all supported DBMSs. An example of such an operation is the creation of a table. For each DBMS, SQL queries will be automatically generated in the required syntax. There are also operations that require different actions to be performed in different DBMSs. An example of such an operation is the creation of a stored procedure. SQL queries for stored procedures cannot be automatically generated. You need to separately implement stored procedures for each DBMS (for example, make several `.sql` files and put them in resources) and, depending on the DBMS, use the necessary SQL query inside the migration.  

For conditional execution of commands, depending on the DBMS, use the `ConditionalExecuteAction` method.

```c#
public override void Apply()
{
    // the assembly that contains the current migration
    var asm = GetType().Assembly;

    // execute SQL scripts from dll resources - different for different DBMS
    Database.ConditionalExecuteAction()
        .For<PostgreSQLTransformationProvider>(db => db.ExecuteFromResource(asm, "file.for.postgres.sql"))
        .For<SqlServerTransformationProvider>(db => db.ExecuteFromResource(asm, "file.for.mssql.sql"))
        .Else(db => db.ExecuteFromResource(asm, "other.sql"));

}
```

## Transform Providers API

### Table operations

Creating a new table:

```c#
void AddTable(SchemaQualifiedObjectName name, params Column[] columns);
```

> The first argument is the title, then the list of table columns. For each column, you must specify a name and type, and you can also specify additional properties (for example, `NOT NULL`) and a default value.

```c#
Database.AddTable("my_table",
    new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
    new Column("Name", DbType.String.WithSize(50), ColumnProperty.NotNull));
```

> To create a table with a composite primary key, you need to specify the `ColumnProperty.PrimaryKey` parameter for several required columns.

```c#
Database.AddTable("CustomerAddress",
    new Column("customer_id", DbType.Int32, ColumnProperty.PrimaryKey),
    new Column("address_id", DbType.Int32, ColumnProperty.PrimaryKey)
);
``` 

Check if a table with the given name exists:

```c#
bool TableExists(SchemaQualifiedObjectName tableName);
```

Get a list of tables in a given schema:

```c#
SchemaQualifiedObjectName[] GetTables(string schema = null);
```

Rename table:

```c#
void RenameTable(SchemaQualifiedObjectName oldName, string newName);
```

Delete table:

```c#
void RemoveTable(SchemaQualifiedObjectName tableName);
```

### Operations with table columns

Add column to table:

```c#
void AddColumn(SchemaQualifiedObjectName table, Column column);
```

Check if a column with the given name exists in the table:

```c#
bool ColumnExists(SchemaQualifiedObjectName table, string column);
```

Rename table column:

```c#
void RenameColumn(SchemaQualifiedObjectName tableName, string oldColumnName, string newColumnName);
```

Change the type of a column or its ability to store a `NULL` value:

```c#
void ChangeColumn(SchemaQualifiedObjectName table, string column, ColumnType columnType, bool notNull);
```

Change the default value for a table column:

```c#
void ChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue);
```

Delete table column:

```c#
void RemoveColumn(SchemaQualifiedObjectName table, string column);
```

### Constraints

Create primary key:

```c#
void AddPrimaryKey(string name, SchemaQualifiedObjectName table, params string[] columns);
```

> You can specify multiple columns to create a composite primary key:

```c#
Database.AddTable("CustomerAddress",
    new Column("customer_id", DbType.Int32),
    new Column("address_id", DbType.Int32)
);

Database.AddPrimaryKey("CustomerAddress", "customer_id", "address_id");
```

Create foreign key:

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

Create unique constraint:

```c#
void AddUniqueConstraint(string name, SchemaQualifiedObjectName table, params string[] columns);
```

Create check constraint:

```c#
void AddCheckConstraint(string name, SchemaQualifiedObjectName table, string checkSql);
```

Check if a constraint with the given name exists:

```c#
bool ConstraintExists(SchemaQualifiedObjectName table, string name);
```

Delete constraint with given name:

```c#
void RemoveConstraint(SchemaQualifiedObjectName table, string name);
```


### Indices

Add index:

```c#
void AddIndex(string name, bool unique, SchemaQualifiedObjectName table, params string[] columns);
```

Check if an index with the given name exists:

```c#
bool IndexExists(string indexName, SchemaQualifiedObjectName tableName);
```

Delete index:

```c#
void RemoveIndex(string indexName, SchemaQualifiedObjectName tableName);
```


### Data operations

Insert record into table:

```c#
int Insert(SchemaQualifiedObjectName table, string[] columns, string[] values);
```

Change values in table rows by condition:

```c#
int Update(SchemaQualifiedObjectName table, string[] columns, string[] values, string whereSql = null);
```

Delete records from a table by condition:

```c#
int Delete(SchemaQualifiedObjectName table, string whereSql = null);
```


### Custom SQL queries

Execute an custom SQL query given by the line:

```c#
int ExecuteNonQuery(string sql);
```

Read data using an custom SQL query:

```c#
IDataReader ExecuteReader(string sql);
```

Get a single value using an custom SQL query:

```c#
object ExecuteScalar(string sql);
```

Execute an custom SQL query contained in a text file in `.dll` resources

```c#
void ExecuteFromResource(Assembly assembly, string path);
```

## Next steps

Learn [how to run migrations](how-to-run.md).
