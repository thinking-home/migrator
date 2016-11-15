using NLog;
using NLog.Config;
using NLog.Targets;

namespace ThinkingHome.Migrator.Logging
{
    /// <summary>
    /// Логирование
    /// </summary>
    public static class MigratorLogManager
    {
        public const string LOGGER_NAME = "ecm7-migrator-logger";

        public static Logger Log { get; } = LogManager.GetLogger(LOGGER_NAME);

        public static void SetNLogTarget(Target target, LogLevel minLevel = null)
        {
            if (target != null)
            {
                SimpleConfigurator.ConfigureForTargetLogging(target, minLevel ?? LogLevel.Info);
            }
        }
    }
}