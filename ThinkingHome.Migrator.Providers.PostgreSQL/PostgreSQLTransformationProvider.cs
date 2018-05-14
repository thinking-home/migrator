using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using Npgsql;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Providers.PostgreSQL
{
    public class PostgreSQLTransformationProvider : TransformationProvider<NpgsqlConnection>
    {
        public PostgreSQLTransformationProvider(NpgsqlConnection connection, ILogger logger) : base(connection, logger)
        {
            typeMap.Put(DbType.AnsiStringFixedLength, "char(255)");
            typeMap.Put(DbType.AnsiStringFixedLength, 8000, "char($l)");
            typeMap.Put(DbType.AnsiString, "varchar(255)");
            typeMap.Put(DbType.AnsiString, 8000, "varchar($l)");
            typeMap.Put(DbType.AnsiString, int.MaxValue, "text");
            typeMap.Put(DbType.Binary, "bytea");
            typeMap.Put(DbType.Binary, 2147483647, "bytea");
            typeMap.Put(DbType.Boolean, "boolean");
            typeMap.Put(DbType.Byte, "int2");
            typeMap.Put(DbType.Currency, "decimal(16,4)");
            typeMap.Put(DbType.Date, "date");
            typeMap.Put(DbType.DateTime, "timestamp");
            typeMap.Put(DbType.Decimal, "decimal(18,5)");
            typeMap.Put(DbType.Decimal, 18, "decimal($l, $s)");
            typeMap.Put(DbType.Double, "float8");
            typeMap.Put(DbType.Guid, "uuid");
            typeMap.Put(DbType.Int16, "int2");
            typeMap.Put(DbType.Int32, "int4");
            typeMap.Put(DbType.Int64, "int8");
            typeMap.Put(DbType.Single, "float4");
            typeMap.Put(DbType.StringFixedLength, "char(255)");
            typeMap.Put(DbType.StringFixedLength, 4000, "char($l)");
            typeMap.Put(DbType.String, "varchar(255)");
            typeMap.Put(DbType.String, 4000, "varchar($l)");
            typeMap.Put(DbType.String, int.MaxValue, "text");
            typeMap.Put(DbType.Time, "time");

            propertyMap.RegisterPropertySql(ColumnProperty.Identity, "serial");

        }

        public override bool IdentityNeedsType => false;

        public override string BatchSeparator => null;

        #region custom sql

        protected override string GetSqlRemoveTable(SchemaQualifiedObjectName table)
        {
            return FormatSql("DROP TABLE {0:NAME} CASCADE", table);
        }

        protected override string GetSqlRemoveIndex(string indexName, SchemaQualifiedObjectName tableName)
        {
            SchemaQualifiedObjectName ixName = indexName.WithSchema(tableName.Schema);
            return FormatSql("DROP INDEX {0:NAME}", ixName);
        }

        protected override string GetSqlChangeColumnType(SchemaQualifiedObjectName table, string column, ColumnType columnType)
        {
            string columnTypeSql = typeMap.Get(columnType);

            return FormatSql("ALTER TABLE {0:NAME} ALTER COLUMN {1:NAME} TYPE {2}", table, column, columnTypeSql);
        }

        protected override string GetSqlChangeNotNullConstraint(SchemaQualifiedObjectName table, string column, bool notNull, ref string sqlChangeColumnType)
        {
            // ���� ��������� ���� ������� � �������� NOT NULL ���������� ����� ��������,
            // �� �������� �������� sqlChangeColumnType � ���������� NULL
            // ����� ���������� ������, �������� ������� NOT NULL

            string sqlNotNull = notNull ? "SET NOT NULL" : "DROP NOT NULL";

            return FormatSql("ALTER TABLE {0:NAME} ALTER COLUMN {1:NAME} {2}", table, column, sqlNotNull);
        }

        #endregion

        public override bool ColumnExists(SchemaQualifiedObjectName table, string column)
        {
            string nspname = table.SchemaIsEmpty ? "current_schema()" : $"'{table.Schema}'";

            string sql = FormatSql(
                "SELECT {0:NAME} FROM {1:NAME}.{2:NAME} WHERE {3:NAME} = {4} AND {5:NAME} = '{6}' AND {7:NAME} = '{8}'",
                "column_name", "information_schema", "columns", "table_schema",
                nspname, "table_name", table.Name, "column_name", column);

            using (IDataReader reader = ExecuteReader(sql))
            {
                return reader.Read();
            }
        }

        public override bool ConstraintExists(SchemaQualifiedObjectName table, string name)
        {
            string nspname = table.SchemaIsEmpty ? "current_schema()" : $"'{table.Schema}'";

            string sql = FormatSql(
                "SELECT {0:NAME} FROM {1:NAME}.{2:NAME} WHERE {3:NAME} = {4} AND {5:NAME} = '{6}' AND {7:NAME} = '{8}'",
                "constraint_name", "information_schema", "table_constraints", "table_schema",
                nspname, "constraint_name", name, "table_name", table.Name);

            using (IDataReader reader = ExecuteReader(sql))
            {
                return reader.Read();
            }
        }

        public override bool IndexExists(string indexName, SchemaQualifiedObjectName tableName)
        {
            string nspname = tableName.SchemaIsEmpty ? "current_schema()" : string.Format("'{0}'", tableName.Schema);

            var builder = new StringBuilder();

            builder.Append("SELECT count(*) FROM pg_class c ");
            builder.Append("JOIN pg_index i ON i.indexrelid = c.oid ");
            builder.Append("JOIN pg_class c2 ON i.indrelid = c2.oid ");
            builder.Append("LEFT JOIN pg_user u ON u.usesysid = c.relowner ");
            builder.Append("LEFT JOIN pg_namespace n ON n.oid = c.relnamespace ");
            builder.Append("WHERE c.relkind = 'i' ");
            builder.AppendFormat("AND n.nspname = {0} ", nspname);
            builder.AppendFormat("AND c2.relname = '{0}' ", tableName.Name);
            builder.AppendFormat("AND c.relname = '{0}' ", indexName);

            int count = Convert.ToInt32(ExecuteScalar(builder.ToString()));
            return count > 0;
        }

        public override SchemaQualifiedObjectName[] GetTables(string schema = null)
        {
            string nspname = string.IsNullOrWhiteSpace(schema) ? "current_schema()" : string.Format("'{0}'", schema);

            string sql = FormatSql(
                "SELECT {0:NAME}, {1:NAME} FROM {2:NAME}.{3:NAME} WHERE {4:NAME} = {5}",
                "table_name", "table_schema", "information_schema", "tables", "table_schema", nspname);

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
            string nspname = table.SchemaIsEmpty ? "current_schema()" : $"'{table.Schema}'";

            string sql = FormatSql(
                "SELECT {0:NAME} FROM {1:NAME}.{2:NAME} WHERE {3:NAME} = {4} AND {5:NAME} = '{6}'",
                "table_name", "information_schema", "tables", "table_schema", nspname, "table_name", table.Name);

            using (IDataReader reader = ExecuteReader(sql))
            {
                return reader.Read();
            }
        }
    }
}
