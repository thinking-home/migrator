using System;
using System.Linq;
using System.Collections.Generic;

namespace ThinkingHome.Migrator.Exceptions
{
    /// <summary>
    /// Исключение, генерируемое при наличии некорректных версий
    /// </summary>
    public class VersionException : Exception
    {
        /// <summary>
        /// Список некорректных версий
        /// </summary>
        public long[] Versions { get; }

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="invalidVersions">Список некорректных версий</param>
        public VersionException(string message = null, params long[] invalidVersions)
            : base(message)
        {
            Versions = invalidVersions;
        }
    }
}