using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using ThinkingHome.Migrator.Exceptions;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Loader;
using ThinkingHome.Migrator.Logging;
using ThinkingHome.Migrator.Providers;

namespace ThinkingHome.Migrator
{
    /// <summary>
    /// Migrations mediator.
    /// </summary>
    public class Migrator : IDisposable
    {
        /// <summary>
        /// Database transformation provider
        /// </summary>
        public ITransformationProvider Provider { get; }

        /// <summary>
        /// Assembly that contains the migrations
        /// </summary>
        private readonly MigrationAssembly migrationAssembly;

        /// <summary>
        /// Migration assembly ley
        /// </summary>
        private string Key => migrationAssembly.Key;

        #region constructors

        /// <summary>
        /// Creates an instance of migrator
        /// </summary>
        /// <param name="providerFactoryType">Name or alias of provider factory class</param>
        /// <param name="connection">Instance of database connection</param>
        /// <param name="asm">Assembly that contains migrations</param>
        public Migrator(string providerFactoryType, IDbConnection connection, Assembly asm)
            : this(ProviderFactory.Create(providerFactoryType).CreateProvider(connection), asm)
        {
        }

        /// <summary>
        /// �������������
        /// </summary>
        public Migrator(string providerFactoryType, string connectionString, Assembly asm)
            : this(ProviderFactory.Create(providerFactoryType).CreateProvider(connectionString), asm)
        {
        }

        /// <summary>
        /// �������������
        /// </summary>
        public Migrator(ITransformationProvider provider, Assembly asm)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (asm == null) throw new ArgumentNullException(nameof(asm));

            Provider = provider;
            migrationAssembly = new MigrationAssembly(asm);
        }

        #endregion

        /// <summary>
        /// Returns registered migration <see cref="System.Type">types</see>.
        /// </summary>
        public ReadOnlyCollection<MigrationInfo> AvailableMigrations => migrationAssembly.MigrationsTypes;

        /// <summary>
        /// Returns the current migrations applied to the database.
        /// </summary>
        public IList<long> GetAppliedMigrations()
        {
            return Provider.GetAppliedMigrations(Key);
        }

        /// <summary>
        /// Migrate the database to a specific version.
        /// Runs all migration between the actual version and the
        /// specified version.
        /// If <c>version</c> is greater then the current version,
        /// the <c>Apply()</c> method will be invoked.
        /// If <c>version</c> lower then the current version,
        /// the <c>Revert()</c> method of previous migration will be invoked.
        /// If <c>dryrun</c> is set, don't write any changes to the database.
        /// </summary>
        /// <param name="databaseVersion">The version that must became the current one</param>
        public void Migrate(long databaseVersion = -1)
        {

            long targetVersion = databaseVersion < 0 ? migrationAssembly.LatestVersion : databaseVersion;

            IList<long> appliedMigrations = Provider.GetAppliedMigrations(Key);
            IList<long> availableMigrations = migrationAssembly.MigrationsTypes
                .Select(mInfo => mInfo.Version).ToList();

            MigrationPlan plan = BuildMigrationPlan(targetVersion, appliedMigrations, availableMigrations);

            long currentDatabaseVersion = plan.StartVersion;
            MigratorLogManager.Log.Started(currentDatabaseVersion, targetVersion);

            foreach (long currentExecutedVersion in plan)
            {
                ExecuteMigration(currentExecutedVersion, currentDatabaseVersion);

                currentDatabaseVersion = currentExecutedVersion;
            }
        }

        /// <summary>
        /// ���������� ��������
        /// </summary>
        /// <param name="targetVersion">������ ����������� ��������</param>
        /// <param name="currentDatabaseVersion">������� ������ ��</param>
        public void ExecuteMigration(long targetVersion, long currentDatabaseVersion)
        {
            var migrationInfo = migrationAssembly.GetMigrationInfo(targetVersion);

            Migration migration = migrationAssembly.InstantiateMigration(migrationInfo, Provider);

            try
            {
                if (!migrationInfo.WithoutTransaction)
                {
                    Provider.BeginTransaction();
                }

                if (targetVersion <= currentDatabaseVersion)
                {
                    MigratorLogManager.Log.MigrateDown(targetVersion, migration.Name);
                    migration.Revert();
                    Provider.MigrationUnApplied(targetVersion, Key);
                }
                else
                {
                    MigratorLogManager.Log.MigrateUp(targetVersion, migration.Name);
                    migration.Apply();
                    Provider.MigrationApplied(targetVersion, Key);
                }

                if (!migrationInfo.WithoutTransaction)
                {
                    Provider.Commit();
                }
            }
            catch (Exception ex)
            {
                MigratorLogManager.Log.Exception(targetVersion, migration.Name, ex);

                if (!migrationInfo.WithoutTransaction)
                {
                    // ��� ������ ���������� ���������
                    Provider.Rollback();
                    MigratorLogManager.Log.RollingBack(currentDatabaseVersion);
                }

                throw;
            }
        }

        /// <summary>
        /// �������� ������ ������ ��� ����������
        /// </summary>
        /// <param name="target">������ ����������</param>
        /// <param name="appliedMigrations">������ ������ ����������� ��������</param>
        /// <param name="availableMigrations">������ ������ ��������� ��������</param>
        public static MigrationPlan BuildMigrationPlan(long target, IList<long> appliedMigrations, IList<long> availableMigrations)
        {
            long startVersion = appliedMigrations.Any() ? appliedMigrations.Max() : 0;
            var set = new HashSet<long>(appliedMigrations);

            // ��������
            var list = availableMigrations.Where(x => x < startVersion && !set.Contains(x)).ToArray();
            if (list.Any())
            {
                throw new VersionException(
                    "�������� ������������� ��������, ������ ������� ������ ������� ������ ��", list);
            }

            set.UnionWith(availableMigrations);

            var versions = target < startVersion
                ? set.Where(n => n <= startVersion && n > target).OrderByDescending(x => x).ToList()
                : set.Where(n => n > startVersion && n <= target).OrderBy(x => x).ToList();

            return new MigrationPlan(versions, startVersion);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Provider.Dispose();
        }

        #endregion
    }
}