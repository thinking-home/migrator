using System.Collections.Generic;

namespace ThinkingHome.Migrator.Loader
{
    /// <summary>
    /// Comparer of Migration by their version attribute.
    /// </summary>
    public class MigrationInfoComparer : IComparer<MigrationInfo>
    {
        /// <summary>
        /// Sort order
        /// </summary>
        private readonly bool ascending;

        /// <param name="ascending">Sort order (true = ascending order, false = decreasing order)</param>
        public MigrationInfoComparer(bool ascending = true)
        {
            this.ascending = ascending;
        }

        /// <summary>
        /// Compare two migrations
        /// </summary>
        public int Compare(MigrationInfo x, MigrationInfo y)
        {
            return ascending
                ? x.Version.CompareTo(y.Version)
                : y.Version.CompareTo(x.Version);
        }
    }
}