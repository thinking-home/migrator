using Xunit;
using ThinkingHome.Migrator.Providers;
using ThinkingHome.Migrator.Providers.MySql;
using ThinkingHome.Migrator.Providers.Oracle;
using ThinkingHome.Migrator.Providers.PostgreSQL;
using ThinkingHome.Migrator.Providers.SqlServer;
using ThinkingHome.Migrator.Providers.SQLite;

namespace ThinkingHome.Migrator.Tests
{
    public class ProviderFactoryTest
    {
        #region Shortcuts tests

        [Fact]
        public void PostgreSQLShortcutTest()
        {
            Assert.IsType<PostgreSQLProviderFactory>(ProviderFactory.Create("PostgreS"));
        }

        [Fact]
        public void OracleShortcutTest()
        {
            Assert.IsType<OracleProviderFactory>(ProviderFactory.Create("Oracle"));
        }

        [Fact]
        public void SQLiteShortcutTest()
        {
            Assert.IsType<SQLiteProviderFactory>(ProviderFactory.Create("SQLite"));
        }

        [Fact]
        public void SqlServerShortcutTest()
        {
            Assert.IsType<SqlServerProviderFactory>(ProviderFactory.Create("SqlServer"));
        }

        [Fact]
        public void MySqlShortcutTest()
        {
            Assert.IsType<MySqlProviderFactory>(ProviderFactory.Create("MySql"));
        }

        #endregion
    }
}
