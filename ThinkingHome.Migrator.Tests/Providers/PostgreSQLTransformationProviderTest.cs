using System.Text;
using Microsoft.Extensions.Logging;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Providers.PostgreSQL;

namespace ThinkingHome.Migrator.Tests.Providers
{
    public class PostgreSQLTransformationProviderTest : TransformationProviderTestBase
    {
        public override ITransformationProvider CreateProvider(ILogger logger = null)
        {
            return new PostgreSQLProviderFactory()
                .CreateProvider("host=localhost;port=5432;database=migrations;user name=migrator;password=123", logger);
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

        protected override string ResourceSql => "ThinkingHome.Migrator.Tests.TestMigrations.pgsql.test.sql";

        protected override string GetSchemaForCompare()
        {
            return provider.ExecuteScalar("select current_schema()").ToString();
        }

    }
}