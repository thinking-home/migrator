using System.Data;
using ThinkingHome.Migrator.Framework.Interfaces;

namespace ThinkingHome.Migrator.Providers
{
    public interface IProviderFactory
    {
        ITransformationProvider CreateProvider(string connectionString);
        ITransformationProvider CreateProvider(IDbConnection connection);
        IDbConnection CreateConnection(string connectionString);
    }
}