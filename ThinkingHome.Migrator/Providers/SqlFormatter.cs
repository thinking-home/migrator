using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Providers
{
    /// <summary>
    /// Поддержка форматов "NAME" и "COLS" для экранирования идентификаторов
    /// </summary>
    public class SqlFormatter : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// Функция, выполняющая экранирование идентификаторов
        /// </summary>
        protected readonly Func<object, string> converter;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="converter">Функция, выполняющая экранирование идентификаторов</param>
        public SqlFormatter(Func<object, string> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            this.converter = converter;
        }

        /// <summary>
        /// Returns an object that provides formatting services for the specified type.
        /// </summary>
        /// <returns>
        /// An instance of the object specified by <paramref name="formatType"/>, if the <see cref="T:System.IFormatProvider"/> implementation can supply that type of object; otherwise, null.
        /// </returns>
        /// <param name="formatType">An object that specifies the type of format object to return. </param><filterpriority>1</filterpriority>
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        /// <summary>
        /// Converts the value of a specified object to an equivalent string representation using specified format and culture-specific formatting information.
        /// </summary>
        /// <returns>
        /// The string representation of the value of <paramref name="arg"/>, formatted as specified by <paramref name="format"/> and <paramref name="formatProvider"/>.
        /// </returns>
        /// <param name="format">A format string containing formatting specifications. </param><param name="arg">An object to format. </param><param name="formatProvider">An object that supplies format information about the current instance. </param><filterpriority>2</filterpriority>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            string ufmt = (format ?? string.Empty).ToUpper();

            try
            {
                if (arg is IEnumerable<object> && ufmt == "COLS")
                {
                    var collection = (IEnumerable<object>) arg;
                    return collection.Select(converter).ToSeparatedString();
                }

                if (ufmt == "NAME")
                {
                    if (arg is SchemaQualifiedObjectName)
                    {
                        var typedArg = arg as SchemaQualifiedObjectName;

                        string name = converter(typedArg.Name);

                        if (!string.IsNullOrWhiteSpace(typedArg.Schema))
                        {
                            string schema = converter(typedArg.Schema);
                            return string.Format("{0}.{1}", schema, name);
                        }

                        return name;
                    }

                    return converter(arg);
                }

                return HandleOtherFormats(format, arg);
            }
            catch (FormatException ex)
            {
                throw new FormatException(string.Format("The format of '{0}' is invalid.", format), ex);
            }
        }

        /// <summary>
        /// Обработка формата стандартным провайдером форматирования
        /// </summary>
        /// <param name="fmt">Формат</param>
        /// <param name="arg">Форматируемый объект</param>
        private static string HandleOtherFormats(string fmt, object arg)
        {
            if (arg is IFormattable)
            {
                return (arg as IFormattable).ToString(fmt, CultureInfo.CurrentCulture);
            }

            return arg != null ? arg.ToString() : string.Empty;
        }
    }
}