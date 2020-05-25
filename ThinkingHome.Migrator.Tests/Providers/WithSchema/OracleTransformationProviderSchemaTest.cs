namespace ThinkingHome.Migrator.Tests.Providers.WithSchema
{
    public class OracleTransformationProviderSchemaTest : OracleTransformationProviderTest
    {
        protected override string GetSchemaForCreateTables()
        {
            return "MOO";
        }

        protected override string GetSchemaForCompare()
        {
            return "MOO";
        }
    }
}
