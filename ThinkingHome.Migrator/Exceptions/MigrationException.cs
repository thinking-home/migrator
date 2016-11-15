using System;

namespace ThinkingHome.Migrator.Exceptions
{
    /// <summary>
    /// Base class for migration errors.
    /// </summary>
    public class MigrationException : Exception
    {
        public MigrationException(string message)
            : base(message) {}

        public MigrationException(string message, Exception cause)
            : base(message, cause) {}

        public MigrationException(string migration, int version, Exception innerException)
            : base($"Exception in migration {migration} (#{version})", innerException) {}
    }
}