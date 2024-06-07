using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using Xunit;
using ThinkingHome.Migrator.Exceptions;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Loader;
using ThinkingHome.Migrator.Tests.TestMigrations;

namespace ThinkingHome.Migrator.Tests.Core
{
    public class MigrationAssemblyTest
    {
        [Fact]
        public void CanLoadMigrationsWithKey()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            var migrationAssembly = new MigrationAssembly(assembly);

            IList<long> list = migrationAssembly.MigrationsTypes.Select(x => x.Version).ToList();

            Assert.Equal("test-key111", migrationAssembly.Key);

            Assert.Equal(3, list.Count);
            Assert.Contains(1, list);
            Assert.Contains(2, list);
            Assert.Contains(4, list);
        }

        /// <summary>
        /// ��������, ��� ��� ���������� �������� � �������� ������� ������������ ����������
        /// </summary>
        [Fact]
        public void ThrowIfNoMigrationForVersion()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);

            Assert.Throws<Exception>(() => migrationAssembly.GetMigrationInfo(99999999));
        }

        /// <summary>
        /// �������� ��������� ����������, ���� �� ������ ��������� ����
        /// </summary>
        [Fact]
        public void ForNullProviderShouldThrowException()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            var loader = new MigrationAssembly(assembly);

            var mi = loader.GetMigrationInfo(1);
            Assert.Throws<ArgumentNullException>(() => loader.InstantiateMigration(mi, null));
        }


        /// <summary>
        /// �������� ������������ ����������� ��������� ��������� ������
        /// </summary>
        [Fact]
        public void LastVersion()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
            Assert.Equal(4, migrationAssembly.LatestVersion);
        }

        /// <summary>
        /// ��������, ��� ��� ���������� �������� ��������� ��������� ������ == 0
        /// (��������� �� ��������������� �����)
        /// </summary>
        [Fact]
        public void LaseVersionIsZeroIfNoMigrations()
        {
            Assembly assembly =
                typeof(Migration).GetTypeInfo().Assembly; // ��������� ������� ������ - � ��� ��� ��������
            MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
            Assert.Equal(0, migrationAssembly.LatestVersion);
        }

        /// <summary>
        /// �������� ����������� �� ���������� ������� ������
        /// </summary>
        [Fact]
        public void CheckForDuplicatedVersion()
        {
            var versions = new long[] {1, 2, 3, 4, 2, 4};

            var ex = Assert.Throws<DuplicatedVersionException>(() =>
                MigrationAssembly.CheckForDuplicatedVersion(versions));

            Assert.Equal(2, ex.Versions.Length);
            Assert.True(ex.Versions.Contains(2));
            Assert.True(ex.Versions.Contains(4));
        }

        /// <summary>
        /// �������� �������� ������� �������� �� ������ ������
        /// </summary>
        [Fact]
        public void CanCreateMigrationObject()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);

            Mock<ITransformationProvider> provider = new Mock<ITransformationProvider>();

            var mi = migrationAssembly.GetMigrationInfo(2);
            Migration migration = migrationAssembly.InstantiateMigration(mi, provider.Object);

            Assert.NotNull(migration);
            Assert.True(migration is TestMigration02);
            Assert.Same(provider.Object, migration.Database);
        }

        [Fact]
        public void MigrationsMustBeSortedByNumber()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            var asm = new MigrationAssembly(assembly);

            var expected = new[] {1, 2, 4};

            Assert.Equal(expected.Length, asm.MigrationsTypes.Count);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], asm.MigrationsTypes[i].Version);
            }
        }
    }
}
