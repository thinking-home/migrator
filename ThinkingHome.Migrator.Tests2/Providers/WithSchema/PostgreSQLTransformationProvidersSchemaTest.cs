namespace ThinkingHome.Migrator.Tests.Providers.WithSchema
{
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