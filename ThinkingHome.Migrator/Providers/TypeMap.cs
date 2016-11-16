using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;

namespace ThinkingHome.Migrator.Providers
{
    using TypesDictionary = Dictionary<DbType, SortedList<int, TypeMap.TypeDefinitionInfo>>;

    /// <summary>
    /// This class maps a DbType to names.
    /// </summary>
    /// <remarks>
    /// Associations may be marked with a capacity. Calling the <c>Get()</c>
    /// method with a type and actual size n will return the associated
    /// name with smallest capacity >= n, if available and an unmarked
    /// default type otherwise.
    /// Eg, setting
    /// <code>
    ///		Names.Put(DbType,			"TEXT" );
    ///		Names.Put(DbType,	255,	"VARCHAR($l)" );
    ///		Names.Put(DbType,	65534,	"LONGVARCHAR($l)" );
    /// </code>
    /// will give you back the following:
    /// <code>
    ///		Names.Get(DbType)			// --> "TEXT" (default)
    ///		Names.Get(DbType,100)		// --> "VARCHAR(100)" (100 is in [0:255])
    ///		Names.Get(DbType,1000)		// --> "LONGVARCHAR(1000)" (100 is in [256:65534])
    ///		Names.Get(DbType,100000)	// --> "TEXT" (default)
    /// </code>
    /// On the other hand, simply putting
    /// <code>
    ///		Names.Put(DbType, "VARCHAR($l)" );
    /// </code>
    /// would result in
    /// <code>
    ///		Names.Get(DbType)		// --> "VARCHAR($l)" (will cause trouble)
    ///		Names.Get(DbType,100)	// --> "VARCHAR(100)"
    ///		Names.Get(DbType,1000)	// --> "VARCHAR(1000)"
    ///		Names.Get(DbType,10000)	// --> "VARCHAR(10000)"
    /// </code>
    /// </remarks>
    public class TypeMap
    {
        public class TypeDefinitionInfo
        {
            public string TypeDefinitionPattern { get; set; }

            public int? DefaultScale { get; set; }
        }

        #region Private

        private readonly TypesDictionary typeMapping = new TypesDictionary();
        private readonly Dictionary<DbType, string> defaults = new Dictionary<DbType, string>();

        /// <summary>
        /// �������� �������� ��-��������� ��� ����
        /// </summary>
        /// <param name="typecode">���</param>
        /// <param name="value">��������</param>
        private void PutDefaultValue(DbType typecode, string value)
        {
            defaults[typecode] = value;
        }

        /// <summary>
        /// �������� �������� ��-���������
        /// </summary>
        /// <param name="typecode">���</param>
        /// <returns>
        /// �������� ��-��������� ��� ������� ����.
        /// ���� �������� ���������� �� �������, ������������ ����������.
        /// </returns>
        private string GetDefaultValue(DbType typecode)
        {
            string result;

            if (!defaults.TryGetValue(typecode, out result))
            {
                throw new ArgumentException($"Provider does not support DbType {typecode}.");
            }

            return result;
        }

        private void PutValue(DbType typecode, int length, TypeDefinitionInfo value)
        {
            SortedList<int, TypeDefinitionInfo> map;

            if (!typeMapping.TryGetValue(typecode, out map))
            {
                typeMapping[typecode] = map = new SortedList<int, TypeDefinitionInfo>();
            }

            map[length] = value;
        }

        /// <summary>
        /// ��������� ������ SQL ��� ���� � ������ ��������
        /// </summary>
        /// <param name="typecode">���</param>
        /// <param name="size">������</param>
        /// <returns>
        /// ���������� ������ SQL ��� ����, ������������ � ������ ��� ��������.
        /// ���� ������ SQL ���������� �� �������, ������������ null.
        /// </returns>
        private TypeDefinitionInfo GetValue(DbType typecode, int size)
        {
            SortedList<int, TypeDefinitionInfo> map;
            typeMapping.TryGetValue(typecode, out map);

            if (map == null)
            {
                return null;
            }

            if (!map.Any(pair => pair.Key >= size))
            {
                return null;
            }

            return map
                .OrderBy(pair => pair.Key)
                .First(pair => pair.Key >= size).Value;
        }

