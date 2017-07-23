using System.Collections.Generic;
using System.Linq;
using Xunit;
using ThinkingHome.Migrator.Exceptions;

namespace ThinkingHome.Migrator.Tests.Core
{
    /// <summary>
    /// Тестирование построения плана выполнения миграций
    /// </summary>
    public class MigratorPlanTest
    {
        /// <summary>
        /// Проверка, что в нормальных условиях корректно формируется план для движения назад
        /// </summary>
        [Fact]
        public void CanBuildCorrectMigrationPlanDown()
        {
            var applied = new List<long> { 1, 2, 3 };
            var available = new List<long> { 1, 2, 3, 4, 77, 88 };

            var res = Migrator.BuildMigrationPlan(1, applied, available);
            Assert.Equal(2, res.Count());

            Assert.Equal(3, res.ElementAt(0));
            Assert.Equal(2, res.ElementAt(1));
        }

        /// <summary>
        /// Проверка, что в нормальных условиях корректно формируется план для движения назад
        /// </summary>
        [Fact]
        public void CanBuildCorrectMigrationPlanUp()
        {
            var applied = new List<long> { 1, 2, 3 };
            var available = new List<long> { 1, 2, 3, 4, 77, 88 };

            var res = Migrator.BuildMigrationPlan(88, applied, available);
            Assert.Equal(3, res.Count());

            Assert.Equal(4, res.ElementAt(0));
            Assert.Equal(77, res.ElementAt(1));
            Assert.Equal(88, res.ElementAt(2));
        }

        /// <summary>
        /// Проверка, что план формируется корректно для несуществующих версий (идем вверх)
        /// </summary>
        [Fact]
        public void CanBuildPlanForNotExistsVersionUp()
        {
            var applied = new List<long> { 5, 10 };
            var available = new List<long> { 5, 10, 15, 20 };

            var res = Migrator.BuildMigrationPlan(12, applied, available);
            Assert.Equal(0, res.Count());

            var res2 = Migrator.BuildMigrationPlan(17, applied, available);
            Assert.Equal(1, res2.Count());
            Assert.Equal(15, res2.ElementAt(0));

            var res3 = Migrator.BuildMigrationPlan(23, applied, available);
            Assert.Equal(2, res3.Count());
            Assert.Equal(15, res3.ElementAt(0));
            Assert.Equal(20, res3.ElementAt(1));
        }

        /// <summary>
        /// Проверка, что план формируется корректно для несуществующих версий (идем вниз)
        /// </summary>
        [Fact]
        public void CanBuildPlanForNotExistsVersionDown()
        {
            var applied = new List<long> { 5, 10, 15 };
            var available = new List<long> { 5, 10, 15, 20 };

            var res = Migrator.BuildMigrationPlan(12, applied, available);
            Assert.Equal(1, res.Count());
            Assert.Equal(15, res.ElementAt(0));

            var res2 = Migrator.BuildMigrationPlan(7, applied, available);
            Assert.Equal(2, res2.Count());
            Assert.Equal(15, res2.ElementAt(0));
            Assert.Equal(10, res2.ElementAt(1));
        }

        /// <summary>
        /// Построение плана для текущей версии
        /// </summary>
        [Fact]
        public void CanBuildCorrectMigrationPlanForCurrentVersion()
        {
            var applied = new List<long> { 1, 2, 3 };
            var available = new List<long> { 1, 2, 3, 4, 77, 88 };

            var res = Migrator.BuildMigrationPlan(3, applied, available);
            Assert.Equal(0, res.Count());
        }

        /// <summary>
        /// Отсутствие классов миграций для выполненных версий
        /// </summary>
        [Fact]
        public void CanBuildPlanForVersionsHasntMigrationClass()
        {
            var applied = new List<long> { 1, 2, 3, 4 };
            var available = new List<long> { 1, 2, 4, 77, 88 };

            var res = Migrator.BuildMigrationPlan(1, applied, available);
            Assert.Equal(3, res.Count());
            Assert.Equal(4, res.ElementAt(0));
            Assert.Equal(3, res.ElementAt(1));
            Assert.Equal(2, res.ElementAt(2));
        }

        /// <summary>
        /// Проверка, что при невыполненных миграциях ниже текущей версии генерируется исключение
        /// </summary>
        [Fact]
        public void ShouldThrowExceptionIfExistsNotAppliedMigrationLessThatCurrent()
        {
            var applied = new List<long> { 1, 4 };
            var available = new List<long> { 1, 2, 3, 4, 5 };

            var ex = Assert.Throws<VersionException>(() =>
                    Migrator.BuildMigrationPlan(5, applied, available));

            Assert.Equal(2, ex.Versions.Length);
            Assert.True(ex.Versions.Contains(3));
            Assert.True(ex.Versions.Contains(2));
        }
    }
}