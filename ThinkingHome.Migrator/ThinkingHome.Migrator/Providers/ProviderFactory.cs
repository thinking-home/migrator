using System;
using System.Data;
using ThinkingHome.Migrator.Framework.Interfaces;

namespace ThinkingHome.Migrator.Providers
{
    public abstract class ProviderFactory<TProvider, TConnection> : IProviderFactory
        where TProvider : TransformationProvider<TConnection>
        where TConnection : IDbConnection
    {
        protected abstract TProvider CreateProviderInternal(TConnection connection);
        protected abstract TConnection CreateConnectionInternal(string connectionString);

        public IDbConnection CreateConnection(string connectionString)
        {
            return CreateConnectionInternal(connectionString);
        }

        public ITransformationProvider CreateProvider(IDbConnection connection)
        {
            if (!(connection is TConnection)) throw new InvalidCastException();

            return CreateProviderInternal((TConnection) connection);
        }

        public ITransformationProvider CreateProvider(string connectionString)
        {
            var connection = CreateConnectionInternal(connectionString);

            return CreateProviderInternal(connection);
        }
    }
}