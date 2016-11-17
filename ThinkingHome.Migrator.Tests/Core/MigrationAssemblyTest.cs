using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;
using ThinkingHome.Migrator.Exceptions;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Interfaces;
using ThinkingHome.Migrator.Loader;
using ThinkingHome.Migrator.Logging;
using ThinkingHome.Migrator.Tests.TestMigrations;

namespace ThinkingHome.Migrator.Tests.Core
{
    [TestFixture]
    public class MigrationAssemblyTest
    {
        [Test]
        public void CanLoadMigrationsWithKey()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            var migrationAssembly = new MigrationAssembly(assembly);

            IList<long> list = migrationAssembly.MigrationsTypes.Select(x => x.Version).ToList();

            Assert.AreEqual("test-key111", migrationAssembly.Key);

            Assert.AreEqual(3, list.Count);
            Assert.IsTrue(list.Contains(1));
            Assert.IsTrue(list.Contains(2));
            Assert.IsTrue(list.Contains(4));
        }

        /// <summary>
        /// ��������, ��� ��� ���������� �������� � �������� ������� ������������ ����������
        /// </summary>
        [Test]
        public void ThrowIfNoMigrationForVersion()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);

            Assert.Throws<Exception>(() => migrationAssembly.GetMigrationInfo(99999999));
        }

        /// <summary>
        /// �������� ��������� ����������, ���� �� ������ ��������� ����
        /// </summary>
        [Test]
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
        [Test]
        public void LastVersion()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
            Assert.AreEqual(4, migrationAssembly.LatestVersion);
        }

        /// <summary>
        /// ��������, ��� ��� ���������� �������� ��������� ��������� ������ == 0
        /// (��������� �� ��������������� �����)
        /// </summary>
        [Test]
        public void LaseVersionIsZeroIfNoMigrations()
        {
            Assembly assembly = typeof(Migration).GetTypeInfo().Assembly; // ��������� ������� ������ - � ��� ��� ��������
            MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);
            Assert.AreEqual(0, migrationAssembly.LatestVersion);
        }

        /// <summary>
        /// �������� ����������� �� ���������� ������� ������
        /// </summary>
        [Test]
        public void CheckForDuplicatedVersion()
        {
            var versions = new long[] { 1, 2, 3, 4, 2, 4 };

            var ex = Assert.Throws<DuplicatedVersionException>(() =>
                    MigrationAssembly.CheckForDuplicatedVersion(versions));

            Assert.AreEqual(2, ex.Versions.Length);
            Assert.That(ex.Versions.Contains(2));
            Assert.That(ex.Versions.Contains(4));
        }

        /// <summary>
        /// �������� �������� ������� �������� �� ������ ������
        /// </summary>
        [Test]
        public void CanCreateMigrationObject()
        {
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            MigrationAssembly migrationAssembly = new MigrationAssembly(assembly);

            Mock<ITransformationProvider> provider = new Mock<ITransformationProvider>();

            var mi = migrationAssembly.GetMigrationInfo(2);
            Migration migration = migrationAssembly.InstantiateMigration(mi, provider.Object);

            Assert.IsNotNull(migration);
            Assert.That(migration is TestMigration02);
            Assert.AreSame(provider.Object, migration.Database);
        }

        [Test]
        public void MigrationsMustBeSortedByNumber()
        {
            var target = new MemoryTarget { Name = MigratorLogManager.LOGGER_NAME, Layout = new SimpleLayout("${message}") };
            MigratorLogManager.SetNLogTarget(target);

            Assembly assembly = GetType().GetTypeInfo().Assembly;
            var asm = new MigrationAssembly(assembly);

            var list = target.Logs
                .Where(str => str.StartsWith("Loaded migrations:"))
                .ToList();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Loaded migrations:\n    1 Test migration01\n    2 Test migration02\n    4 Test migration04\n", list[0]);
        }
    }
}
