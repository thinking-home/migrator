using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using ThinkingHome.Migrator.Exceptions;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Loader;
using ThinkingHome.Migrator.Logging;

namespace ThinkingHome.Migrator
{
    /*
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
        /// �������������
        /// </summary>
        public Migrator(string providerTypeName, IDbConnection connection, Assembly asm)
            : this(ProviderFactory.Create(providerTypeName, connection), asm)
        {
        }

        /// <summary>
        /// �������������
        /// </summary>
        public Migrator(string providerTypeName, string connectionString, Assembly asm)
            : this(ProviderFactory.Create(providerTypeName, connectionString), asm)
        {
        }

        /// <summary>
        /// �������������
        /// </summary>
        public Migrator(ITransformationProvider provider, Assembly asm)
        {
            Require.IsNotNull(provider, "�� ����� ��������� �������������");
            this.provider = provider;

            Require.IsNotNull(asm, "�� ������ ������ � ����������");
            migrationAssembly = new MigrationAssembly(asm);
        }

        #endregion

        /// <summary>
        /// Returns registered migration <see cref="System.Type">types</see>.
        /// </summary>
        public ReadOnlyCollection<MigrationInfo> AvailableMigrations
        {
            get { return migrationAssembly.MigrationsTypes; }
        }

        /// <summary>
        /// Returns the current migrations applied to the database.
        /// </summary>
        public IList<long> GetAppliedMigrations()
        {
            return provider.GetAppliedMigrations(Key);
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

            long targetVersion = databaseVersion < 0 ? migrationAssembly.LastVersion : databaseVersion;

            IList<long> appliedMigrations = provider.GetAppliedMigrations(Key);
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

            IMigration migration = migrationAssembly.InstantiateMigration(migrationInfo, provider);

            try
            {
                if (!migrationInfo.WithoutTransaction)
                {
                    provider.BeginTransaction();
                }

                if (targetVersion <= currentDatabaseVersion)
                {
                    MigratorLogManager.Log.MigrateDown(targetVersion, migration.Name);
                    migration.Revert();
                    provider.MigrationUnApplied(targetVersion, Key);
                }
                else
                {
                    MigratorLogManager.Log.MigrateUp(targetVersion, migration.Name);
                    migration.Apply();
                    provider.MigrationApplied(targetVersion, Key);
                }

                if (!migrationInfo.WithoutTransaction)
                {
                    provider.Commit();
                }
            }
            catch (Exception ex)
            {
                MigratorLogManager.Log.Exception(targetVersion, migration.Name, ex);

                if (!migrationInfo.WithoutTransaction)
                {
                    // ��� ������ ���������� ���������
                    provider.Rollback();
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
            long startVersion = appliedMigrations.IsEmpty() ? 0 : appliedMigrations.Max();
            var set = new HashSet<long>(appliedMigrations);

            // ��������
            var list = availableMigrations.Where(x => x < startVersion && !set.Contains(x)).ToList();
            if (!list.IsEmpty())
            {
                throw new VersionException(
                    "�������� ������������� ��������, ������ ������� ������ ������� ������ ��", list.ToArray());
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
            provider.Dispose();
        }

        #endregion
    }

    */
}