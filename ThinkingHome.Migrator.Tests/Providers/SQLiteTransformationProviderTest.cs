using System;
using System.Data;
using Microsoft.Extensions.Logging;
using Xunit;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Providers.SQLite;

namespace ThinkingHome.Migrator.Tests.Providers
{
    public class SQLiteTransformationProviderTest
        : TransformationProviderTestBase
    {
        public override ITransformationProvider CreateProvider(ILogger logger = null)
        {
            return new SQLiteProviderFactory()
                .CreateProvider("Data Source=:memory:;", logger);
        }

        #region Overrides of TransformationProviderTestBase<SQLiteTransformationProvider>

        protected override string GetSchemaForCompare()
        {
            return string.Empty;
        }

        protected override string BatchSql => @"
			insert into [BatchSqlTest] ([Id], [TestId]) values (11, 111);
            insert into [BatchSqlTest] ([Id], [TestId]) values (22, 222);
            insert into [BatchSqlTest] ([Id], [TestId]) values (33, 333);
            ";

        #endregion

        public override void CanRemoveColumnWithContrainsts()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKey);
        }
        
        #region foreign keys

        [Fact]
        public override void CanAddForeignKey()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKey);
        }

        [Fact]
        public override void CanAddForeignKeyWithDeleteCascade()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteCascade);
        }

        [Fact]
        public override void CanAddForeignKeyWithDeleteSetDefault()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteSetDefault);
        }

        [Fact]
        public override void CanAddForeignKeyWithDeleteSetNull()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteSetNull);
        }

        [Fact]
        public override void CanAddForeignKeyWithUpdateCascade()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateCascade);
        }

        [Fact]
        public override void CanAddForeignKeyWithUpdateSetDefault()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateSetDefault);
        }

        [Fact]
        public override void CanAddForeignKeyWithUpdateSetNull()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithUpdateSetNull);
        }

        // check constraints
        [Fact]
        public override void CanAddCheckConstraint()
        {
            Assert.Throws<NotSupportedException>(base.CanAddCheckConstraint);
        }

        [Fact]
        public override void CanVerifyThatCheckConstraintIsExist()
        {
            Assert.Throws<NotSupportedException>(base.CanVerifyThatCheckConstraintIsExist);
        }

        [Fact]
        public override void CanAddComplexForeignKey()
        {
            Assert.Throws<NotSupportedException>(base.CanAddComplexForeignKey);
        }

        [Fact]
        public override void CanAddComplexUniqueConstraint()
        {
            Assert.Throws<NotSupportedException>(base.CanAddComplexUniqueConstraint);
        }

        [Fact]
        public override void CanAddPrimaryKey()
        {
            Assert.Throws<NotSupportedException>(base.CanAddPrimaryKey);
        }

        [Fact]
        public override void CanCheckThatPrimaryKeyIsExist()
        {
            Assert.Throws<NotSupportedException>(base.CanCheckThatPrimaryKeyIsExist);
        }

        [Fact]
        public override void CanCheckThatUniqueConstraintIsExist()
        {
            Assert.Throws<NotSupportedException>(base.CanCheckThatUniqueConstraintIsExist);
        }

        #endregion

        #region change column

        [Fact]
        public override void CanChangeColumnType()
        {
            Assert.Throws<NotSupportedException>(base.CanChangeColumnType);
        }

        [Fact]
        public override void CanChangeDefaultValueForColumn()
        {
            Assert.Throws<NotSupportedException>(base.CanChangeDefaultValueForColumn);
        }

        [Fact]
        public override void CanChangeNotNullProperty()
        {
            Assert.Throws<NotSupportedException>(base.CanChangeNotNullProperty);
        }

        [Fact]
        public override void CanRemoveColumn()
        {
            Assert.Throws<NotSupportedException>(base.CanRemoveColumn);
        }

        [Fact]
        public override void CanRenameColumn()
        {
            Assert.Throws<NotSupportedException>(base.CanRenameColumn);
        }

        [Fact]
        public override void CanSetNotNullRepeatedly()
        {
            Assert.Throws<NotSupportedException>(base.CanSetNotNullRepeatedly);
        }

        [Fact]
        public override void CantRemoveUnexistingColumn()
        {
            string tableName = GetRandomName("RemoveUnexistingColumn");

            provider.AddTable(tableName, new Column("ID", DbType.Int32));

            Assert.Throws<NotSupportedException>(() =>
                provider.RemoveColumn(tableName, GetRandomName()));

            provider.RemoveTable(tableName);
        }


        #endregion
    }}
