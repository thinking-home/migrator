using System;

namespace ThinkingHome.Migrator.Framework
{
    /// <summary>
    /// Describe a migration assembly
    /// </summary>
    public class MigrationAssemblyAttribute : Attribute
    {
        /// <summary>
        /// Describe the migration assembly
        /// </summary>
        /// <param name="key">Key of the migration.</param>
        public MigrationAssemblyAttribute(string key = null)
        {
            Key = key;
        }

        /// <summary>
        /// The key of the migration assembly
        /// </summary>
        public string Key { get; }
    }
}