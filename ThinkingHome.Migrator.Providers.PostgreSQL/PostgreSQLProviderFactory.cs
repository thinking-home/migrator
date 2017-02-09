using Microsoft.Extensions.Logging;
using Npgsql;

namespace ThinkingHome.Migrator.Providers.PostgreSQL
{
    public class PostgreSQLProviderFactory :
        ProviderFactory<PostgreSQLTransformationProvider,NpgsqlConnection>
    {
        protected override PostgreSQLTransformationProvider CreateProviderInternal(NpgsqlConnection connection, ILogger logger)
        {
            return new PostgreSQLTransformationProvider(connection, logger);
        }

        protected override NpgsqlConnection CreateConnectionInternal(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}