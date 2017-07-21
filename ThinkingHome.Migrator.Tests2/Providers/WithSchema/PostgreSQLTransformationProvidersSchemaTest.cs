using NUnit.Framework;

namespace ThinkingHome.Migrator.Tests.Providers.WithSchema
{
    [TestFixture, Category("PostgreSQL")]
    public class PostgreSQLTransformationProvidersSchemaTest : PostgreSQLTransformationProviderTest
    {
        protected override string GetSchemaForCreateTables()
        {
            return "Moo";
        }

        protected override string GetSchemaForCompare()
        {
            return "Moo";
        }
    }
}