using System.Data;

namespace ThinkingHome.Migrator.Framework
{
    /// <summary>
    /// Database table column type
    /// </summary>
    public class ColumnType
    {
        public ColumnType(DbType dataType)
        {
            DataType = dataType;
        }

        public ColumnType(DbType dataType, int length)
            : this(dataType)
        {
            Length = length;
        }

        public ColumnType(DbType dataType, int length, int scale)
            : this(dataType, length)
        {
            Scale = scale;
        }

        public DbType DataType { get; set; }

        public int? Length { get; set; }

        public int? Scale { get; set; }

        /// <summary>
        /// Implicit type conversion DbType to ColumnType
        /// </summary>
        public static implicit operator ColumnType(DbType type)
        {
            return new ColumnType(type);
        }

        public override string ToString()
        {
            var length = Length.HasValue && Scale.HasValue
                ? $"({Length}, {Scale})"
                : Length.HasValue ? $"({Length})" : string.Empty;

            return $"{DataType}{length}";
        }
    }
}