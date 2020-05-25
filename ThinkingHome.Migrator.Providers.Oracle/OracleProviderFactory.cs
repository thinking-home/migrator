using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace ThinkingHome.Migrator.Providers.Oracle
{
    public class OracleProviderFactory :
        ProviderFactory<OracleTransformationProvider,OracleConnection>
    {
        protected override OracleTransformationProvider CreateProviderInternal(OracleConnection connection, ILogger logger)
        {
            return new OracleTransformationProvider(connection, logger);
        }

        protected override OracleConnection CreateConnectionInternal(string connectionString)
        {
            return new OracleConnection(connectionString);
        }
    }
}
