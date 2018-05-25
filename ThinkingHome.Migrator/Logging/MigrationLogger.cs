using System;
using System.Text;
using Microsoft.Extensions.Logging;
using ThinkingHome.Migrator.Framework.Extensions;
using ThinkingHome.Migrator.Loader;

namespace ThinkingHome.Migrator.Logging
{
    /// <summary>
    /// Методы расширения для объектов, реализующих интерфейс ILogger
    /// </summary>
    public class MigrationLogger
    {
        private readonly ILogger logger;

        public MigrationLogger(ILogger logger)
        {
            this.logger = logger;
        }

        #region basic

        public void Trace(string msg)
        {
            logger?.LogTrace(msg);
        }

        public void Info(string msg)
        {
            logger?.LogInformation(msg);
        }

        public void Warn(string msg)
        {
            logger?.LogWarning(msg);
        }

        public void Error(string msg, Exception ex)
        {
            logger?.LogError(new EventId(), ex, msg);
        }

        #endregion

        #region custom

        /// <summary>
        /// Запись в лог о начале выполнения серии миграций
        /// </summary>
        /// <param name="currentVersion">Текущая версия БД</param>
        /// <param name="finalVersion">Новая версия БД</param>
        public void Started(long currentVersion, long finalVersion)
        {
            Info($"Latest version applied : {currentVersion}.  Target version : {finalVersion}");
        }

        /// <summary>
        /// Запись о выполнении миграции
        /// </summary>
        /// <param name="version">Версия миграции</param>
        /// <param name="migrationName">Название миграции</param>
        public void MigrateUp(long version, string migrationName)
        {
            Info($"Applying {version}: {migrationName}");
        }

        /// <summary>
        /// Запись об откате миграции
        /// </summary>
        /// <param name="version">Версия миграции</param>
        /// <param name="migrationName">Название миграции</param>
        public void MigrateDown(long version, string migrationName)
        {
            Info($"Removing {version}: {migrationName}");
        }

        /// <summary>
        /// Запись о пропущенной миграции
        /// </summary>
        /// <param name="version">Версия миграции</param>
        public void Skipping(long version)
        {
            Info($"{version} <Migration not found>");
        }

        /// <summary>
        /// Запись об откате изменений миграции во время выполнения
        /// </summary>
        /// <param name="originalVersion">Версия БД, к которой производится откат</param>
        public void RollingBack(long originalVersion)
        {
            Info($"Rolling back to migration {originalVersion}");
        }

        /// <summary>
        /// Запись о выполнении SQL-запроса
        /// </summary>
        /// <param name="sql">Текст SQL запроса</param>
        public void ExecuteSql(string sql)
        {
            Trace(sql);
        }

        /// <summary>
        /// Запись об ошибке
        /// </summary>
        /// <param name="version">Версия миграции, в которой произощла ошибка</param>
        /// <param name="migrationName">Название миграции</param>
        /// <param name="ex">Исключение</param>
        public void Exception(long version, string migrationName, Exception ex)
        {
            Exception($"Error in migration: {version}", ex);
        }

        /// <summary>
        /// Запись об ошибке
        /// </summary>
        /// <param name="message">Сообщение об ошибке</param>
        /// <param name="ex">Исключение</param>
        public void Exception(string message, Exception ex)
        {
            var msg = message;

            for (var current = ex; current != null; current = current.InnerException)
            {
                Error(msg, current);
                msg = "Inner exception:";
            }
        }

        /// <summary>
        /// Запись об окончании выполнения серии миграций
        /// </summary>
        /// <param name="originalVersion">Начальная версия БД</param>
        /// <param name="currentVersion">Конечная версия БД</param>
        public void Finished(long originalVersion, long currentVersion)
        {
            Info($"Migrated to version {currentVersion}");
        }

        /// <summary>
        /// Записывает в лог список миграций
        /// </summary>
        /// <param name="asm">Сборка с миграциями</param>
        public void WriteMigrationAssemblyInfo(MigrationAssembly asm)
        {
            Info($"Migration key: {asm.Key}");

            var msg = new StringBuilder("Loaded migrations:").AppendLine();

            foreach (var mi in asm.MigrationsTypes)
            {
                msg.AppendLine($"{mi.Version.ToString().PadLeft(5)} {mi.Type.Name.ToHumanName()}");
            }

            Info(msg.ToString());
        }

        #endregion
    }
}