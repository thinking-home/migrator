using System;
using System.Text;
using Microsoft.Extensions.Logging;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Providers.Oracle;
using Xunit;

namespace ThinkingHome.Migrator.Tests.Providers
{
    public class OracleTransformationProviderTest : TransformationProviderTestBase
    {
        public override ITransformationProvider CreateProvider(ILogger logger = null)
        {
            return new OracleProviderFactory()
                .CreateProvider("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER = DEDICATED)(SERVICE_NAME = XE)));User Id=TEST;Password=123;", logger);
        }

        protected override string BatchSql {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (11, 111)");
                sb.AppendLine("/");
                sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (22, 222)");
                sb.AppendLine("/");
                sb.AppendLine("/");
                sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (33, 333)");

                return sb.ToString();
            }
        }

        protected override string ResourceSql => "ThinkingHome.Migrator.Tests.TestMigrations.pgsql.test.sql";

        protected override string GetSchemaForCompare()
        {
            return provider.ExecuteScalar("select user from dual").ToString();
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

        [Fact]
        public override void CanAddForeignKeyWithDeleteSetDefault()
        {
            Assert.Throws<NotSupportedException>(base.CanAddForeignKeyWithDeleteSetDefault);
        }

        protected override string GetRandomName(string baseName = "")
        {
            return base.GetRandomName(baseName).Substring(0, 27);
        }
    }
}