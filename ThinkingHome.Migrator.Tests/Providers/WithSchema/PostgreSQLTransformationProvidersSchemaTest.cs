namespace ThinkingHome.Migrator.Tests.Providers.WithSchema
{
    public class PostgreSQLTransformationProvidersSchemaTest : PostgreSQLTransformationProviderTest
    {
        protected override string GetSchemaForCreateTables()
        {
            return "Moo4";
        }

        protected override string GetSchemaForCompare()
        {
            return "Moo4";
        }
    }
}