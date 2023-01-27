using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Providers.MySql
{
    using ForeignKeyConstraint = Framework.ForeignKeyConstraint;

    public class MySqlTransformationProvider : TransformationProvider<MySqlConnection>
    {
        public MySqlTransformationProvider(MySqlConnection connection, ILogger logger)
            : base(connection, logger)
        {
            typeMap.Put(DbType.AnsiStringFixedLength, "CHAR(255)");
            typeMap.Put(DbType.AnsiStringFixedLength, 255, "CHAR($l)");
            typeMap.Put(DbType.AnsiStringFixedLength, 65535, "TEXT");
            typeMap.Put(DbType.AnsiStringFixedLength, 16777215, "MEDIUMTEXT");
            typeMap.Put(DbType.AnsiString, "VARCHAR(255)");
            typeMap.Put(DbType.AnsiString, 255, "VARCHAR($l)");
            typeMap.Put(DbType.AnsiString, 65535, "TEXT");
            typeMap.Put(DbType.AnsiString, 16777215, "MEDIUMTEXT");
            typeMap.Put(DbType.Binary, "LONGBLOB");
            typeMap.Put(DbType.Binary, 127, "TINYBLOB");
            typeMap.Put(DbType.Binary, 65535, "BLOB");
            typeMap.Put(DbType.Binary, 16777215, "MEDIUMBLOB");
            typeMap.Put(DbType.Boolean, "TINYINT(1)");
            typeMap.Put(DbType.Byte, "TINYINT UNSIGNED");
            typeMap.Put(DbType.Currency, "MONEY");
            typeMap.Put(DbType.Date, "DATE");
            typeMap.Put(DbType.DateTime, "DATETIME");
            typeMap.Put(DbType.Decimal, "NUMERIC");
            typeMap.Put(DbType.Decimal, 38, "NUMERIC($l, $s)", 2);
            typeMap.Put(DbType.Double, "DOUBLE");
            typeMap.Put(DbType.Guid, "VARCHAR(40)");
            typeMap.Put(DbType.Int16, "SMALLINT");
            typeMap.Put(DbType.Int32, "INTEGER");
            typeMap.Put(DbType.Int64, "BIGINT");
            typeMap.Put(DbType.Single, "FLOAT");
            typeMap.Put(DbType.StringFixedLength, "CHAR(255)");
            typeMap.Put(DbType.StringFixedLength, 255, "CHAR($l)");
            typeMap.Put(DbType.StringFixedLength, 65535, "TEXT");
            typeMap.Put(DbType.StringFixedLength, 16777215, "MEDIUMTEXT");
            typeMap.Put(DbType.String, "VARCHAR(255)");
            typeMap.Put(DbType.String, 255, "VARCHAR($l)");
            typeMap.Put(DbType.String, 65535, "TEXT");
            typeMap.Put(DbType.String, 16777215, "MEDIUMTEXT");
            typeMap.Put(DbType.Time, "TIME");

            propertyMap.RegisterPropertySql(ColumnProperty.Unsigned, "UNSIGNED");
            propertyMap.RegisterPropertySql(ColumnProperty.Identity, "AUTO_INCREMENT");
        }

        #region custom sql

        protected override bool IdentityNeedsType => false;

        protected override string NamesQuoteTemplate => "`{0}`";

        #endregion

        #region custom sql

        protected override string GetSqlChangeColumnType(SchemaQualifiedObjectName table, string column,
            ColumnType columnType)
        {
            string columnTypeSql = typeMap.Get(columnType);

            return FormatSql("ALTER TABLE {0:NAME} MODIFY {1:NAME} {2}", table, column, columnTypeSql);
        }

        protected override string GetSqlDefaultValue(object defaultValue)
        {
            if (defaultValue is bool)
            {
                defaultValue = ((bool) defaultValue) ? 1 : 0;
            }

            return $"DEFAULT {defaultValue}";
        }

        protected override string GetSqlRemoveConstraint(SchemaQualifiedObjectName table, string name)
        {
            var constraintTypeSql = GetConstraintTypeSql(table, name);
            var constraintType = ExecuteScalar(constraintTypeSql).ToString();

            string constraintSql = constraintType?.ToUpper() switch
            {
                "PRIMARY KEY" => "PRIMARY KEY",
                "FOREIGN KEY" => FormatSql("FOREIGN KEY {0:NAME}", name),
                _ => FormatSql("KEY {0:NAME}", name)
            }; 

            return FormatSql("ALTER TABLE {0:NAME} DROP {1}", table, constraintSql);
        }

        /// <summary>
        /// MySql can migrate the table to another schema when renaming.
        /// Therefore, you must explicitly add the schema of the source table to the new table name.
        /// </summary>
        protected override string GetSqlRenameTable(SchemaQualifiedObjectName oldName, string newName)
        {
            return FormatSql("ALTER TABLE {0:NAME} RENAME TO {1:NAME}", oldName, newName.WithSchema(oldName.Schema));
        }

        #endregion

        #region DDL

        public override bool IndexExists(string indexName, SchemaQualifiedObjectName tableName)
        {
            string sql = FormatSql("SHOW INDEXES FROM {0:NAME}", tableName);

            using (IDataReader reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    if (reader["Key_name"].ToString() == indexName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool ConstraintExists(SchemaQualifiedObjectName table, string name)
        {
            string sqlConstraint = FormatSql("SHOW KEYS FROM {0:NAME}", table);

            using (IDataReader reader = ExecuteReader(sqlConstraint))
            {
                while (reader.Read())
                {
                    if (reader["Key_name"].ToString() == name)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override SchemaQualifiedObjectName[] GetTables(string schema = null)
        {
            string schemaSql = string.IsNullOrWhiteSpace(schema) ? "SCHEMA()" : $"'{schema}'";

            string sql = FormatSql(
                "SELECT {0:NAME}, {1:NAME} FROM {2:NAME}.{3:NAME} WHERE {1:NAME} = {4}",
                "TABLE_NAME", "TABLE_SCHEMA", "information_schema", "TABLES", schemaSql);

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
            string sql = table.SchemaIsEmpty
                ? FormatSql("SHOW TABLES LIKE '{0}'", table)
                : FormatSql("SHOW TABLES IN {0:NAME} LIKE '{1}'", table.Schema, table.Name);

            using (IDataReader reader = ExecuteReader(sql))
            {
                return reader.Read();
            }
        }

        public override bool ColumnExists(SchemaQualifiedObjectName table, string column)
        {
            string sql = FormatSql("SHOW COLUMNS FROM {0:NAME} WHERE Field='{1}'", table, column);

            using (IDataReader reader = ExecuteReader(sql))
            {
                return reader.Read();
            }
        }

        public override void RenameColumn(SchemaQualifiedObjectName tableName, string oldColumnName,
            string newColumnName)
        {
            throw new NotSupportedException("MySql doesn't support column rename");
        }

        public override void AddCheckConstraint(string name, SchemaQualifiedObjectName table, string checkSql)
        {
            throw new NotSupportedException("MySql doesn't support check constraints");
        }

        public override void AddForeignKey(string name, SchemaQualifiedObjectName primaryTable, string[] primaryColumns,
            SchemaQualifiedObjectName refTable, string[] refColumns,
            ForeignKeyConstraint onDeleteConstraint = ForeignKeyConstraint.NoAction,
            ForeignKeyConstraint onUpdateConstraint = ForeignKeyConstraint.NoAction)
        {
            if (onDeleteConstraint == ForeignKeyConstraint.SetDefault ||
                onUpdateConstraint == ForeignKeyConstraint.SetDefault)
            {
                throw new NotSupportedException("MySQL foreign keys doesn't support SET DEFAULT action");
            }

            base.AddForeignKey(name, primaryTable, primaryColumns, refTable, refColumns, onDeleteConstraint,
                onUpdateConstraint);
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
            string sqlContraints = GetColConstraintsSql(table, column);
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
        
        protected string GetColConstraintsSql(SchemaQualifiedObjectName table, string column)
        {
            var sqlBuilder = new StringBuilder();
            
            sqlBuilder.Append(FormatSql("SELECT {0:NAME} ", "CONSTRAINT_NAME"));
            sqlBuilder.Append(FormatSql("FROM {0:NAME} ", "KEY_COLUMN_USAGE".WithSchema("INFORMATION_SCHEMA")));
            sqlBuilder.Append(FormatSql("WHERE {0:NAME} = '{1}' AND {2:NAME} = '{3}' ",
                "TABLE_NAME", table.Name, "COLUMN_NAME", column));
            
            if (!table.SchemaIsEmpty)
            {
                sqlBuilder.Append(FormatSql("AND {0:NAME} = '{1}' ", "TABLE_SCHEMA", table.Schema));
            }

            return sqlBuilder.ToString();
        }
        
        protected string GetConstraintTypeSql(SchemaQualifiedObjectName table, string name)
        {
            var sqlBuilder = new StringBuilder();
            
            sqlBuilder.Append(FormatSql("SELECT {0:NAME} ", "CONSTRAINT_TYPE"));
            sqlBuilder.Append(FormatSql("FROM {0:NAME} ", "TABLE_CONSTRAINTS".WithSchema("INFORMATION_SCHEMA")));
            sqlBuilder.Append(FormatSql("WHERE {0:NAME} = '{1}' AND {2:NAME} = '{3}' ",
                "TABLE_NAME", table.Name, "CONSTRAINT_NAME", name));
            
            if (!table.SchemaIsEmpty)
            {
                sqlBuilder.Append(FormatSql("AND {0:NAME} = '{1}' ", "TABLE_SCHEMA", table.Schema));
            }

            return sqlBuilder.ToString();
        }

        #endregion
    }
}
