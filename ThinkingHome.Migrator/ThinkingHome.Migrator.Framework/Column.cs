using System;

namespace ThinkingHome.Migrator.Framework
{
    /// <summary>
    /// Represents a table column.
    /// </summary>
    public class Column : IColumn
    {
        #region constructors

        public Column(string name, ColumnType type, ColumnProperty property = ColumnProperty.None, object defaultValue = null)
        {

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Name = name;
            ColumnType = type;
            ColumnProperty = property;
            DefaultValue = defaultValue;
        }

        #endregion

        #region properties

        public string Name { get; set; }

        public void SetColumnType(ColumnType type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            ColumnType = type;
        }

        public ColumnType ColumnType { get; protected set; }

        public ColumnProperty ColumnProperty { get; set; }

        public object DefaultValue { get; set; }

        public bool IsIdentity => ColumnProperty.HasProperty(ColumnProperty.Identity);

        public bool IsPrimaryKey  => ColumnProperty.HasProperty(ColumnProperty.PrimaryKey);

        #endregion
    }
}