using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ThinkingHome.Migrator.Providers.SQLite
{
    public class SQLiteProviderFactory :
        ProviderFactory<SQLiteTransformationProvider, SqliteConnection>
    {
        protected override SQLiteTransformationProvider CreateProviderInternal(SqliteConnection connection, ILogger logger)
        {
            return new SQLiteTransformationProvider(connection, logger);
        }

        protected override SqliteConnection CreateConnectionInternal(string connectionString)
        {
            return new SqliteConnection(connectionString);
        }
    }
}