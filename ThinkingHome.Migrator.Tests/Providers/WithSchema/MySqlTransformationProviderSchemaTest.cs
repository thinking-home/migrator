namespace ThinkingHome.Migrator.Tests.Providers.WithSchema
{
    public class MySqlTransformationProviderSchemaTest : MySqlTransformationProviderTest
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
