using System.Collections.Generic;
using System.Data;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;

namespace ThinkingHome.Migrator.Providers
{
    public abstract class TransformationProvider<TConnection> : SqlRunner, ITransformationProvider
        where TConnection : IDbConnection
    {
        protected TransformationProvider(IDbConnection connection) : base(connection)
        {
        }

        public bool NeedQuotesForNames { get; set; }

        public List<long> GetAppliedMigrations(string key = "")
        {
            throw new System.NotImplementedException();
        }

        public void AddColumn(SchemaQualifiedObjectName table, Column column)
        {
            throw new System.NotImplementedException();
        }

        public void AddIndex(string name, bool unique, SchemaQualifiedObjectName table, params string[] columns)
        {
            throw new System.NotImplementedException();
        }

        public bool IndexExists(string indexName, SchemaQualifiedObjectName tableName)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveIndex(string indexName, SchemaQualifiedObjectName tableName)
        {
            throw new System.NotImplementedException();
        }

        public void AddPrimaryKey(string name, SchemaQualifiedObjectName table, params string[] columns)
        {
            throw new System.NotImplementedException();
        }

        public void AddUniqueConstraint(string name, SchemaQualifiedObjectName table, params string[] columns)
        {
            throw new System.NotImplementedException();
        }

        public void AddCheckConstraint(string name, SchemaQualifiedObjectName table, string checkSql)
        {
            throw new System.NotImplementedException();
        }

        public void AddTable(SchemaQualifiedObjectName name, params Column[] columns)
        {
            throw new System.NotImplementedException();
        }

        public void ChangeColumn(SchemaQualifiedObjectName table, string column, ColumnType columnType, bool notNull)
        {
            throw new System.NotImplementedException();
        }

        public void ChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue)
        {
            throw new System.NotImplementedException();
        }

        public bool ColumnExists(SchemaQualifiedObjectName table, string column)
        {
            throw new System.NotImplementedException();
        }

        public bool ConstraintExists(SchemaQualifiedObjectName table, string name)
        {
            throw new System.NotImplementedException();
        }

        public SchemaQualifiedObjectName[] GetTables(string schema = null)
        {
            throw new System.NotImplementedException();
        }

        public int Insert(SchemaQualifiedObjectName table, string[] columns, string[] values)
        {
            throw new System.NotImplementedException();
        }

        public int Insert(SchemaQualifiedObjectName table, object row)
        {
            throw new System.NotImplementedException();
        }

        public int Delete(SchemaQualifiedObjectName table, string whereSql = null)
        {
            throw new System.NotImplementedException();
        }

        public void MigrationApplied(long version, string key)
        {
            throw new System.NotImplementedException();
        }

        public void MigrationUnApplied(long version, string key)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveColumn(SchemaQualifiedObjectName table, string column)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveConstraint(SchemaQualifiedObjectName table, string name)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveTable(SchemaQualifiedObjectName tableName)
        {
            throw new System.NotImplementedException();
        }

        public void RenameTable(SchemaQualifiedObjectName oldName, string newName)
        {
            throw new System.NotImplementedException();
        }

        public void RenameColumn(SchemaQualifiedObjectName tableName, string oldColumnName, string newColumnName)
        {
            throw new System.NotImplementedException();
        }

        public bool TableExists(SchemaQualifiedObjectName tableName)
        {
            throw new System.NotImplementedException();
        }

        public int Update(SchemaQualifiedObjectName table, string[] columns, string[] values, string whereSql = null)
        {
            throw new System.NotImplementedException();
        }

        public int Update(SchemaQualifiedObjectName table, object row, string whereSql = null)
        {
            throw new System.NotImplementedException();
        }

        public bool TypeIsSupported(DbType type)
        {
            throw new System.NotImplementedException();
        }

        public IConditionByProvider ConditionalExecuteAction()
        {
            throw new System.NotImplementedException();
        }

        public void AddForeignKey(string name, SchemaQualifiedObjectName primaryTable, string[] primaryColumns,
            SchemaQualifiedObjectName refTable, string[] refColumns,
            ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
            ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
        {
            throw new System.NotImplementedException();
        }

        public void AddForeignKey(string name, SchemaQualifiedObjectName primaryTable, string primaryColumn,
            SchemaQualifiedObjectName refTable, string refColumn,
            ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
            ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
        {
            throw new System.NotImplementedException();
        }

        public string FormatSql(string format, params object[] args)
        {
            throw new System.NotImplementedException();
        }
    }
}