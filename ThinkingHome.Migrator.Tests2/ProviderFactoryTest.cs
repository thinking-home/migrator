using NUnit.Framework;
using ThinkingHome.Migrator.Providers;
using ThinkingHome.Migrator.Providers.PostgreSQL;
using ThinkingHome.Migrator.Providers.SQLite;

namespace ThinkingHome.Migrator.Tests
{
    [TestFixture]
    public class ProviderFactoryTest
    {
        #region Shortcuts tests

        [Test, Category("PostgreSQL")]
        public void PostgreSQLShortcutTest()
        {
            Assert.That(ProviderFactory.Create("PostgreS") is PostgreSQLProviderFactory);
        }

        [Test, Category("SQLite")]
        public void SQLiteShortcutTest()
        {
            Assert.That(ProviderFactory.Create("SQLite") is SQLiteProviderFactory);
        }

        #endregion
    }
}