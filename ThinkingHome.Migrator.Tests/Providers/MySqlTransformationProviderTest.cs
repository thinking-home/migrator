using System;
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
                .CreateProvider("Database=migrations;Data Source=localhost;User Id=migrator;Password=123;", logger);
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

		public override void CanVerifyThatCheckConstraintIsExist()
		{
			// todo: пройтись по всем тестам с NotSupportedException и проверить необходимость выдачи исключения
			Assert.Throws<NotSupportedException>((Action) base.CanVerifyThatCheckConstraintIsExist);
		}

		public override void CanAddCheckConstraint()
		{
			Assert.Throws<NotSupportedException>((Action) base.CanAddCheckConstraint);
		}

		public override void CanRenameColumn()
		{
			Assert.Throws<NotSupportedException>((Action) base.CanRenameColumn);
		}

		public override void CanAddForeignKeyWithDeleteSetDefault()
		{
			Assert.Throws<NotSupportedException>((Action) base.CanAddForeignKeyWithDeleteSetDefault);
		}

		public override void CanAddForeignKeyWithUpdateSetDefault()
		{
			Assert.Throws<NotSupportedException>((Action) base.CanAddForeignKeyWithUpdateSetDefault);
		}

	    public override void CanRollbackTransactions()
	    {
		    provider.AddTable("transtest3", new Column("id", DbType.Int32));

		    provider.BeginTransaction();

		    provider.Insert("transtest3", new {id = 1});
		    provider.Insert("transtest3", new {id = 2});

		    Assert.Equal(2, (long) provider.ExecuteScalar("select count(*) from transtest3"));

		    provider.Rollback();

		    Assert.Equal(0, (long) provider.ExecuteScalar("select count(*) from transtest3"));

		    provider.TableExists("transtest3");
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
