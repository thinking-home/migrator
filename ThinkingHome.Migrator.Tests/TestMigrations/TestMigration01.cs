using ThinkingHome.Migrator.Framework;

[assembly: MigrationAssembly("test-key111")]

namespace ThinkingHome.Migrator.Tests.TestMigrations
{
    [Migration(1)]
    public class TestMigration01 : Migration
    {
        public override void Apply()
        {

        }
    }
}