namespace ThinkingHome.Migrator.Tests.Providers.WithSchema
{
    public class SqlServerTransformationProvidersSchemaTest : SqlServerTransformationProviderTest
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
