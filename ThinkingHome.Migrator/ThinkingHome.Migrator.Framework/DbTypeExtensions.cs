using System.Data;

namespace ThinkingHome.Migrator.Framework
{
    /// <summary>
    /// Методы-расширения для класса System.Data.DbType
    /// </summary>
    public static class DbTypeExtensions
    {

        /// <summary>
        /// Создание объекта ColumnType с заданным размером
        /// </summary>
        /// <param name="type">Тип колонки</param>
        /// <param name="length">Размер</param>
        /// <returns>Возвращает ColumnType с заданными параметрами</returns>
        public static ColumnType WithSize(this DbType type, int length)
        {
            return new ColumnType(type, length);
        }

        /// <summary>
        /// Создание объекта ColumnType с заданным размером
        /// </summary>
        /// <param name="type">Тип колонки</param>
        /// <param name="length">Размер</param>
        /// <param name="scale">Количество знаков после запятой</param>
        /// <returns>Возвращает ColumnType с заданными параметрами</returns>
        public static ColumnType WithSize(this DbType type, int length, int scale)
        {
            return new ColumnType(type, length, scale);
        }
    }
}