        #endregion

        #region Put

        /// <summary>
        /// ������������ �������� ���� ��, ������� ����� ������������ ���
        /// ����������� �������� DbType, ���������� � "���������".
        /// <para><c>$l</c> - ����� �������� �� ���������� �������� �����</para>
        /// <para><c>$s</c> - ����� �������� �� ���������� ��������, ������������
        /// ���������� ������ ����� ������� ��� ������������ �����</para>�
        /// </summary>
        /// <param name="typecode">The typecode</param>
        /// <param name="length">Maximum length of database type</param>
        /// <param name="name">The database type name</param>
        public void Put(DbType typecode, int? length, string name)
        {
            Put(typecode, length, name, null);
        }

        /// <summary>
        /// ������������ �������� ���� ��, ������� ����� ������������ ���
        /// ����������� �������� DbType, ���������� � "���������".
        /// <para><c>$l</c> - ����� �������� �� ���������� �������� �����</para>
        /// <para><c>$s</c> - ����� �������� �� ���������� ��������, ������������
        /// ���������� ������ ����� ������� ��� ������������ �����</para>
        /// </summary>
        /// <param name="typecode">���</param>
        /// <param name="length">������������ �����</param>
        /// <param name="name">�������� ���� ��</param>
        /// <param name="defaultScale">�������� ��-���������: ���������� ������ ����� ������� ��� ������������ �����</param>
        public void Put(DbType typecode, int? length, string name, int? defaultScale)
        {
            if (length.HasValue)
            {
                PutValue(typecode, length.Value,
                    new TypeDefinitionInfo {TypeDefinitionPattern = name, DefaultScale = defaultScale});
            }
            else
            {
                PutDefaultValue(typecode, name);
            }
        }

        /// <summary>
        /// ������������ �������� ���� ��, ������� ����� ������������ ���
        /// ����������� �������� DbType, ���������� � "���������".
        /// </summary>
        /// <para><c>$l</c> - ����� �������� �� ���������� �������� �����</para>
        /// <para><c>$s</c> - ����� �������� �� ���������� ��������, ������������
        /// ���������� ������ ����� ������� ��� ������������ �����</para>
        /// <param name="typecode">���</param>
        /// <param name="name">�������� ���� ��</param>
        public void Put(DbType typecode, string name)
        {
            PutDefaultValue(typecode, name);
        }

        #endregion

        #region Get

        public string Get(ColumnType columnType)
        {
            return Get(columnType.DataType, columnType.Length, columnType.Scale);
        }

        public string Get(DbType typecode)
        {
            return Get(typecode, null);
        }

        public string Get(DbType typecode, int? length)
        {
            return Get(typecode, length, null);
        }

        public string Get(DbType typecode, int? length, int? scale)
        {
            TypeDefinitionInfo result = null;

            if (length.HasValue)
            {
                result = GetValue(typecode, length.Value);
            }

            if (result == null)
            {
                result = new TypeDefinitionInfo {TypeDefinitionPattern = GetDefaultValue(typecode), DefaultScale = null};
            }

            return Replace(result.TypeDefinitionPattern, length, scale ?? result.DefaultScale);
        }

        #endregion

        /// <summary>
        /// ��������, ��� �������� ��� ���������������
        /// </summary>
        /// <param name="type">����������� ���</param>
        public bool HasType(DbType type)
        {
            return typeMapping.ContainsKey(type) || defaults.ContainsKey(type);
        }

        #region Replacing

        public const string LENGTH_PLACE_HOLDER = "$l";
        public const string SCALE_PLACE_HOLDER = "$s";

        private static string Replace(string type, int? size, int? scale)
        {
            if (size.HasValue)
            {
                type = type.ReplaceOnce(LENGTH_PLACE_HOLDER, size.ToString());
            }

            if (scale.HasValue)
            {
                type = type.ReplaceOnce(SCALE_PLACE_HOLDER, scale.ToString());
            }

            return type;
        }

        #endregion
    }
}