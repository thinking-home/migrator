using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace ThinkingHome.Migrator.Providers.MySql
{
    public class MySqlProviderFactory :
        ProviderFactory<MySqlTransformationProvider, MySqlConnection>
    {
        protected override MySqlTransformationProvider CreateProviderInternal(MySqlConnection connection, ILogger logger)
        {
            return new MySqlTransformationProvider(connection, logger);
        }

        protected override MySqlConnection CreateConnectionInternal(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
    }
}
