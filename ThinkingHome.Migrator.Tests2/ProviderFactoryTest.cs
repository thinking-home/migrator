using Xunit;
using ThinkingHome.Migrator.Providers;
using ThinkingHome.Migrator.Providers.PostgreSQL;
using ThinkingHome.Migrator.Providers.SQLite;

namespace ThinkingHome.Migrator.Tests
{
    public class ProviderFactoryTest
    {
        #region Shortcuts tests

        [Fact]
        public void PostgreSQLShortcutTest()
        {
            Assert.True(ProviderFactory.Create("PostgreS") is PostgreSQLProviderFactory);
        }

        [Fact]
        public void SQLiteShortcutTest()
        {
            Assert.True(ProviderFactory.Create("SQLite") is SQLiteProviderFactory);
        }

        #endregion
    }
}