namespace ThinkingHome.Migrator.Framework
{
    /// <summary>
    /// A migration is a group of transformation applied to the database schema
    /// (or sometimes data) to port the database from one version to another.
    /// The <c>Apply()</c> method must apply the modifications (eg.: create a table)
    /// and the <c>Revert()</c> method must revert, or rollback the modifications
    /// (eg.: delete a table).
    /// <para>
    /// Each migration must be decorated with the <c>[Migration(0)]</c> attribute.
    /// Each migration number (0) must be unique, or else a
    /// <c>DuplicatedVersionException</c> will be trown.
    /// </para>
    /// <para>
    /// All migrations are executed inside a transaction. If an exception is
    /// thrown, the transaction will be rolledback and transformations wont be
    /// applied.
    /// </para>
    /// <para>
    /// It is best to keep a limited number of transformation inside a migration
    /// so you can easely move from one version of to another with fine grain
    /// modifications.
    /// You should give meaningful name to the migration class and prepend the
    /// migration number to the filename so they keep ordered, eg.:
    /// <c>002_CreateTableTest.cs</c>.
    /// </para>
    /// <para>
    /// Use the <c>Database</c> property to apply transformation and the
    /// <c>Logger</c> property to output informations in the console (or other).
    /// For more details on transformations see
    /// <see cref="ITransformationProvider">ITransformationProvider</see>.
    /// </para>
    /// </summary>
    /// <example>
    /// The following migration creates a new Customer table.
    /// (File <c>003_AddCustomerTable.cs</c>)
    /// <code>
    /// [Migration(3)]
    /// public class AddCustomerTable : Migration
    /// {
    /// 	public override void Apply()
    /// 	{
    /// 		Database.AddTable("Customer",
    ///		                  new Column("Name", typeof(string), 50),
    ///		                  new Column("Address", typeof(string), 100)
    ///		                 );
    /// 	}
    /// 	public override void Revert()
    /// 	{
    /// 		Database.RemoveTable("Customer");
    /// 	}
    /// }
    /// </code>
    /// </example>
    ///
    public abstract class Migration : IMigration
    {
        /// <summary>
        /// Migration name
        /// </summary>
        public virtual string Name => StringUtils.ToHumanName(GetType().Name);

        /// <summary>
        /// Defines tranformations to port the database to the current version.
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Defines transformations to revert things done in <c>Apply</c>.
        /// </summary>
        public virtual void Revert()
        {
        }

        /// <summary>
        /// Represents the database.
        /// <see cref="ITransformationProvider"></see>.
        /// </summary>
        /// <seealso cref="ITransformationProvider">Migration.Framework.ITransformationProvider</seealso>
        public ITransformationProvider Database { get; set; }
    }
}