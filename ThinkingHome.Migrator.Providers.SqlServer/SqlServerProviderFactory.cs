using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ThinkingHome.Migrator.Providers.SqlServer
{
    public class SqlServerProviderFactory :
        ProviderFactory<SqlServerTransformationProvider, SqlConnection>
    {
        protected override SqlServerTransformationProvider CreateProviderInternal(SqlConnection connection, ILogger logger)
        {
            return new SqlServerTransformationProvider(connection, logger);
        }

        protected override SqlConnection CreateConnectionInternal(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}
