using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Exceptions
{
    /// <summary>
    /// Exception thrown when a migration number is not unique.
    /// </summary>
    public class DuplicatedVersionException : VersionException
    {
        /// <param name="versions">Duplicated version</param>
        public DuplicatedVersionException(params long[] versions)
            : base($"Migration version #{versions.ToSeparatedString()} is duplicated")
        {
        }
    }
}