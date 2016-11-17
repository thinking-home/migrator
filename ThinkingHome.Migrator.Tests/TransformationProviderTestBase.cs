using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ThinkingHome.Migrator.Exceptions;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Tests
{
    public abstract class TransformationProviderTestBase
    {
        public class NameComparer : IEqualityComparer<string>
        {
            /// <summary>
            /// "Нормализация" названия схемы (чтобы при сравнении считать разные варианты пустой схемы равными)
            /// </summary>
            private static string GetNotNullSchemaName(string name)
            {
                return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
            }

            private readonly StringComparer comparer;

            public NameComparer(bool ignoreCase = false)
            {
                comparer = ignoreCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture;
            }

            #region Implementation of IEqualityComparer<in SchemaQualifiedObjectName>

            public bool Equals(string x, string y)
            {
                string schema1 = GetNotNullSchemaName(x);
                string schema2 = GetNotNullSchemaName(y);

                return comparer.Equals(schema1, schema2);
            }

            public int GetHashCode(string obj)
            {
                string name = GetNotNullSchemaName(obj);

                return name?.GetHashCode() ?? 0;
            }

            #endregion
        }

        #region common

        protected ITransformationProvider provider;

        protected static bool isInitialized;

        public abstract ITransformationProvider CreateProvider();

        [SetUp]
        public virtual void SetUp()
        {
            if (!isInitialized)
            {
                isInitialized = true;
            }

            provider = CreateProvider();
        }

        [TearDown]
        public virtual void TearDown()
        {
            provider.Dispose();
        }

        protected virtual string ResourceSql => "ThinkingHome.Migrator.Tests.TestMigrations.test.sql";

        protected abstract string BatchSql { get; }

        #endregion

        #region tests

        #region common

        [Test]
        public void CanExecuteBatches()
        {
            provider.AddTable("BatchSqlTest",
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestId", DbType.Int32));

            provider.ExecuteNonQuery(BatchSql);

            string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME} ORDER BY {2:NAME}", "TestId", "BatchSqlTest", "Id");

            using (var reader = provider.ExecuteReader(sql))
            {
                for (int i = 1; i <= 3; i++)
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual(111 * i, Convert.ToInt32(reader[0]));
                }
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable("BatchSqlTest");
        }

        [Test]
        public void CanExecuteScriptFromResources()
        {
            provider.AddTable("TestTwo",
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestId", DbType.Int32));

            Assembly asm = GetType().GetTypeInfo().Assembly;
            provider.ExecuteFromResource(asm, ResourceSql);

            string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME} WHERE {2:NAME} = {3}",
                "TestId", "TestTwo", "Id", 5555);

            Assert.AreEqual(9999, provider.ExecuteScalar(sql));

            provider.RemoveTable("TestTwo");
        }

        #endregion

        #region table

        [Test]
        public virtual void CanAddAndDropTable()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("MooTable");

            Assert.IsFalse(provider.TableExists(tableName));

            provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));

            Assert.IsTrue(provider.TableExists(tableName));

            provider.RemoveTable(tableName);

            Assert.IsFalse(provider.TableExists(tableName));
        }

        [Test]
        public virtual void CanCreateTableWithNecessaryCols()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("Mimimi");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("StringColumn", DbType.String.WithSize(500)),
                new Column("IntegerColumn", DbType.Int32)
            );

            provider.Insert(
                tableName,
                new[] { "ID", "StringColumn", "IntegerColumn" },
                new[] { "1984", "test moo", "123" }
            );


            string sql = provider.FormatSql("select * from {0:NAME}", tableName);
            using (var reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(1984, reader["ID"]);
                Assert.AreEqual("test moo", reader["StringColumn"]);
                Assert.AreEqual(123, Convert.ToDecimal(reader["IntegerColumn"]));
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanAddTableWithCompoundPrimaryKey()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("TableWCPK");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("ID2", DbType.Int32, ColumnProperty.PrimaryKey)
            );

            provider.Insert(tableName, new[] { "ID", "ID2" }, new[] { "5", "6" });
            provider.Insert(tableName, new[] { "ID", "ID2" }, new[] { "5", "7" });
            provider.Insert(tableName, new[] { "ID", "ID2" }, new[] { "7", "6" });

            Assert.Throws<SQLException>(() =>
                    provider.Insert(tableName, new[] { "ID", "ID2" }, new[] { "5", "6" }));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void TableWithCompoundPrimaryKeyShouldKeepNullForOtherProperties()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("Test");

            provider.AddTable(tableName,
                new Column("PersonId", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("AddressId", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(30), ColumnProperty.Null));

            provider.Insert(tableName,
                new[] { "PersonId", "AddressId", "Name" },
                new[] { "1", "2", null });

            string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "Name", tableName);

            using (var reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(DBNull.Value, reader[0]);
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanRenameTable()
        {
            SchemaQualifiedObjectName table1 = GetRandomTableName("tableMoo");
            SchemaQualifiedObjectName table2 = GetRandomTableName("tableHru");

            Assert.IsFalse(provider.TableExists(table1));
            Assert.IsFalse(provider.TableExists(table2));

            provider.AddTable(table1, new Column("ID", DbType.Int32));
            provider.RenameTable(table1, table2.Name);

            Assert.IsTrue(provider.TableExists(table2));

            provider.RemoveTable(table2);
        }

        [Test]
        public virtual void CantRemoveUnexistingTable()
        {
            var randomTableName = GetRandomTableName();
            Assert.Throws<SQLException>(() => provider.RemoveTable(randomTableName));
        }

        [Test]
        public virtual void CanGetTables()
        {
            string schema = GetSchemaForCreateTables();
            string schemaForCompare = GetSchemaForCompare();

            SchemaQualifiedObjectName table1 = GetRandomTableName("tableMoo");
            SchemaQualifiedObjectName table2 = GetRandomTableName("tableHru");

            var tables = provider.GetTables(schema);

            Assert.That(tables.All(t => StrComparer.Equals(t.Schema, schemaForCompare)));
            Assert.IsFalse(tables.Select(t => t.Name).Contains(table1.Name, StrComparer));
            Assert.IsFalse(tables.Select(t => t.Name).Contains(table2.Name, StrComparer));

            provider.AddTable(table1, new Column("ID", DbType.Int32));
            provider.AddTable(table2, new Column("ID", DbType.Int32));

            var tables2 = provider.GetTables(schema);

            Assert.AreEqual(tables.Length + 2, tables2.Length);
            Assert.That(tables2.All(t => StrComparer.Equals(t.Schema, schemaForCompare)));
            Assert.IsTrue(tables2.Select(t => t.Name).Contains(table1.Name, StrComparer));
            Assert.IsTrue(tables2.Select(t => t.Name).Contains(table2.Name, StrComparer));

            provider.RemoveTable(table1);
            provider.RemoveTable(table2);
        }

        #endregion

        #region columns

        [Test]
        public virtual void CanAddColumn()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddColumnTest");

            provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));

            provider.AddColumn(tableName, new Column("TestStringColumn", DbType.String.WithSize(7)));

            provider.Insert(tableName, new[] { "ID", "TestStringColumn" }, new[] { "2", "test" });
            provider.Insert(tableName, new[] { "ID", "TestStringColumn" }, new[] { "4", "testmoo" });

            string sql = provider.FormatSql("select * from {0:NAME}", tableName);
            using (var reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(2, reader[0]);
                Assert.AreEqual("test", reader[1]);

                Assert.IsTrue(reader.Read());
                Assert.AreEqual(4, reader[0]);
                Assert.AreEqual("testmoo", reader[1]);

                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanAddBooleanColumnWithDefault()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddBooleanColumnTest");

            provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));

            provider.AddColumn(tableName, new Column("Boolean1", DbType.Boolean, ColumnProperty.NotNull, true));
            provider.AddColumn(tableName, new Column("Boolean2", DbType.Boolean, ColumnProperty.NotNull, false));

            provider.Insert(tableName, new[] { "ID" }, new[] { "22" });

            string sql = provider.FormatSql("select * from {0:NAME}", tableName);
            using (var reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());

                Assert.AreEqual(22, reader["ID"]);
                Assert.AreEqual(true, Convert.ToBoolean(reader["Boolean1"]));
                Assert.AreEqual(false, Convert.ToBoolean(reader["Boolean2"]));

                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanSetNotNullRepeatedly()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("TestTable");
            string columnName1 = GetRandomName("TestNullableColumn");
            string columnName2 = GetRandomName("TestNotNullableColumn");

            provider.AddTable(tableName,
                new Column(columnName1, DbType.Int32, ColumnProperty.Null),
                new Column(columnName2, DbType.Int32, ColumnProperty.NotNull)
            );

            provider.ChangeColumn(tableName, columnName1, DbType.Int32, false);
            provider.ChangeColumn(tableName, columnName2, DbType.Int32, true);

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanChangeColumnType()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("ChangeColumnTypeTest");
            string columnName1 = GetRandomName("TestDecimalColumn1");
            string columnName2 = GetRandomName("TestDecimalColumn2");
            string selectSql = provider.FormatSql("select {0:NAME} from {1:NAME}", columnName2, tableName);

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column(columnName1, DbType.Decimal.WithSize(8, 4)),
                new Column(columnName2, DbType.Decimal.WithSize(8, 4)));

            provider.Insert(tableName, new[] { "ID", columnName1, columnName2 }, new[] { "1", "123.4568", "123.4568" });
            Assert.AreEqual(123.4568, provider.ExecuteScalar(selectSql));

            // делаем по извращенски с 2 колонками, т.к. у оракла ограничение: изменяемая колонка должна быть пустой
            provider.Update(tableName, new[] { columnName2 }, new string[] { null });
            provider.ChangeColumn(tableName, columnName2, DbType.Int32, false);
            string updateSql = provider.FormatSql("update {0:NAME} set {1:NAME} = {2:NAME}", tableName, columnName2, columnName1);
            provider.ExecuteNonQuery(updateSql);

            Assert.AreEqual(123, provider.ExecuteScalar(selectSql));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanChangeNotNullProperty()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("ChangeNotNullPropertyTest");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestStringColumn", DbType.String.WithSize(4)));

            // проверяем, что можно вставить null
            provider.Insert(tableName, new[] { "ID", "TestStringColumn" }, new[] { "1", null });
            provider.Delete(tableName);

            // ставим ограничение NOT NULL и проверяем, что вставка NULL генерирует исключение
            provider.ChangeColumn(tableName, "TestStringColumn", DbType.String.WithSize(4), true);
            Assert.Throws<SQLException>(() =>
                    provider.Insert(tableName, new[] { "ID", "TestStringColumn" }, new[] { "2", null }));

            // удаляем ограничение NOT NULL и проверяем, что можно вставить NULL
            provider.ChangeColumn(tableName, "TestStringColumn", DbType.String.WithSize(4), false);
            provider.Insert(tableName, new[] { "ID", "TestStringColumn" }, new[] { "3", null });


            provider.RemoveTable(tableName);
        }



        [Test]
        public virtual void CanChangeDefaultValueForColumn()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("ChangeDefault");

            provider.AddTable(
                tableName,
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestStringColumn", DbType.String.WithSize(40), ColumnProperty.NotNull));

            // нет значения по умолчанию
            Assert.Throws<SQLException>(() =>
                    provider.Insert(tableName, new[] { "Id" }, new[] { "1" }));

            // добавляем значнеие по умолчанию
            provider.ChangeDefaultValue(tableName, "TestStringColumn", "'moo-default'");
            provider.Insert(tableName, new[] { "Id" }, new[] { "2" });

            // изменяем значение по умолчанию
            provider.ChangeDefaultValue(tableName, "TestStringColumn", "'mi-default'");
            provider.Insert(tableName, new[] { "Id" }, new[] { "3" });

            // удаляем значение по умолчанию
            provider.ChangeDefaultValue(tableName, "TestStringColumn", null);
            Assert.Throws<SQLException>(() =>
                    provider.Insert(tableName, new[] { "Id" }, new[] { "4" }));

            string sql = provider.FormatSql(
                "select {0:NAME}, {1:NAME} from {2:NAME} order by {0:NAME}",
                "Id", "TestStringColumn", tableName);

            using (var reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(2, reader[0]);
                Assert.AreEqual("moo-default", reader[1]);

                Assert.IsTrue(reader.Read());
                Assert.AreEqual(3, reader[0]);
                Assert.AreEqual("mi-default", reader[1]);

                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanRenameColumn()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("RenameColumnTest");

            provider.AddTable(tableName, new Column("TestColumn1", DbType.Int32));
            provider.RenameColumn(tableName, "TestColumn1", "TestColumn2");

            Assert.IsFalse(provider.ColumnExists(tableName, "TestColumn1"));
            Assert.IsTrue(provider.ColumnExists(tableName, "TestColumn2"));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanRemoveColumn()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("RemoveColumnTest");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32),
                new Column("TestColumn1", DbType.Int32));

            provider.RemoveColumn(tableName, "TestColumn1");

            Assert.IsFalse(provider.ColumnExists(tableName, "TestColumn1"));

            provider.RemoveTable(tableName);
        }

        [Test]
        public void CanCheckThatColumnExists()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("RemoveColumnTest");

            provider.AddTable(tableName, new Column("MooMooMi", DbType.Int32));

            Assert.IsFalse(provider.ColumnExists(tableName, "asdfgfhgj"));
            Assert.IsTrue(provider.ColumnExists(tableName, "MooMooMi"));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CantRemoveUnexistingColumn()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("RemoveUnexistingColumn");
            string column = GetRandomName();

            provider.AddTable(tableName, new Column("ID", DbType.Int32));

            Assert.Throws<SQLException>(() =>
                    provider.RemoveColumn(tableName, column));

            provider.RemoveTable(tableName);
        }

        #endregion

        #region constraints

        #region primary key

        [Test]
        public virtual void CanAddPrimaryKey()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddPrimaryKey");
            string pkName = GetRandomName("PK_AddPrimaryKey");

            provider.AddTable(tableName,
                new Column("ID1", DbType.Int32, ColumnProperty.NotNull),
                new Column("ID2", DbType.Int32, ColumnProperty.NotNull));

            provider.AddPrimaryKey(pkName, tableName, "ID1", "ID2");

            provider.Insert(tableName, new[] { "ID1", "ID2" }, new[] { "1", "2" });
            provider.Insert(tableName, new[] { "ID1", "ID2" }, new[] { "2", "2" });

            Assert.Throws<SQLException>(() =>
                    provider.Insert(tableName, new[] { "ID1", "ID2" }, new[] { "1", "2" }));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanCheckThatPrimaryKeyIsExist()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("CheckThatPrimaryKeyIsExist");
            string pkName = GetRandomName("PK_CheckThatPrimaryKeyIsExist");

            provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.NotNull));
            Assert.IsFalse(provider.ConstraintExists(tableName, pkName));

            provider.AddPrimaryKey(pkName, tableName, "ID");
            Assert.IsTrue(provider.ConstraintExists(tableName, pkName));

            provider.RemoveConstraint(tableName, pkName);
            Assert.IsFalse(provider.ConstraintExists(tableName, pkName));

            provider.RemoveTable(tableName);
        }

        #endregion

        #region foreign key

        [Test]
        public virtual void CanAddForeignKey()
        {
            // создаем таблицы и добавляем внешний ключ
            SchemaQualifiedObjectName primaryTable = GetRandomTableName("AddForeignKey_Primary");
            SchemaQualifiedObjectName refTable = GetRandomTableName("AddForeignKey_Ref");
            string foreignKeyName = GetRandomName("FK_TestSimpleKey");

            provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(refTable, new[] { "ID" }, new[] { "17" });

            provider.AddTable(primaryTable,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("RefID", DbType.Int32));
            provider.AddForeignKey(foreignKeyName, primaryTable, "RefID", refTable, "ID");

            // пробуем нарушить ограничения внешнего ключа
            Assert.Throws<SQLException>(() =>
                    provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "111" }));
            provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "17" });
            Assert.Throws<SQLException>(() => provider.Delete(refTable));

            // удаляем таблицы
            provider.RemoveTable(primaryTable);
            provider.RemoveTable(refTable);
        }

        [Test]
        public virtual void CanAddComplexForeignKey()
        {
            // создаем таблицы и добавляем внешний ключ
            SchemaQualifiedObjectName primaryTable = GetRandomTableName("AddForeignKey_Primary");
            SchemaQualifiedObjectName refTable = GetRandomTableName("AddForeignKey_Ref");
            string foreignKeyName = GetRandomName("FK_TestComplexKeyabd");

            provider.AddTable(refTable,
                new Column("ID1", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("ID2", DbType.Int32, ColumnProperty.PrimaryKey));

            provider.Insert(refTable, new[] { "ID1", "ID2" }, new[] { "111", "222" });

            provider.AddTable(primaryTable,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("RefID1", DbType.Int32),
                new Column("RefID2", DbType.Int32));

            provider.AddForeignKey(foreignKeyName,
                primaryTable, new[] { "RefID1", "RefID2" },
                refTable, new[] { "ID1", "ID2" });

            // пробуем нарушить ограничения внешнего ключа
            Assert.Throws<SQLException>(() =>
                    provider.Insert(primaryTable, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "123", "456" }));

            Assert.Throws<SQLException>(() =>
                    provider.Insert(primaryTable, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "111", "456" }));

            Assert.Throws<SQLException>(() =>
                    provider.Insert(primaryTable, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "123", "222" }));

            provider.Insert(primaryTable, new[] { "ID", "RefID1", "RefID2" }, new[] { "1", "111", "222" });

            Assert.Throws<SQLException>(() => provider.Delete(refTable));

            // удаляем таблицы
            provider.RemoveTable(primaryTable);
            provider.RemoveTable(refTable);
        }

        [Test]
        public virtual void CanAddForeignKeyWithDeleteCascade()
        {
            SchemaQualifiedObjectName primaryTable = GetRandomTableName("AddForeignKey_Primary");
            SchemaQualifiedObjectName refTable = GetRandomTableName("AddForeignKey_Ref");
            string foreignKeyName = GetRandomName("FK_Test");

            provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(refTable, new[] { "ID" }, new[] { "177" });

            provider.AddTable(primaryTable,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("RefID", DbType.Int32));
            provider.AddForeignKey(foreignKeyName, primaryTable, "RefID", refTable, "ID", ForeignKeyConstraint.Cascade);

            provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "177" });
            provider.Delete(refTable);

            string sql = provider.FormatSql("select count(*) from {0:NAME}", primaryTable);
            Assert.AreEqual(0, provider.ExecuteScalar(sql));

            // удаляем таблицы
            provider.RemoveTable(primaryTable);
            provider.RemoveTable(refTable);
        }

        [Test]
        public virtual void CanAddForeignKeyWithUpdateCascade()
        {
            SchemaQualifiedObjectName primaryTable = GetRandomTableName("AddForeignKey_Primary");
            SchemaQualifiedObjectName refTable = GetRandomTableName("AddForeignKey_Ref");
            string foreignKeyName = GetRandomName("FK_Test");

            provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(refTable, new[] { "ID" }, new[] { "654" });

            provider.AddTable(primaryTable,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("RefID", DbType.Int32));

            provider.AddForeignKey(foreignKeyName,
                primaryTable, "RefID", refTable, "ID",
                ForeignKeyConstraint.NoAction, ForeignKeyConstraint.Cascade);

            provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "654" });

            provider.Update(refTable, new[] { "ID" }, new[] { "777" });

            string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
            Assert.AreEqual(777, provider.ExecuteScalar(sql));

            // удаляем таблицы
            provider.RemoveTable(primaryTable);
            provider.RemoveTable(refTable);
        }

        [Test]
        public virtual void CanAddForeignKeyWithDeleteSetNull()
        {
            SchemaQualifiedObjectName primaryTable = GetRandomTableName("AddForeignKey_Primary");
            SchemaQualifiedObjectName refTable = GetRandomTableName("AddForeignKey_Ref");
            string foreignKeyName = GetRandomName("FK_Test");

            provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(refTable, new[] { "ID" }, new[] { "177" });

            provider.AddTable(primaryTable,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("RefID", DbType.Int32));
            provider.AddForeignKey(foreignKeyName, primaryTable, "RefID", refTable, "ID", ForeignKeyConstraint.SetNull);

            provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "177" });
            provider.Delete(refTable);

            string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
            Assert.AreEqual(DBNull.Value, provider.ExecuteScalar(sql));

            // удаляем таблицы
            provider.RemoveTable(primaryTable);
            provider.RemoveTable(refTable);
        }

        [Test]
        public virtual void CanAddForeignKeyWithUpdateSetNull()
        {
            SchemaQualifiedObjectName primaryTable = GetRandomTableName("AddForeignKey_Primary");
            SchemaQualifiedObjectName refTable = GetRandomTableName("AddForeignKey_Ref");
            string foreignKeyName = GetRandomName("FK_Test");

            provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(refTable, new[] { "ID" }, new[] { "654" });

            provider.AddTable(primaryTable,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("RefID", DbType.Int32));

            provider.AddForeignKey(foreignKeyName,
                primaryTable, "RefID", refTable, "ID",
                ForeignKeyConstraint.NoAction, ForeignKeyConstraint.SetNull);

            provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "654" });

            provider.Update(refTable, new[] { "ID" }, new[] { "777" });

            string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
            Assert.AreEqual(DBNull.Value, provider.ExecuteScalar(sql));

            // удаляем таблицы
            provider.RemoveTable(primaryTable);
            provider.RemoveTable(refTable);
        }

        [Test]
        public virtual void CanAddForeignKeyWithDeleteSetDefault()
        {
            SchemaQualifiedObjectName primaryTable = GetRandomTableName("AddForeignKey_Primary");
            SchemaQualifiedObjectName refTable = GetRandomTableName("AddForeignKey_Ref");
            string foreignKeyName = GetRandomName("FK_Test");

            provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(refTable, new[] { "ID" }, new[] { "177" });
            provider.Insert(refTable, new[] { "ID" }, new[] { "998" });

            provider.AddTable(primaryTable,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("RefID", DbType.Int32, ColumnProperty.NotNull, 998));
            provider.AddForeignKey(foreignKeyName, primaryTable, "RefID", refTable, "ID", ForeignKeyConstraint.SetDefault);

            provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "177" });

            string whereSql = provider.FormatSql("{0:NAME} = 177", "ID");
            provider.Delete(refTable, whereSql);

            string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
            Assert.AreEqual(998, provider.ExecuteScalar(sql));

            // удаляем таблицы
            provider.RemoveTable(primaryTable);
            provider.RemoveTable(refTable);
        }

        [Test]
        public virtual void CanAddForeignKeyWithUpdateSetDefault()
        {
            SchemaQualifiedObjectName primaryTable = GetRandomTableName("AddForeignKey_Primary");
            SchemaQualifiedObjectName refTable = GetRandomTableName("AddForeignKey_Ref");
            string foreignKeyName = GetRandomName("FK_Test");

            provider.AddTable(refTable, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(refTable, new[] { "ID" }, new[] { "999" });
            provider.Insert(refTable, new[] { "ID" }, new[] { "654" });

            provider.AddTable(primaryTable,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("RefID", DbType.Int32, ColumnProperty.NotNull, 999));

            provider.AddForeignKey(foreignKeyName,
                primaryTable, "RefID", refTable, "ID",
                ForeignKeyConstraint.NoAction, ForeignKeyConstraint.SetDefault);

            provider.Insert(primaryTable, new[] { "ID", "RefID" }, new[] { "1", "654" });

            string whereSql = provider.FormatSql("{0:NAME} = 654", "ID");
            provider.Update(refTable, new[] { "ID" }, new[] { "777" }, whereSql);

            string sql = provider.FormatSql("select {0:NAME} from {1:NAME}", "RefID", primaryTable);
            Assert.AreEqual(999, provider.ExecuteScalar(sql));

            // удаляем таблицы
            provider.RemoveTable(primaryTable);
            provider.RemoveTable(refTable);
        }

        #endregion

        #region unique constraint

        [Test]
        public virtual void CanAddComplexUniqueConstraint()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddComplexUniqueConstraint");
            string ucName = GetRandomName("UC_AddComplexUniqueConstraint");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestStringColumn1", DbType.String.WithSize(20)),
                new Column("TestStringColumn2", DbType.String.WithSize(20)));
            Assert.IsFalse(provider.ConstraintExists(tableName, ucName));

            provider.AddUniqueConstraint(ucName, tableName, "TestStringColumn1", "TestStringColumn2");

            // пробуем нарушить ограничения
            provider.Insert(tableName,
                new[] { "ID", "TestStringColumn1", "TestStringColumn2" },
                new[] { "1", "xxx", "abc" });

            provider.Insert(tableName,
                new[] { "ID", "TestStringColumn1", "TestStringColumn2" },
                new[] { "2", "111", "abc" });

            provider.Insert(tableName,
                new[] { "ID", "TestStringColumn1", "TestStringColumn2" },
                new[] { "3", "xxx", "222" });

            Assert.Throws<SQLException>(() =>
                provider.Insert(tableName,
                    new[] { "ID", "TestStringColumn1", "TestStringColumn2" },
                    new[] { "4", "xxx", "abc" }));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanCheckThatUniqueConstraintIsExist()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddUniqueConstraint");
            string ucName = GetRandomName("UK_AddUniqueConstraint");

            provider.AddTable(tableName,
                new Column("ID1", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestStringColumn", DbType.String.WithSize(20)));
            Assert.IsFalse(provider.ConstraintExists(tableName, ucName));

            // добавляем UC и проверяем его существование
            provider.AddUniqueConstraint(ucName, tableName, "TestStringColumn");
            Assert.IsTrue(provider.ConstraintExists(tableName, ucName));

            provider.RemoveConstraint(tableName, ucName);
            Assert.IsFalse(provider.ConstraintExists(tableName, ucName));

            provider.RemoveTable(tableName);
        }

        #endregion

        #region check constraint

        [Test]
        public virtual void CanAddCheckConstraint()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddCheckConstraint");
            string constraintName = GetRandomName("CC_AddCheckConstraint");
            provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));

            string checkSql = provider.FormatSql("{0:NAME} > 5", "ID");
            provider.AddCheckConstraint(constraintName, tableName, checkSql);

            provider.Insert(tableName, new[] { "ID" }, new[] { "11" });

            Assert.Throws<SQLException>(() =>
                    provider.Insert(tableName, new[] { "ID" }, new[] { "4" }));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanVerifyThatCheckConstraintIsExist()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("CheckConstraintIsExist");
            string constraintName = GetRandomName("CC_CheckConstraintIsExist");

            provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey));
            Assert.IsFalse(provider.ConstraintExists(tableName, constraintName));

            string checkSql = provider.FormatSql("{0:NAME} > 5", "ID");
            provider.AddCheckConstraint(constraintName, tableName, checkSql);
            Assert.IsTrue(provider.ConstraintExists(tableName, constraintName));

            provider.RemoveConstraint(tableName, constraintName);
            Assert.IsFalse(provider.ConstraintExists(tableName, constraintName));

            provider.RemoveTable(tableName);
        }

        #endregion

        #endregion

        #region index

        [Test]
        public virtual void CanAddAndRemoveIndex()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddIndex");
            string indexName = GetRandomName("ix_moo_");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(10)));

            provider.AddIndex(indexName, false, tableName, new[] { "Name" });
            Assert.IsTrue(provider.IndexExists(indexName, tableName));

            provider.Insert(tableName, new[] { "ID", "Name" }, new[] { "1", "test-name" });
            provider.Insert(tableName, new[] { "ID", "Name" }, new[] { "2", "test-name" });

            provider.RemoveIndex(indexName, tableName);
            Assert.IsFalse(provider.IndexExists(indexName, tableName));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanAddAndRemoveUniqueIndex()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddUniqueIndex");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("Name", DbType.String.WithSize(10)));

            provider.AddIndex("ix_moo", true, tableName, new[] { "Name" });
            Assert.IsTrue(provider.IndexExists("ix_moo", tableName));

            provider.Insert(tableName, new[] { "ID", "Name" }, new[] { "1", "test-name" });
            Assert.Throws<SQLException>(() =>
                    provider.Insert(tableName, new[] { "ID", "Name" }, new[] { "2", "test-name" }));

            provider.RemoveIndex("ix_moo", tableName);
            Assert.IsFalse(provider.IndexExists("ix_moo", tableName));

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanAddAndRemoveComplexIndex()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("AddComplexIndex");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("Name1", DbType.String.WithSize(20)),
                new Column("Name2", DbType.String.WithSize(20)));

            provider.AddIndex("ix_moo", true, tableName, new[] { "Name1", "Name2" });
            Assert.IsTrue(provider.IndexExists("ix_moo", tableName));

            provider.Insert(tableName, new[] { "ID", "Name1", "Name2" }, new[] { "1", "test-name", "xxx" });
            provider.Insert(tableName, new[] { "ID", "Name1", "Name2" }, new[] { "2", "test-name-2", "xxx" });
            provider.Insert(tableName, new[] { "ID", "Name1", "Name2" }, new[] { "3", "test-name", "zzz" });
            Assert.Throws<SQLException>(() =>
                    provider.Insert(tableName, new[] { "ID", "Name1", "Name2" }, new[] { "4", "test-name", "xxx" }));


            provider.RemoveIndex("ix_moo", tableName);
            Assert.IsFalse(provider.IndexExists("ix_moo", tableName));

            provider.RemoveTable(tableName);
        }

        #endregion

        #region DML

        #region insert

        [Test]
        public virtual void CanInsertData()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("InsertTest");
            provider.AddTable(tableName,
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("Title", DbType.String.WithSize(30), ColumnProperty.Null),
                new Column("Title2", DbType.String.WithSize(30)));

            provider.Insert(tableName, new[] { "Id", "Title", "Title2" }, new[] { "126", null, "Muad'Dib" });

            string sql = provider.FormatSql("SELECT {0:NAME}, {1:NAME}, {2:NAME} FROM {3:NAME}",
                "Id", "Title", "Title2", tableName);
            using (IDataReader reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(126, reader[0]);
                Assert.AreEqual(DBNull.Value, reader[1]);
                Assert.AreEqual("Muad'Dib", reader.GetString(2));
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanInsertDataFromObject()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("InsertTest");

            provider.AddTable(tableName,
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("Title", DbType.String.WithSize(30), ColumnProperty.Null),
                new Column("Title2", DbType.String.WithSize(30)));

            provider.Insert(tableName,
                new
                {
                    Id = 129,
                    Title = (object)null,
                    Title2 = "lewqkghwl"
                });

            string sql = provider.FormatSql("SELECT {0:NAME}, {1:NAME}, {2:NAME} FROM {3:NAME}",
                "Id", "Title", "Title2", tableName);

            using (IDataReader reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(129, reader[0]);
                Assert.AreEqual(DBNull.Value, reader[1]);
                Assert.AreEqual("lewqkghwl", reader.GetString(2));
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        #endregion

        #region update

        [Test]
        public virtual void CanUpdateData()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("UpdateData");

            provider.AddTable(tableName,
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestInteger", DbType.Int32));

            provider.Insert(tableName, new[] { "Id", "TestInteger" }, new[] { "1", "1122" });
            provider.Insert(tableName, new[] { "Id", "TestInteger" }, new[] { "2", "3344" });

            provider.Update(tableName, new[] { "TestInteger" }, new[] { "42" });

            string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME}", "TestInteger", tableName);
            using (IDataReader reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(42, reader["TestInteger"]);
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(42, reader["TestInteger"]);
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanUpdateDataFromObject()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("UpdateData");

            provider.AddTable(tableName,
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestInteger", DbType.Int32));

            provider.Insert(tableName, new[] { "Id", "TestInteger" }, new[] { "1", "1122" });
            provider.Insert(tableName, new[] { "Id", "TestInteger" }, new[] { "2", "3344" });

            provider.Update(tableName, new { TestInteger = 249 });

            string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME}", "TestInteger", tableName);
            using (IDataReader reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(249, reader["TestInteger"]);
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(249, reader["TestInteger"]);
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanUpdateWithNullData()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("UpdateWithNullData");

            provider.AddTable(tableName,
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestInteger", DbType.Int32));

            provider.Insert(tableName, new[] { "Id", "TestInteger" }, new[] { "1", null });
            provider.Insert(tableName, new[] { "Id", "TestInteger" }, new[] { "2", "5566" });

            provider.Update(tableName, new[] { "TestInteger" }, new string[] { null });

            string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME}", "TestInteger", tableName);
            using (IDataReader reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(DBNull.Value, reader[0]);
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(DBNull.Value, reader[0]);
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanUpdateDataWithWhere()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("UpdateDataWithWhere");

            provider.AddTable(tableName,
                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("TestInteger", DbType.Int32));

            provider.Insert(tableName, new[] { "Id", "TestInteger" }, new[] { "1", "123" });
            provider.Insert(tableName, new[] { "Id", "TestInteger" }, new[] { "2", "456" });

            string whereSql = provider.FormatSql("{0:NAME} = 2", "Id");
            provider.Update(tableName, new[] { "TestInteger" }, new[] { "777" }, whereSql);

            string sql = provider.FormatSql(
                "SELECT {0:NAME}, {1:NAME} FROM {2:NAME} ORDER BY {0:NAME}", "Id", "TestInteger", tableName);

            using (IDataReader reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(1, reader[0]);
                Assert.AreEqual(123, reader[1]);
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(2, reader[0]);
                Assert.AreEqual(777, reader[1]);
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        #endregion

        #region delete

        [Test]
        public virtual void CanDeleteData()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("DeleteData");

            provider.AddTable(tableName, new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(tableName, new[] { "Id" }, new[] { "1023" });
            provider.Insert(tableName, new[] { "Id" }, new[] { "2047" });

            string sqlWhere = provider.FormatSql("{0:NAME} = 2047", "Id");
            provider.Delete(tableName, sqlWhere);

            string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME}", "Id", tableName);
            using (IDataReader reader = provider.ExecuteReader(sql))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(1023, Convert.ToInt32(reader[0]));
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        [Test]
        public virtual void CanDeleteAllData()
        {
            SchemaQualifiedObjectName tableName = GetRandomTableName("DeleteAllData");

            provider.AddTable(tableName, new Column("Id", DbType.Int32, ColumnProperty.PrimaryKey));
            provider.Insert(tableName, new[] { "Id" }, new[] { "1111" });
            provider.Insert(tableName, new[] { "Id" }, new[] { "2222" });

            provider.Delete(tableName);

            string sql = provider.FormatSql("SELECT {0:NAME} FROM {1:NAME}", "Id", tableName);
            using (IDataReader reader = provider.ExecuteReader(sql))
            {
                Assert.IsFalse(reader.Read());
            }

            provider.RemoveTable(tableName);
        }

        #endregion

        #endregion

        #region for migrator core

        [Test]
        public virtual void SchemaInfoTableShouldBeCreatedWhenGetAppliedMigrations()
        {
            const string KEY = "mi mi mi";
            const string SCHEMA_INFO_TABLE_NAME = "SchemaInfo";

            Assert.IsFalse(provider.TableExists(SCHEMA_INFO_TABLE_NAME));

            var appliedMigrations = provider.GetAppliedMigrations(KEY);
            Assert.AreEqual(0, appliedMigrations.Count);
            Assert.IsTrue(provider.TableExists(SCHEMA_INFO_TABLE_NAME));

            provider.RemoveTable(SCHEMA_INFO_TABLE_NAME);
        }

        [Test]
        public virtual void SchemaInfoTableShouldBeCreatedWhenMigrationApplied()
        {
            const string KEY = "mi mi mi";
            const string SCHEMA_INFO_TABLE_NAME = "SchemaInfo";

            Assert.IsFalse(provider.TableExists(SCHEMA_INFO_TABLE_NAME));

            provider.MigrationApplied(1, KEY);
            Assert.IsTrue(provider.TableExists(SCHEMA_INFO_TABLE_NAME));

            provider.RemoveTable(SCHEMA_INFO_TABLE_NAME);
        }

        [Test]
        public virtual void SchemaInfoTableShouldBeCreatedWhenMigrationUnApplied()
        {
            const string KEY = "mi mi mi";
            const string SCHEMA_INFO_TABLE_NAME = "SchemaInfo";

            Assert.IsFalse(provider.TableExists(SCHEMA_INFO_TABLE_NAME));

            provider.MigrationUnApplied(1, KEY);
            Assert.IsTrue(provider.TableExists(SCHEMA_INFO_TABLE_NAME));

            provider.RemoveTable(SCHEMA_INFO_TABLE_NAME);
        }

        [Test]
        public virtual void CanGetAppliedMigrations()
        {
            // todo: разбить этот тест на несколько
            // todo: проверить обновление структуры таблицы "SchemaInfo"
            const string KEY = "mi mi mi";
            const string SCHEMA_INFO_TABLE_NAME = "SchemaInfo";

            Assert.IsFalse(provider.TableExists(SCHEMA_INFO_TABLE_NAME));

            // Check that a "set" called after the first run works.
            provider.MigrationApplied(123, KEY);
            provider.MigrationApplied(125, KEY);
            var appliedMigrations = provider.GetAppliedMigrations(KEY);
            Assert.AreEqual(2, appliedMigrations.Count);
            Assert.AreEqual(123, appliedMigrations[0]);
            Assert.AreEqual(125, appliedMigrations[1]);

            provider.MigrationUnApplied(123, KEY);
            appliedMigrations = provider.GetAppliedMigrations(KEY);
            Assert.AreEqual(1, appliedMigrations.Count);
            Assert.AreEqual(125, appliedMigrations[0]);

            var appliedMigrationsForAnotherKey = provider.GetAppliedMigrations("d3d4136830a94fdca8bd19f1c2eb9e81");
            Assert.AreEqual(0, appliedMigrationsForAnotherKey.Count);

            provider.RemoveTable(SCHEMA_INFO_TABLE_NAME);
        }

        #endregion

        #endregion

        #region helpers

        protected virtual string GetRandomName(string baseName = "")
        {
            string guid = Guid.NewGuid().ToString().Replace("-", string.Empty).ToLower();
            return string.Format("{0}{1}", baseName, guid);
        }

        protected virtual SchemaQualifiedObjectName GetRandomTableName(string baseName = "")
        {
            return GetRandomName(baseName).WithSchema(GetSchemaForCreateTables());
        }

        protected virtual bool IgnoreCase
        {
            get { return false; }
        }

        private IEqualityComparer<string> comparer;

        protected IEqualityComparer<string> StrComparer
        {
            get { return comparer ?? (comparer = new NameComparer(IgnoreCase)); }
        }

        /// <summary>
        /// Схема, которая используется для создания таблиц
        /// </summary>
        protected virtual string GetSchemaForCreateTables()
        {
            return string.Empty;
        }

        /// <summary>
        /// Схема, которая используется дял проверки имен таблиц
        /// (может отличаться от схемы, используемой для создания таблиц, т.к. в случае пустой схемы
        ///  создаются таблицы в текущей схеме пользователя)
        /// </summary>
        /// <returns></returns>
        protected abstract string GetSchemaForCompare();

        #endregion
    }
}