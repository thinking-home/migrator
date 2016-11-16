using System.Text;
using NUnit.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Providers.PostgreSQL;

namespace ThinkingHome.Migrator.Tests.Providers
{
    [TestFixture, Category("PostgreSQL")]
    public class PostgreSQLTransformationProviderTest : TransformationProviderTestBase
    {
        public override ITransformationProvider CreateProvider()
        {
            return new PostgreSQLProviderFactory()
                .CreateProvider("host=localhost;port=5432;database=migrations;user name=postgres;password=123");
        }

        protected override string BatchSql
        {
            get
            {
                var sb = new StringBuilder();

                sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (11, 111);");
                sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (22, 222);");
                sb.AppendLine("insert into \"BatchSqlTest\" (\"Id\", \"TestId\") values (33, 333);");

                return sb.ToString();
            }
        }

        protected override string GetSchemaForCompare()
        {
            return provider.ExecuteScalar("select current_schema()").ToString();
        }

    }
}