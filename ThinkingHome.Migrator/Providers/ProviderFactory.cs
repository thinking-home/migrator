using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
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
            return Create(GetFactoryType(factoryName));
        }

        public static IProviderFactory Create<TFactory>() where TFactory : IProviderFactory
        {
            return Create(typeof(TFactory));
        }

        public static IProviderFactory Create(Type factoryType)
        {
            if (!typeof(IProviderFactory).GetTypeInfo().IsAssignableFrom(factoryType))
            {
                throw new Exception($"The factory class ({factoryType.FullName}) must implement the IProviderFactory interface");
            }

            var factory = Activator.CreateInstance(factoryType) as IProviderFactory;

            if (factory == null) throw new Exception("Could not create a provider factory.");

            return factory;
        }

        #region shortcuts

        private static readonly Dictionary<string, string> shortcuts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "postgres", "ThinkingHome.Migrator.Providers.PostgreSQL.PostgreSQLProviderFactory, ThinkingHome.Migrator.Providers.PostgreSQL" },
            { "sqlserver", "ThinkingHome.Migrator.Providers.SqlServer.SqlServerProviderFactory, ThinkingHome.Migrator.Providers.SqlServer" },
            { "oracle", "ThinkingHome.Migrator.Providers.Oracle.OracleProviderFactory, ThinkingHome.Migrator.Providers.Oracle" },
            { "mysql", "ThinkingHome.Migrator.Providers.MySql.MySqlProviderFactory, ThinkingHome.Migrator.Providers.MySql" },
            { "sqlite",   "ThinkingHome.Migrator.Providers.SQLite.SQLiteProviderFactory, ThinkingHome.Migrator.Providers.SQLite" }
        };

        public static Type GetFactoryType(string factoryName)
        {
            string factoryTypeName = shortcuts.ContainsKey(factoryName)
                ? shortcuts[factoryName]
                : factoryName;

            Type factoryType = Type.GetType(factoryTypeName);

            if (factoryType == null)
            {
                throw new Exception($"Could not load factory class: {factoryName ?? "null"}");
            }

            return factoryType;
        }

        #endregion
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