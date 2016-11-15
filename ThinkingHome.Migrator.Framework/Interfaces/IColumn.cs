namespace ThinkingHome.Migrator.Framework.Interfaces
{
    public interface IColumn
    {
        ColumnProperty ColumnProperty { get; set; }

        string Name { get; set; }

        void SetColumnType(ColumnType type);

        ColumnType ColumnType { get; }

        bool IsIdentity { get; }

        bool IsPrimaryKey { get; }

        object DefaultValue { get; set; }
    }
}