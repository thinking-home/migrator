using System;
using System.Collections;
using System.Collections.Generic;

namespace ThinkingHome.Migrator
{
    /// <summary>
    /// План выполнения миграций
    /// </summary>
    public sealed class MigrationPlan : IEnumerable<long>
    {
        /// <summary>
        /// Версии миграций в порядке выполнения
        /// </summary>
        private readonly IEnumerable<long> versions;

        /// <summary>
        /// Начальная версия БД
        /// </summary>
        public long StartVersion { get; private set; }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="versions">Версии миграций в порядке выполнения</param>
        /// <param name="startVersion">Начальная версия БД</param>
        public MigrationPlan(IEnumerable<long> versions, long startVersion)
        {
            if (versions == null) throw new ArgumentNullException(nameof(versions));
            this.versions = versions;
            StartVersion = startVersion;
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<long> GetEnumerator()
        {
            return versions.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}