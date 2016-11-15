using System;

namespace ThinkingHome.Migrator.Framework
{
    /// <summary>
    /// Describe a migration
    /// </summary>
    public class MigrationAttribute : Attribute
    {
        /// <summary>
        /// Describe the migration
        /// </summary>
        /// <param name="version">The unique version of the migration.</param>
        public MigrationAttribute(long version)
        {
            Version = version;
        }

        /// <summary>
        /// The version reflected by the migration
        /// </summary>
        public long Version { get; private set; }

        /// <summary>
        /// Set to <c>true</c> to ignore this migration.
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Execute migration without transaction
        /// </summary>
        public bool WithoutTransaction  { get; set; }
    }
}