using Microsoft.Extensions.Logging;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Providers.SqlServer;

namespace ThinkingHome.Migrator.Tests.Providers
{
    public class SqlServerTransformationProviderTest : TransformationProviderTestBase
    {
        public override ITransformationProvider CreateProvider(ILogger logger = null)
        {
            return new SqlServerProviderFactory()
                .CreateProvider("Server=localhost;Database=migrations;User Id=sa;Password=x987(!)654;", logger);
        }

        protected override string BatchSql => @"
			insert into [BatchSqlTest] ([Id], [TestId]) values (11, 111)
			GO
			insert into [BatchSqlTest] ([Id], [TestId]) values (22, 222)
			GO
			insert into [BatchSqlTest] ([Id], [TestId]) values (33, 333)";

        protected override string GetSchemaForCompare()
        {
            return provider.ExecuteScalar("select SCHEMA_NAME()").ToString();
        }
    }
}
