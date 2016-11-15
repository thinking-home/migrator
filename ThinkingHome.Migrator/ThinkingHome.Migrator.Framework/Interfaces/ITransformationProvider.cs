using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace ThinkingHome.Migrator.Framework.Interfaces
{
    /// <summary>
    /// The main interface to use in Migrations to make changes on a database schema.
    /// </summary>
    public interface ITransformationProvider : IDisposable
    {
        IDbConnection Connection { get; }
        int CommandTimeout { get; set; }
        bool NeedQuotesForNames { get; set; }

        /// <summary>
        /// The list of Migrations currently applied to the database.
        /// </summary>
        List<long> GetAppliedMigrations(string key = "");

        /// <summary>
        /// Add a column to an existing table
        /// </summary>
        /// <param name="table">The name of the table that will get the new column</param>
        /// <param name="column">An instance of a <see cref="Column">Column</see> with the specified properties</param>
        void AddColumn(SchemaQualifiedObjectName table, Column column);

        /// <summary>
        /// Define a new index
        /// </summary>
        /// <param name="name">Name of new index</param>
        /// <param name="unique">Sign that the index is unique</param>
        /// <param name="table">Table name</param>
        /// <param name="columns">Columns</param>
        void AddIndex(string name, bool unique, SchemaQualifiedObjectName table, params string[] columns);

        /// <summary>
        /// Check that the index with the specified name already exists
        /// </summary>
        /// <param name="indexName">�������� �������</param>
        /// <param name="tableName">�������� �������</param>
        /// <returns></returns>
        bool IndexExists(string indexName, SchemaQualifiedObjectName tableName);

        /// <summary>
        /// Deleting index
        /// </summary>
        /// <param name="indexName">Index name</param>
        /// <param name="tableName">Table name</param>
        void RemoveIndex(string indexName, SchemaQualifiedObjectName tableName);

        /// <summary>
        /// Add a primary key to a table
        /// </summary>
        /// <param name="name">The name of the primary key to add.</param>
        /// <param name="table">The name of the table that will get the primary key.</param>
        /// <param name="columns">The name of the column or columns that are in the primary key.</param>
        void AddPrimaryKey(string name, SchemaQualifiedObjectName table, params string[] columns);

        /// <summary>
        /// Add a constraint to a table
        /// </summary>
        /// <param name="name">The name of the constraint to add.</param>
        /// <param name="table">The name of the table that will get the constraint</param>
        /// <param name="columns">The name of the column or columns that will get the constraint.</param>
        void AddUniqueConstraint(string name, SchemaQualifiedObjectName table, params string[] columns);

        /// <summary>
        /// Add a constraint to a table
        /// </summary>
        /// <param name="name">The name of the constraint to add.</param>
        /// <param name="table">The name of the table that will get the constraint</param>
        /// <param name="checkSql">The check constraint definition.</param>
        void AddCheckConstraint(string name, SchemaQualifiedObjectName table, string checkSql);

        /// <summary>
        /// Add new table
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="columns">Columns collection</param>
        void AddTable(SchemaQualifiedObjectName name, params Column[] columns);

        /// <summary>
        /// Start a transction
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Change the definition of an existing column.
        /// </summary>
        /// <param name="table">The name of the table that will get the new column</param>
        /// <param name="column">�������� ���������� ������� �������</param>
        /// <param name="columnType">����� ��� �������</param>
        /// <param name="notNull">�������: ��������� �������� NULL</param>
        void ChangeColumn(SchemaQualifiedObjectName table, string column, ColumnType columnType, bool notNull);

        /// <summary>
        /// ��������� �������� �� ���������
        /// </summary>
        /// <param name="table">�������� �������</param>
        /// <param name="column">�������� �������</param>
        /// <param name="newDefaultValue">����� �������� �� ���������</param>
        void ChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue);

        /// <summary>
        /// Check to see if a column exists
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        bool ColumnExists(SchemaQualifiedObjectName table, string column);

        /// <summary>
        /// Commit the running transction
        /// </summary>
        void Commit();

        /// <summary>
        /// Check to see if a constraint exists
        /// </summary>
        /// <param name="name">The name of the constraint</param>
        /// <param name="table">The table that the constraint lives on.</param>
        /// <returns></returns>
        bool ConstraintExists(SchemaQualifiedObjectName table, string name);

        /// <summary>
        /// Execute an arbitrary SQL query
        /// </summary>
        /// <param name="sql">The SQL to execute.</param>
        /// <returns></returns>
        int ExecuteNonQuery(string sql);

        /// <summary>
        /// Execute an arbitrary SQL query
        /// </summary>
        /// <param name="sql">The SQL to execute.</param>
        /// <returns></returns>
        IDataReader ExecuteReader(string sql);

        /// <summary>
        /// Execute an arbitrary SQL query
        /// </summary>
        /// <param name="sql">The SQL to execute.</param>
        /// <returns>A single value that is returned.</returns>
        object ExecuteScalar(string sql);

        /// <summary>
        /// Get the names of all of the tables
        /// </summary>
        /// <returns>The names of all the tables.</returns>
        SchemaQualifiedObjectName[] GetTables(string schema = null);

        /// <summary>
        /// Insert data into a table
        /// </summary>
        /// <param name="table">The table that will get the new data</param>
        /// <param name="columns">The names of the columns</param>
        /// <param name="values">The values in the same order as the columns</param>
        /// <returns></returns>
        int Insert(SchemaQualifiedObjectName table, string[] columns, string[] values);

        /// <summary>
        /// Insert data into a table
        /// </summary>
        /// <param name="table">The table that will get the new data</param>
        /// <param name="row">Data for insert</param>
        /// <returns></returns>
        int Insert(SchemaQualifiedObjectName table, object row);

        /// <summary>
        /// Delete data from a table
        /// </summary>
        /// <param name="table">The table that will have the data deleted</param>
        /// <param name="whereSql">Condition for select deleting rows</param>
        /// <returns></returns>
        int Delete(SchemaQualifiedObjectName table, string whereSql = null);

        /// <summary>
        /// Marks a Migration version number as having been applied
        /// </summary>
        /// <param name="version">The version number of the migration that was applied</param>
        /// <param name="key">Key of migration series</param>
        void MigrationApplied(long version, string key);

        /// <summary>
        /// Marks a Migration version number as having been rolled back from the database
        /// </summary>
        /// <param name="version">The version number of the migration that was removed</param>
        /// <param name="key">Key of migration series</param>
        void MigrationUnApplied(long version, string key);

        /// <summary>
        /// Remove an existing column from a table
        /// </summary>
        /// <param name="table">The name of the table to remove the column from</param>
        /// <param name="column">The column to remove</param>
        void RemoveColumn(SchemaQualifiedObjectName table, string column);

        /// <summary>
        /// Remove an existing constraint
        /// </summary>
        /// <param name="table">The table that contains the foreign key.</param>
        /// <param name="name">The name of the constraint to remove</param>
        void RemoveConstraint(SchemaQualifiedObjectName table, string name);

        /// <summary>
        /// Remove an existing table
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        void RemoveTable(SchemaQualifiedObjectName tableName);

        /// <summary>
        /// Rename an existing table
        /// </summary>
        /// <param name="oldName">The old name of the table</param>
        /// <param name="newName">The new name of the table</param>
        void RenameTable(SchemaQualifiedObjectName oldName, string newName);

        /// <summary>
        /// Rename an existing table
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <param name="oldColumnName">The old name of the column</param>
        /// <param name="newColumnName">The new name of the column</param>
        void RenameColumn(SchemaQualifiedObjectName tableName, string oldColumnName, string newColumnName);

        /// <summary>
        /// Rollback the currently running transaction.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Check if a table already exists
        /// </summary>
        /// <param name="tableName">The name of the table that you want to check on.</param>
        /// <returns></returns>
        bool TableExists(SchemaQualifiedObjectName tableName);

        /// <summary>
        /// Update the values in a table
        /// </summary>
        /// <param name="table">The name of the table to update</param>
        /// <param name="columns">The names of the columns.</param>
        /// <param name="values">The values for the columns in the same order as the names.</param>
        /// <param name="whereSql">A whereSql clause to limit the update</param>
        /// <returns></returns>
        int Update(SchemaQualifiedObjectName table, string[] columns, string[] values, string whereSql = null);

        int Update(SchemaQualifiedObjectName table, object row, string whereSql = null);

        IDbCommand GetCommand(string sql = null);

        bool TypeIsSupported(DbType type);

        #region For

        IConditionByProvider ConditionalExecuteAction();

        #endregion

        void AddForeignKey(
            string name,
            SchemaQualifiedObjectName primaryTable,
            string[] primaryColumns,
            SchemaQualifiedObjectName refTable,
            string[] refColumns,
            ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
            ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction);

        void AddForeignKey(
            string name,
            SchemaQualifiedObjectName primaryTable,
            string primaryColumn,
            SchemaQualifiedObjectName refTable,
            string refColumn,
            ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
            ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction);

        void ExecuteFromResource(Assembly assembly, string path);

        string FormatSql(string format, params object[] args);
    }
}