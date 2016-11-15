using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ThinkingHome.Migrator.Framework.Extensions
{
    public static class StringUtils
    {
        /// <summary>
        /// Convert a classname to something more readable.
        /// ex.: CreateATable => Create a table
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string ToHumanName(this string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                return string.Empty;
            }

            var name = Regex.Replace(className, "([a-z0-9])([A-Z])", "$1 $2").Replace('_', ' ');
            name = Regex.Replace(name, "\\s+", " ").ToLower();

            return char.ToUpper(name[0]) + name.Substring(1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="template"></param>
        /// <param name="placeholder"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceOnce(this string template, string placeholder, string replacement)
        {
            var location = template.IndexOf(placeholder, StringComparison.Ordinal);
            if (location < 0)
            {
                return template;
            }

            return new StringBuilder(template.Substring(0, location))
                .Append(replacement)
                .Append(template.Substring(location + placeholder.Length))
                .ToString();
        }

        public static SchemaQualifiedObjectName WithSchema(this string name, string schema)
        {
            return new SchemaQualifiedObjectName { Name = name, Schema = schema };
        }

        /// <summary>
        /// Получает строку, состоящую из строковых представлений
        /// элементов коллекции, разделенных заданной строкой-разделителем
        /// </summary>
        /// <typeparam name="T">Тип элементов коллекции</typeparam>
        /// <param name="collection">Коллекция</param>
        /// <param name="separator">Разделитель</param>
        /// <returns>Строковое представление коллекции с использованием заданного разделителя</returns>
        public static string ToSeparatedString<T>(this IEnumerable<T> collection, string separator = ",")
        {
            return string.Join(separator, collection.Select(el => el.ToString()).ToArray());
        }
    }
}