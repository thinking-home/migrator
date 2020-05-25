using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;
using ForeignKeyConstraint = ThinkingHome.Migrator.Framework.ForeignKeyConstraint;

namespace ThinkingHome.Migrator.Providers.Oracle
{
    public class OracleTransformationProvider : TransformationProvider<OracleConnection>
    {
        public OracleTransformationProvider(OracleConnection connection, ILogger logger) : base(connection, logger)
        {
            typeMap.Put(DbType.AnsiStringFixedLength, "CHAR(255)");
            typeMap.Put(DbType.AnsiStringFixedLength, 2000, "CHAR($l)");
            typeMap.Put(DbType.AnsiString, "VARCHAR2(255)");
            typeMap.Put(DbType.AnsiString, 2000, "VARCHAR2($l)");
            typeMap.Put(DbType.AnsiString, 2147483647, "CLOB"); // should use the IType.ClobType
            typeMap.Put(DbType.Binary, "RAW(2000)");
            typeMap.Put(DbType.Binary, 2000, "RAW($l)");
            typeMap.Put(DbType.Binary, 2147483647, "BLOB");
            typeMap.Put(DbType.Boolean, "NUMBER(1,0)");
            typeMap.Put(DbType.Byte, "NUMBER(3,0)");
            typeMap.Put(DbType.Currency, "NUMBER(19,1)");
            typeMap.Put(DbType.Date, "DATE");
            typeMap.Put(DbType.DateTime, "TIMESTAMP(4)");
            typeMap.Put(DbType.Decimal, "NUMBER");
            typeMap.Put(DbType.Decimal, 38, "NUMBER($l, $s)", 2);
            // having problems with both ODP and OracleClient from MS not being able
            // to read values out of a field that is DOUBLE PRECISION
            typeMap.Put(DbType.Double, "BINARY_DOUBLE");
            typeMap.Put(DbType.Guid, "RAW(16)");
            typeMap.Put(DbType.Int16, "NUMBER(5,0)");
            typeMap.Put(DbType.Int32, "NUMBER(10,0)");
            typeMap.Put(DbType.Int64, "NUMBER(18,0)");
            typeMap.Put(DbType.Single, "FLOAT(24)");
            typeMap.Put(DbType.StringFixedLength, "NCHAR(255)");
            typeMap.Put(DbType.StringFixedLength, 2000, "NCHAR($l)");
            typeMap.Put(DbType.String, "NVARCHAR2(255)");
            typeMap.Put(DbType.String, 2000, "NVARCHAR2($l)");
            typeMap.Put(DbType.String, 1073741823, "NCLOB");
            typeMap.Put(DbType.Time, "DATE");

            propertyMap.RegisterPropertySql(ColumnProperty.Null, String.Empty);

            fkActionMap.RegisterSql(ForeignKeyConstraint.NoAction, string.Empty);
        }

        public override string BatchSeparator => "/";

        #region generate sql

        public override void AddForeignKey(string name, SchemaQualifiedObjectName primaryTable, string[] primaryColumns, SchemaQualifiedObjectName refTable, string[] refColumns, ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction, ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
        {
            if (onUpdateConstraint != ForeignKeyConstraint.NoAction)
            {
                throw new NotSupportedException("Oracle does not support actions when updating a foreign key");
            }

            if (onDeleteConstraint == ForeignKeyConstraint.SetDefault)
            {
                throw new NotSupportedException("Oracle does not support SET DEFAULT when deleting a record referenced by a foreign key");
            }

            base.AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns, onDeleteConstraint, onUpdateConstraint);
        }

        protected override string GetSqlDefaultValue(object defaultValue)
        {
            // convert boolean to number (1, 0)
            if (defaultValue is bool)
            {
                defaultValue = (bool)defaultValue ? 1 : 0;
            }
            else if (defaultValue == null)
            {
                defaultValue = "NULL";
            }

            return base.GetSqlDefaultValue(defaultValue);
        }

        protected override string GetSqlColumnDef(Column column, bool compoundPrimaryKey)
        {
            var sqlBuilder = new ColumnSqlBuilder(column, typeMap, propertyMap, GetQuotedName);

            sqlBuilder.AppendColumnName();
            sqlBuilder.AppendColumnType(IdentityNeedsType);
            sqlBuilder.AppendDefaultValueSql(GetSqlDefaultValue);
            sqlBuilder.AppendNotNullSql(NeedsNotNullForIdentity);
            sqlBuilder.AppendPrimaryKeySql(compoundPrimaryKey);
            sqlBuilder.AppendUniqueSql();

            return sqlBuilder.ToString();
        }

        protected override string GetSqlAddColumn(SchemaQualifiedObjectName table, string columnSql)
        {
            return FormatSql("ALTER TABLE {0:NAME} ADD ({1})", table, columnSql);
        }

        protected override string GetSqlChangeColumnType(SchemaQualifiedObjectName table, string column, ColumnType columnType)
        {
            string columnTypeSql = typeMap.Get(columnType);

            return FormatSql("ALTER TABLE {0:NAME} MODIFY {1:NAME} {2}", table, column, columnTypeSql);
        }

