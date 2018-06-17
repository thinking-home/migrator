﻿using System;
using System.Data;
using Microsoft.Extensions.Logging;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Providers.MySql;
using Xunit;

namespace ThinkingHome.Migrator.Tests.Providers
{
    public class MySqlTransformationProviderTest : TransformationProviderTestBase
    {
        public override ITransformationProvider CreateProvider(ILogger logger = null)
        {
            return new MySqlProviderFactory()
                .CreateProvider("Database=migrations;Data Source=localhost;User Id=migrator;Password=123;SslMode=none;", logger);
        }

        #region Overrides of TransformationProviderTestBase<MySqlTransformationProvider>

        protected override string GetSchemaForCompare()
        {
            return provider.ExecuteScalar("SELECT SCHEMA()").ToString();
        }

        protected override bool IgnoreCase => true;

        protected override string BatchSql => @"
				insert into `BatchSqlTest` (`Id`, `TestId`) values (11, 111);
				insert into `BatchSqlTest` (`Id`, `TestId`) values (22, 222);
				insert into `BatchSqlTest` (`Id`, `TestId`) values (33, 333);";

        #endregion

        #region override tests

        [Fact]
        public override void CanVerifyThatCheckConstraintIsExist()
        {
            // todo: пройтись по всем тестам с NotSupportedException и проверить необходимость выдачи исключения
            Assert.Throws<NotSupportedException>((Action) base.CanVerifyThatCheckConstraintIsExist);
        }

        [Fact]
        public override void CanAddCheckConstraint()
        {
            Assert.Throws<NotSupportedException>((Action) base.CanAddCheckConstraint);
        }

        [Fact]
        public override void CanRenameColumn()
        {
            Assert.Throws<NotSupportedException>((Action) base.CanRenameColumn);
        }

        [Fact]
        public override void CanAddForeignKeyWithDeleteSetDefault()
        {
            Assert.Throws<NotSupportedException>((Action) base.CanAddForeignKeyWithDeleteSetDefault);
        }

        [Fact]
        public override void CanAddForeignKeyWithUpdateSetDefault()
        {
            Assert.Throws<NotSupportedException>((Action) base.CanAddForeignKeyWithUpdateSetDefault);
        }

        [Fact]
        public override void CanRollbackTransactions()
        {
            var primaryTable = GetRandomTableName("transtest3");
            provider.AddTable(primaryTable, new Column("id", DbType.Int32));

            provider.BeginTransaction();

            provider.Insert(primaryTable, new {id = 1});
            provider.Insert(primaryTable, new {id = 2});

            var countSql = provider.FormatSql("select count(*) from {0:NAME}", primaryTable);
            Assert.Equal(2, (long) provider.ExecuteScalar(countSql));

            provider.Rollback();

            Assert.Equal(0, (long) provider.ExecuteScalar(countSql));

            provider.TableExists(primaryTable);
        }

        #region primary key

        // проверка, что в MySQL первичный ключ всегда называется "PRIMARY"
        [Fact]
        public void PrimaryKeyIsNamedPrimary()
        {
            // в отличие от стандартного теста, сравнение имен ключей происходит без учета регистра
            string tableName = GetRandomName("TableWCPKmysql");

            provider.AddTable(tableName,
                new Column("ID", DbType.Int32, ColumnProperty.PrimaryKey),
                new Column("ID2", DbType.Int32, ColumnProperty.PrimaryKey)
            );

            Assert.True(provider.ConstraintExists(tableName, "PRIMARY"));

            provider.RemoveTable(tableName);
        }

        [Fact]
        public override void CanCheckThatPrimaryKeyIsExist()
        {
            string tableName = GetRandomName("CheckThatPrimaryKeyIsExist");
            string pkName = GetRandomName("PK_CheckThatPrimaryKeyIsExist");

            provider.AddTable(tableName, new Column("ID", DbType.Int32, ColumnProperty.NotNull));
            Assert.False(provider.ConstraintExists(tableName, pkName));

            provider.AddPrimaryKey(pkName, tableName, "ID");
            Assert.True(provider.ConstraintExists(tableName, "PRIMARY"));

            provider.RemoveConstraint(tableName, "PRIMARY");
            Assert.False(provider.ConstraintExists(tableName, "PRIMARY"));

            provider.RemoveTable(tableName);
        }

        #endregion

        #endregion
    }
}