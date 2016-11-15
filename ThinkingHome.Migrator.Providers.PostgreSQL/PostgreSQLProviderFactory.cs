using Npgsql;

namespace ThinkingHome.Migrator.Providers.PostgreSQL
{
    public class PostgreSQLProviderFactory :
        ProviderFactory<PostgreSQLTransformationProvider,NpgsqlConnection>
    {
        protected override PostgreSQLTransformationProvider CreateProviderInternal(NpgsqlConnection connection)
        {
            return new PostgreSQLTransformationProvider(connection);
        }

        protected override NpgsqlConnection CreateConnectionInternal(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }
    }
}