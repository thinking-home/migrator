using ThinkingHome.Migrator.Framework;

namespace ThinkingHome.Migrator.Tests.TestMigrations
{
    [Migration(3, Ignore = true)]
    public class TestMigration03 : Migration
    {
        public override void Apply()
        {

        }
    }
}