using ThinkingHome.Migrator.Framework;

namespace ThinkingHome.Migrator.Tests.TestMigrations
{
    [Migration(4, WithoutTransaction = true)]
    public class TestMigration04 : Migration
    {
        public override void Apply()
        {
            Database.ExecuteNonQuery("up4");
        }

        public override void Revert()
        {
            Database.ExecuteNonQuery("down4");
        }
    }
}