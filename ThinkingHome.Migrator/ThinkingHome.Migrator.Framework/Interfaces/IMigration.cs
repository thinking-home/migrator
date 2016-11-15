namespace ThinkingHome.Migrator.Framework.Interfaces
{
    /// <summary>
    /// Migration interface
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// Migration name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Represents the database.
        /// <see cref="ITransformationProvider"></see>.
        /// </summary>
        /// <seealso cref="ITransformationProvider">ThinkingHome.Migrator.Framework.ITransformationProvider</seealso>
        ITransformationProvider Database { get; set; }

        /// <summary>
        /// Defines tranformations to port the database to the current version.
        /// </summary>
        void Apply();

        /// <summary>
        /// Defines transformations to revert things done in <c>Apply</c>.
        /// </summary>
        void Revert();
    }
}