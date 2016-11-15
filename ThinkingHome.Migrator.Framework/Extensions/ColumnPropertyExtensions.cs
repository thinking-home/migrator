namespace ThinkingHome.Migrator.Framework.Extensions
{
    /// <summary>
    /// Useful extensions for ColumnProperty class
    /// </summary>
    public static class ColumnPropertyExtensions
    {
        /// <summary>
        /// Check that target property exists
        /// </summary>
        /// <param name="source"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static bool HasProperty(this ColumnProperty source, ColumnProperty comparison)
        {
            return (source & comparison) == comparison;
        }
    }
}