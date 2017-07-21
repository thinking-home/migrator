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

        #region foreign keys

        [Fact]
        public override void CanAddForeignKey()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddForeignKey);
        }

        [Fact]
        public override void CanAddForeignKeyWithDeleteCascade()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddForeignKeyWithDeleteCascade);
        }

        [Fact]
        public override void CanAddForeignKeyWithDeleteSetDefault()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddForeignKeyWithDeleteSetDefault);
        }

        [Fact]
        public override void CanAddForeignKeyWithDeleteSetNull()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddForeignKeyWithDeleteSetNull);
        }

        [Fact]
        public override void CanAddForeignKeyWithUpdateCascade()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddForeignKeyWithUpdateCascade);
        }

        [Fact]
        public override void CanAddForeignKeyWithUpdateSetDefault()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddForeignKeyWithUpdateSetDefault);
        }

        [Fact]
        public override void CanAddForeignKeyWithUpdateSetNull()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddForeignKeyWithUpdateSetNull);
        }

        // check constraints
        [Fact]
        public override void CanAddCheckConstraint()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddCheckConstraint);
        }

        [Fact]
        public override void CanVerifyThatCheckConstraintIsExist()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanVerifyThatCheckConstraintIsExist);
        }

        [Fact]
        public override void CanAddComplexForeignKey()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddComplexForeignKey);
        }

        [Fact]
        public override void CanAddComplexUniqueConstraint()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddComplexUniqueConstraint);
        }

        [Fact]
        public override void CanAddPrimaryKey()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanAddPrimaryKey);
        }

        [Fact]
        public override void CanCheckThatPrimaryKeyIsExist()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanCheckThatPrimaryKeyIsExist);
        }

        [Fact]
        public override void CanCheckThatUniqueConstraintIsExist()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanCheckThatUniqueConstraintIsExist);
        }

        #endregion

        #region change column

        [Fact]
        public override void CanChangeColumnType()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanChangeColumnType);
        }

        [Fact]
        public override void CanChangeDefaultValueForColumn()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanChangeDefaultValueForColumn);
        }

        [Fact]
        public override void CanChangeNotNullProperty()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanChangeNotNullProperty);
        }

        [Fact]
        public override void CanRemoveColumn()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanRemoveColumn);
        }

        [Fact]
        public override void CanRenameColumn()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanRenameColumn);
        }

        [Fact]
        public override void CanSetNotNullRepeatedly()
        {
            Assert.Throws<NotSupportedException>((Action)base.CanSetNotNullRepeatedly);
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