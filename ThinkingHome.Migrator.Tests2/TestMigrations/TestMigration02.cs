using ThinkingHome.Migrator.Framework;

namespace ThinkingHome.Migrator.Tests.TestMigrations
{
    [Migration(2)]
    public class TestMigration02 : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery("up");
        }

        public override void Revert()
        {
            Database.ExecuteNonQuery("down");
        }
    }
}