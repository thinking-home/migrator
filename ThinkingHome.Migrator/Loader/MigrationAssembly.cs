using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ThinkingHome.Migrator.Exceptions;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;

namespace ThinkingHome.Migrator.Loader
{
    public class MigrationAssembly
    {
        /// <summary>
        /// Returns registered migration <see cref="System.Type">types</see>.
        /// </summary>
        public ReadOnlyCollection<MigrationInfo> MigrationsTypes { get; }

        /// <summary>
        /// Assembly key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Latest available version
        /// </summary>
        public long LatestVersion { get; }

        /// <param name="asm">Assembly with migrations</param>
        public MigrationAssembly(Assembly asm)
        {
            if (asm == null) throw new ArgumentNullException(nameof(asm));

            Key = GetAssemblyKey(asm);

            var mt = GetMigrationInfoList(asm);
            var versions = mt.Select(info => info.Version).ToArray();

            CheckForDuplicatedVersion(versions);
            MigrationsTypes = new ReadOnlyCollection<MigrationInfo>(mt);

            LatestVersion = versions.Any() ? versions.Max() : 0;
        }

        /// <summary>
        /// Discovers the migration assembly key
        /// </summary>
        private static string GetAssemblyKey(Assembly assembly)
        {
            var asmAttribute = assembly.GetCustomAttribute<MigrationAssemblyAttribute>();

            var assemblyKey = asmAttribute?.Key ?? assembly.FullName;

            return assemblyKey;
        }

        /// <summary>
        /// Collect migrations in one <c>Assembly</c>.
        /// </summary>
        /// <param name="asm">The <c>Assembly</c> to browse.</param>
        private static List<MigrationInfo> GetMigrationInfoList(Assembly asm)
        {
            var migrations = new List<MigrationInfo>();

            foreach (var type in asm.GetExportedTypes())
            {
                var attribute = type.GetTypeInfo().GetCustomAttribute<MigrationAttribute>();

                if (attribute == null || !typeof(Migration).GetTypeInfo().IsAssignableFrom(type) || attribute.Ignore) continue;

                migrations.Add(new MigrationInfo(type));
            }

            migrations.Sort(new MigrationInfoComparer());

            return migrations;
        }

        /// <summary>
        /// Check for duplicated version in migrations.
        /// </summary>
        /// <exception cref="CheckForDuplicatedVersion">CheckForDuplicatedVersion</exception>
        public static void CheckForDuplicatedVersion(IEnumerable<long> migrationsTypes)
        {
            var duplicatedVersions = migrationsTypes
                .GroupBy(v => v)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToArray();

            if (duplicatedVersions.Any())
            {
                throw new DuplicatedVersionException(duplicatedVersions);
            }
        }

        public MigrationInfo GetMigrationInfo(long version)
        {
            var targetMigrationInfo = MigrationsTypes
                .Where(info => info.Version == version)
                .ToList();

            if (!targetMigrationInfo.Any()) throw new Exception($"Migration not found: {version}");

            return targetMigrationInfo.First();
        }

        /// <summary>
        /// Creates the instance of migration class by MigrationInfo
        /// </summary>
        /// <param name="migrationInfo">Information about the migration</param>
        /// <param name="provider">Database transformation provider</param>
        public Migration InstantiateMigration(MigrationInfo migrationInfo, ITransformationProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            var migration = (Migration)Activator.CreateInstance(migrationInfo.Type);
            migration.Database = provider;
            return migration;
        }
    }
}