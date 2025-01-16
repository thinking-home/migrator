using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Providers.SqlServer
{
    /// <summary>
    /// Migration transformations provider for Microsoft SQL Server.
    /// </summary>
    public class SqlServerTransformationProvider : TransformationProvider<SqlConnection>
    {
        public SqlServerTransformationProvider(SqlConnection connection, ILogger logger)
            : base(connection, logger)
        {
            typeMap.Put(DbType.AnsiStringFixedLength, "CHAR(255)");
            typeMap.Put(DbType.AnsiStringFixedLength, 8000, "CHAR($l)");
            typeMap.Put(DbType.AnsiString, "VARCHAR(255)");
            typeMap.Put(DbType.AnsiString, 8000, "VARCHAR($l)");
            typeMap.Put(DbType.AnsiString, int.MaxValue, "VARCHAR(MAX)");
            typeMap.Put(DbType.Binary, "VARBINARY(8000)");
            typeMap.Put(DbType.Binary, 8000, "VARBINARY($l)");
            typeMap.Put(DbType.Binary, int.MaxValue, "VARBINARY(MAX)");
            typeMap.Put(DbType.Boolean, "BIT");
            typeMap.Put(DbType.Byte, "TINYINT");
            typeMap.Put(DbType.Currency, "MONEY");
            typeMap.Put(DbType.Date, "DATE");
            typeMap.Put(DbType.DateTime, "DATETIME");
            typeMap.Put(DbType.DateTime2, "DATETIME2");
            typeMap.Put(DbType.DateTime2, 6, "DATETIME2($l)");
            typeMap.Put(DbType.Decimal, "DECIMAL");
            typeMap.Put(DbType.Decimal, 38, "DECIMAL($l, $s)", 2);
            typeMap.Put(DbType.Double, "DOUBLE PRECISION"); //synonym for FLOAT(53)
            typeMap.Put(DbType.Guid, "UNIQUEIDENTIFIER");
            typeMap.Put(DbType.Int16, "SMALLINT");
            typeMap.Put(DbType.Int32, "INT");
            typeMap.Put(DbType.Int64, "BIGINT");
            typeMap.Put(DbType.Single, "REAL"); //synonym for FLOAT(24)
            typeMap.Put(DbType.StringFixedLength, "NCHAR(255)");
            typeMap.Put(DbType.StringFixedLength, 4000, "NCHAR($l)");
            typeMap.Put(DbType.String, "NVARCHAR(255)");
            typeMap.Put(DbType.String, 4000, "NVARCHAR($l)");
            typeMap.Put(DbType.String, int.MaxValue, "NVARCHAR(MAX)");
            typeMap.Put(DbType.Time, "DATETIME");
            typeMap.Put(DbType.Xml, "XML");

            propertyMap.RegisterPropertySql(ColumnProperty.Identity, "IDENTITY");
        }

        #region change default value

        protected override string GetSqlChangeDefaultValue(
            SchemaQualifiedObjectName table, string column, object newDefaultValue)
        {
            string dfConstraintName = $"DF_{Guid.NewGuid():N}";
            string sqlDefaultValue = GetSqlDefaultValue(newDefaultValue);
            return FormatSql("ALTER TABLE {0:NAME} ADD CONSTRAINT {1:NAME} {2} FOR {3:NAME}",
                table, dfConstraintName, sqlDefaultValue, column);
        }

        protected string GetDefaultConstraintName(SchemaQualifiedObjectName table, string column)
        {
            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append(FormatSql("SELECT {0:NAME}.{1:NAME} AS {2:NAME} ", "dobj", "name", "CONSTRAINT_NAME"));
            sqlBuilder.Append(FormatSql("FROM {0:NAME} {1:NAME} ", "columns".WithSchema("sys"), "col"));
            sqlBuilder.Append(FormatSql("INNER JOIN {0:NAME} {1:NAME} ", "objects".WithSchema("sys"), "dobj"));
            sqlBuilder.Append(FormatSql("ON {0:NAME}.{1:NAME} = {2:NAME}.{3:NAME} AND {0:NAME}.{4:NAME} = 'D' ",
                "dobj", "object_id", "col", "default_object_id", "type"));
            sqlBuilder.Append(FormatSql("WHERE {0:NAME}.{1:NAME} = object_id(N'{2:NAME}') AND {0:NAME}.{3:NAME} = '{4}'",
                "col", "object_id", table, "name", column));

            using var reader = ExecuteReader(sqlBuilder.ToString());
            
            if (reader.Read())
            {
                return reader.GetString(0);
            }

            return null;
        }

        public override void ChangeDefaultValue(SchemaQualifiedObjectName table, string column, object newDefaultValue)
        {
            string defaultConstraintName = GetDefaultConstraintName(table, column);

            if (!string.IsNullOrWhiteSpace(defaultConstraintName))
            {
                RemoveConstraint(table, defaultConstraintName);
            }

            if (newDefaultValue != null)
            {
                string sql = GetSqlChangeDefaultValue(table, column, newDefaultValue);
                ExecuteNonQuery(sql);
            }
        }

        #endregion

        public override SchemaQualifiedObjectName[] GetTables(string schema = null)
        {
            string nspname = string.IsNullOrWhiteSpace(schema) ? "SCHEMA_NAME()" : $"'{schema}'";

            var tables = new List<SchemaQualifiedObjectName>();

            string sql = FormatSql("SELECT {0:NAME}, {1:NAME} FROM {2:NAME}.{3:NAME} where {4:NAME} = {5}",
                "TABLE_NAME", "TABLE_SCHEMA", "INFORMATION_SCHEMA", "TABLES", "TABLE_SCHEMA", nspname);

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
            string nspname = table.SchemaIsEmpty ? "SCHEMA_NAME()" : $"'{table.Schema}'";

            string sql = FormatSql(
                "SELECT * FROM {0:NAME} WHERE {1:NAME}='{2}' AND {3:NAME} = {4}",
                "TABLES".WithSchema("INFORMATION_SCHEMA"), "TABLE_NAME", table.Name, "TABLE_SCHEMA", nspname);

            using (IDataReader reader = ExecuteReader(sql))
            {
                return reader.Read();
            }
        }

        #region generate sql

        protected override string GetSqlDefaultValue(object defaultValue)
        {
            if (defaultValue is bool)
            {
                defaultValue = ((bool) defaultValue) ? 1 : 0;
            }

            return $"DEFAULT {defaultValue}";
        }

        #endregion

        #region Features of DBMS

        protected override string NamesQuoteTemplate => "[{0}]";

        public override string BatchSeparator => "GO";

        #endregion

        #region generate sql

        protected override string GetSqlAddColumn(SchemaQualifiedObjectName table, string columnSql)
        {
            return FormatSql("ALTER TABLE {0:NAME} ADD {1}", table, columnSql);
        }

        protected override string GetSqlRenameColumn(
            SchemaQualifiedObjectName tableName, string oldColumnName, string newColumnName)
        {
            return FormatSql("EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'", tableName, oldColumnName, newColumnName);
        }

        protected override string GetSqlRenameTable(SchemaQualifiedObjectName oldName, string newName)
        {
            return FormatSql("EXEC sp_rename '{0}', '{1}'", oldName, newName);
        }

        #endregion

        #region DDL

        public override bool IndexExists(string indexName, SchemaQualifiedObjectName tableName)
        {
            string sql = FormatSql(
                "SELECT COUNT(*) FROM {0:NAME} WHERE {1:NAME} = '{2}' AND {3:NAME} = object_id(N'{4:NAME}')",
                "indexes".WithSchema("sys"), "name", indexName, "object_id", tableName);
            int count = Convert.ToInt32(ExecuteScalar(sql));
            return count > 0;
        }

        public override bool ConstraintExists(SchemaQualifiedObjectName table, string name)
        {
            SchemaQualifiedObjectName fullConstraintName = name.WithSchema(table.Schema);

            string sql = FormatSql(
                "SELECT TOP 1 {0:NAME} FROM {1:NAME} " +
                "WHERE {2:NAME} = object_id('{3:NAME}') " +
                "AND {4:NAME} = object_id('{5:NAME}') " +
                "AND {6:NAME} IN ('D', 'F', 'PK', 'UQ')" +
                "UNION ALL " +
                "SELECT TOP 1 {0:NAME} FROM {7:NAME} " +
                "WHERE {2:NAME} = object_id(N'{3:NAME}') AND " +
                "{4:NAME} = object_id(N'{5:NAME}')",
                "name", "objects".WithSchema("sys"), "parent_object_id", table,
                "object_id", fullConstraintName, "type", "check_constraints".WithSchema("sys"));

            using (IDataReader reader = ExecuteReader(sql))
            {
                return reader.Read();
            }
        }

        public override bool ColumnExists(SchemaQualifiedObjectName table, string column)
        {
            string sql = FormatSql(
                "SELECT * FROM {0:NAME} " +
                "WHERE {1:NAME}='{2}' AND {3:NAME}='{4}'",
                "COLUMNS".WithSchema("INFORMATION_SCHEMA"), "TABLE_NAME", table.Name,
                "COLUMN_NAME", column);

            if (!table.SchemaIsEmpty)
            {
                sql += FormatSql(" AND {0:NAME} = '{1}'", "TABLE_SCHEMA", table.Schema);
            }

            using (IDataReader reader = ExecuteReader(sql))
            {
                return reader.Read();
            }
        }

        public override void RemoveColumn(SchemaQualifiedObjectName table, string column)
        {
            DeleteColumnConstraints(table, column);
            base.RemoveColumn(table, column);
        }

        // Deletes all constraints linked to a column.
        // Sql Server doesn't seems to do this.
        private void DeleteColumnConstraints(SchemaQualifiedObjectName table, string column)
        {
            string sqlContraints = FindConstraints(table, column);
            var constraints = new List<string>();
            using (IDataReader reader = ExecuteReader(sqlContraints))
            {
                while (reader.Read())
                {
                    constraints.Add(reader.GetString(0));
                }
            }

            // Can't share the connection so two phase modif
            foreach (string constraint in constraints)
            {
                RemoveConstraint(table, constraint);
            }
        }

        protected string FindConstraints(SchemaQualifiedObjectName table, string column)
        {
            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append(FormatSql("SELECT {0:NAME} ", "CONSTRAINT_NAME"));
            sqlBuilder.Append(FormatSql("FROM {0:NAME} ", "CONSTRAINT_COLUMN_USAGE".WithSchema("INFORMATION_SCHEMA")));
            sqlBuilder.Append(FormatSql("WHERE {0:NAME} = '{1}' and {2:NAME} = '{3}' ",
                "TABLE_NAME", table.Name, "COLUMN_NAME", column));

            if (!table.SchemaIsEmpty)
            {
                sqlBuilder.Append(FormatSql("AND {0:NAME} = '{1}' ", "TABLE_SCHEMA", table.Schema));
            }

            sqlBuilder.Append("UNION ALL ");
            sqlBuilder.Append(FormatSql("SELECT {0:NAME}.{1:NAME} as {2:NAME} ", "dobj", "name", "CONSTRAINT_NAME"));
            sqlBuilder.Append(FormatSql("FROM {0:NAME} {1:NAME} ", "columns".WithSchema("sys"), "col"));
            sqlBuilder.Append(FormatSql("INNER JOIN {0:NAME} {1:NAME} ", "objects".WithSchema("sys"), "dobj"));
            sqlBuilder.Append(FormatSql("ON {0:NAME}.{1:NAME} = {2:NAME}.{3:NAME} AND {0:NAME}.{4:NAME} = 'D' ",
                "dobj", "object_id", "col", "default_object_id", "type"));

            sqlBuilder.Append(FormatSql(
                "WHERE {0:NAME}.{1:NAME} = object_id(N'{2:NAME}') AND {0:NAME}.{3:NAME} = '{4}'",
                "col", "object_id", table, "name", column));

            return sqlBuilder.ToString();
        }

        #endregion
    }
}