        protected override string GetSqlChangeNotNullConstraint(SchemaQualifiedObjectName table, string column, bool notNull, ref string sqlChangeColumnType)
        {
            string colsTable = table.SchemaIsEmpty ? "USER_TAB_COLUMNS" : "ALL_TAB_COLUMNS";

            string sqlCheckNotNull = FormatSql(
                "select {0:NAME} from {1:NAME} where {2:NAME} = '{3}' and {4:NAME} = '{5}'",
                "NULLABLE", colsTable, "TABLE_NAME", table.Name, "COLUMN_NAME", column);

            if (!table.SchemaIsEmpty)
            {
                sqlCheckNotNull += FormatSql(" and {0:NAME} = '{1}'", "OWNER", table.Schema);
            }

            using (var reader = ExecuteReader(sqlCheckNotNull))
            {
                if (reader.Read())
                {
                    bool columnAlreadyNotNull = reader[0].ToString().Equals("n", StringComparison.CurrentCultureIgnoreCase);
                    if (notNull == columnAlreadyNotNull)
                    {
                        return null;
                    }
                }
            }

            return base.GetSqlChangeNotNullConstraint(table, column, notNull, ref sqlChangeColumnType);
        }

        protected override string GetSqlChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue)
        {
            return FormatSql("ALTER TABLE {0:NAME} MODIFY {1:NAME} {2}", table, column, GetSqlDefaultValue(newDefaultValue));
        }

        protected override string GetSqlAddIndex(string name, bool unique, SchemaQualifiedObjectName table, params string[] columns)
        {
            if (columns.Length == 0) throw new ArgumentException("Not specified columns of the table to create an index", nameof(columns));

            string uniqueString = unique ? "UNIQUE" : string.Empty;
            string sql = FormatSql("CREATE {0} INDEX {1:NAME} ON {2:NAME} ({3:COLS})",
                uniqueString, name.WithSchema(table.Schema), table, columns);

            return sql;
        }

        protected override string GetSqlRemoveIndex(string indexName, SchemaQualifiedObjectName tableName)
        {
            return FormatSql("DROP INDEX {0:NAME}", indexName.WithSchema(tableName.Schema));
        }

        #endregion

        #region DDL

        public override bool ColumnExists(SchemaQualifiedObjectName table, string column)
        {
            string columnsTableName = table.SchemaIsEmpty ? "USER_TAB_COLUMNS" : "ALL_TAB_COLUMNS";

            string sql = FormatSql(
                "SELECT COUNT(*) from {0:NAME} where {1:NAME} = '{2}' and {3:NAME} = '{4}'",
                columnsTableName, "TABLE_NAME", table.Name, "COLUMN_NAME", column);

            if (!table.SchemaIsEmpty)
            {
                sql += FormatSql(" and {0:NAME} = '{1}'", "OWNER", table.Schema);
            }

            object scalar = ExecuteScalar(sql);
            return Convert.ToInt32(scalar) > 0;
        }

        public override bool ConstraintExists(SchemaQualifiedObjectName table, string name)
        {
            string constraintsTableName = table.SchemaIsEmpty ? "USER_CONSTRAINTS" : "ALL_CONSTRAINTS";

            string sql = FormatSql(
                "SELECT COUNT(*) FROM {0:NAME} WHERE {1:NAME} = '{2}' AND {3:NAME} = '{4}'",
                constraintsTableName, "CONSTRAINT_NAME", name, "TABLE_NAME", table.Name);

            if (!table.SchemaIsEmpty)
            {
                sql += FormatSql(" and {0:NAME} = '{1}'", "OWNER", table.Schema);
            }

            object scalar = ExecuteScalar(sql);
            return Convert.ToInt32(scalar) > 0;
        }

        public override bool IndexExists(string indexName, SchemaQualifiedObjectName tableName)
        {
            string indexesTableName = tableName.SchemaIsEmpty ? "USER_INDEXES" : "ALL_INDEXES";

            string sql = FormatSql(
                "select count(*) from {0:NAME} where {1:NAME} = '{2}' and {3:NAME} = '{4}'",
                indexesTableName, "TABLE_NAME", tableName.Name, "INDEX_NAME", indexName);

            if (!tableName.SchemaIsEmpty)
            {
                sql += FormatSql(" and {0:NAME} = '{1}' and {2:NAME} = '{1}'", "TABLE_OWNER", tableName.Schema, "OWNER");
            }

            int count = Convert.ToInt32(ExecuteScalar(sql));
            return count > 0;
        }

        public override SchemaQualifiedObjectName[] GetTables(string schema = null)
        {
            string schemaName = string.IsNullOrWhiteSpace(schema) ? "user" : $"'{schema}'";

            string sql = FormatSql(
                "SELECT {0:NAME}, {1:NAME} from {2:NAME} where {3:NAME} = {4}",
                "TABLE_NAME", "OWNER", "ALL_TABLES", "OWNER", schemaName);

            var tables = new List<SchemaQualifiedObjectName>();

            using (IDataReader reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    string tableSchema = reader.GetString(1);
                    tables.Add(tableName.WithSchema(tableSchema));
                }
            }

            return tables.ToArray();
        }

        public override bool TableExists(SchemaQualifiedObjectName table)
        {
            string schemaName = table.SchemaIsEmpty ? "user" : $"'{table.Schema}'";

            string sql = FormatSql("SELECT COUNT(*) from {0:NAME} where {1:NAME} = '{2}' and {3:NAME} = {4}",
                "ALL_TABLES", "TABLE_NAME", table.Name, "OWNER", schemaName);

            object count = ExecuteScalar(sql);
            return Convert.ToInt32(count) > 0;
        }

        #endregion
    }
}
