using System;

namespace ThinkingHome.Migrator.Framework
{
    /// <summary>
    /// Represents a table column properties.
    /// </summary>
    [Flags]
    public enum ColumnProperty
    {
        None = 0,

        /// <summary>
        /// Null is allowable
        /// </summary>
        Null = 1,

        /// <summary>
        /// Null is not allowable
        /// </summary>
        NotNull = 2,

        /// <summary>
        /// Identity column, autoinc
        /// </summary>
        Identity = 4,

        /// <summary>
        /// Unique Column
        /// </summary>
        Unique = 8,

        /// <summary>
        /// Unsigned Column
        /// </summary>
        Unsigned = 16,

        /// <summary>
        /// Primary Key
        /// </summary>
        PrimaryKey = 32 | NotNull,

        /// <summary>
        /// Primary key. Make the column a PrimaryKey and unsigned
        /// </summary>
        PrimaryKeyWithIdentity = PrimaryKey | Identity
    }
}