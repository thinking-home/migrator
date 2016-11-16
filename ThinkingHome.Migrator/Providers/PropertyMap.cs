using System.Collections.Generic;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Providers
{
    public class PropertyMap : Dictionary<ColumnProperty, string>
    {
        public void RegisterPropertySql(ColumnProperty property, string sql)
        {
            this[property] = sql;
        }

        public string SqlForProperty(ColumnProperty property)
        {
            return ContainsKey(property) ? this[property] : string.Empty;
        }

        public void AddValueIfSelected(Column column, ColumnProperty property, ICollection<string> vals)
        {
            if (column.ColumnProperty.HasProperty(property))
            {
                vals.Add(SqlForProperty(property));
            }
        }
    }
}