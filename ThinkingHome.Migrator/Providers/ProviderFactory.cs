using System;
using System.Data;
using Microsoft.Extensions.Logging;
using ThinkingHome.Migrator.Framework.Interfaces;

namespace ThinkingHome.Migrator.Providers
{
    public interface IProviderFactory
    {
        ITransformationProvider CreateProvider(string connectionString, ILogger logger);
        ITransformationProvider CreateProvider(IDbConnection connection, ILogger logger);
        IDbConnection CreateConnection(string connectionString);
    }

    public static class ProviderFactory
    {
        public static IProviderFactory Create(string factoryName)
        {
            throw new NotImplementedException();
        }

        public static IProviderFactory Create<TFactory>() where TFactory : IProviderFactory
        {
            return Create(typeof(TFactory));
        }

        public static IProviderFactory Create(Type factoryType)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class ProviderFactory<TProvider, TConnection> : IProviderFactory
        where TProvider : TransformationProvider<TConnection>
        where TConnection : IDbConnection
    {
        protected abstract TProvider CreateProviderInternal(TConnection connection, ILogger logger);
        protected abstract TConnection CreateConnectionInternal(string connectionString);

        public IDbConnection CreateConnection(string connectionString)
        {
            return CreateConnectionInternal(connectionString);
        }

        public ITransformationProvider CreateProvider(IDbConnection connection, ILogger logger)
        {
            if (!(connection is TConnection)) throw new InvalidCastException();

            return CreateProviderInternal((TConnection) connection, logger);
        }

        public ITransformationProvider CreateProvider(string connectionString, ILogger logger)
        {
            var connection = CreateConnectionInternal(connectionString);

            return CreateProviderInternal(connection, logger);
        }
    }
}