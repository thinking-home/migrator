using System;
using System.Collections.Generic;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Providers
{
    public class ColumnSqlBuilder
    {
        protected readonly List<string> vals = new List<string>();

        protected readonly TypeMap typeMap;
        protected readonly PropertyMap propertyMap;
        protected readonly Column column;
        protected readonly Func<string, string> nameConverter;

        /// <summary>
        ///
        /// </summary>
        /// <param name="column">Параметры колонки таблицы</param>
        /// <param name="typeMap">Мэппинг типов данных</param>
        /// <param name="propertyMap">Мэппинг свойств колонки таблицы</param>
        /// <param name="nameConverter">Конвертер имени столбца таблицы</param>
        public ColumnSqlBuilder(Column column, TypeMap typeMap, PropertyMap propertyMap, Func<string, string> nameConverter)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            if (typeMap == null) throw new ArgumentNullException(nameof(typeMap));
            if (propertyMap == null) throw new ArgumentNullException(nameof(propertyMap));
            if (nameConverter == null) throw new ArgumentNullException(nameof(nameConverter));

            this.column = column;
            this.typeMap = typeMap;
            this.propertyMap = propertyMap;
            this.nameConverter = nameConverter;
        }

        #region добавление элементов SQL-выражения для колонки

        public ColumnSqlBuilder AppendColumnName()
        {
            var columnName = nameConverter(column.Name);
            vals.Add(columnName);

            return this;
        }

        public ColumnSqlBuilder AppendColumnType(bool identityNeedsType)
        {
            string type = column.IsIdentity && !identityNeedsType
                ? string.Empty
                : typeMap.Get(column.ColumnType);

            if (!string.IsNullOrEmpty(type))
            {
                vals.Add(type);
            }

            return this;
        }

        public ColumnSqlBuilder AppendSqlForIdentityWhichNotNeedsType(bool identityNeedsType)
        {
            if (!identityNeedsType)
            {
                propertyMap.AddValueIfSelected(column, ColumnProperty.Identity, vals);
            }

            return this;
        }

        public ColumnSqlBuilder AppendUnsignedSql()
        {
            propertyMap.AddValueIfSelected(column, ColumnProperty.Unsigned, vals);

            return this;
        }

        public ColumnSqlBuilder AppendNotNullSql(bool needsNotNullForIdentity)
        {
            if (!column.ColumnProperty.HasProperty(ColumnProperty.PrimaryKey) || needsNotNullForIdentity)
            {
                propertyMap.AddValueIfSelected(column, ColumnProperty.NotNull, vals);
            }

            return this;
        }

        public ColumnSqlBuilder AppendPrimaryKeySql(bool compoundPrimaryKey)
        {
            if (!compoundPrimaryKey)
            {
                propertyMap.AddValueIfSelected(column, ColumnProperty.PrimaryKey, vals);
            }

            return this;
        }

        public ColumnSqlBuilder AppendSqlForIdentityWhichNeedsType(bool identityNeedsType)
        {
            if (identityNeedsType)
            {
                propertyMap.AddValueIfSelected(column, ColumnProperty.Identity, vals);
            }

            return this;
        }

        public ColumnSqlBuilder AppendUniqueSql()
        {
            propertyMap.AddValueIfSelected(column, ColumnProperty.Unique, vals);

            return this;
        }

        public ColumnSqlBuilder AppendDefaultValueSql(Func<object, string> defaultValueMapper)
        {
            if (column.DefaultValue != null)
            {
                string defaultValueSql = defaultValueMapper(column.DefaultValue);

                vals.Add(defaultValueSql);
            }

            return this;
        }

        public ColumnSqlBuilder AppendRawSql(string sql)
        {
            vals.Add(sql);

            return this;
        }

        #endregion

        public ColumnSqlBuilder Clear()
        {
            vals.Clear();

            return this;
        }

        public override string ToString()
        {
            string columnSql = string.Join(" ", vals.ToArray());

            return columnSql;
        }
    }
}