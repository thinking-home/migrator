using System.Collections.Generic;
using System.Data;
using Xunit;
using ThinkingHome.Migrator.Framework.Extensions;
using ThinkingHome.Migrator.Providers;

namespace ThinkingHome.Migrator.Tests.Core
{
    public class OtherTest
    {
        [Fact]
        public void CanGetMigrationHumanName()
        {
            Assert.Equal(
                "Migration0101 add new table with primary key",
                "Migration0101_Add_NewTable_with_primary___Key".ToHumanName());
        }

        #region object converter

        public class Gwelkghlw
        {
            private readonly Dictionary<int, string> dic = new Dictionary<int, string>();

            public int Id { get; set; }

            public string this[int index]
            {
                get
                {
                    return dic.ContainsKey(index) ? dic[index] : null;
                }
                set { dic[index] = value; }
            }
        }

        [Fact]
        public void CanConvertNullObjectToArrays()
        {
            var arrays = TransformationProvider<IDbConnection>.ConvertObjectToArrays(null);
            Assert.Null(arrays);
        }

        [Fact]
        public void CanConvertObjectWithIndexedFieldToArrays()
        {
            var obj = new Gwelkghlw
            {
                Id = 1254,
                [12] = "qewgfwgewrgh",
                [13] = "eljpowwdoihgvwoihio"
            };

            var arrays = TransformationProvider<IDbConnection>.ConvertObjectToArrays(obj);

            Assert.Equal(new[] { "Id" }, arrays.Item1);
            Assert.Equal(new[] { "1254" }, arrays.Item2);
        }

        [Fact]
        public void CanConvertObjectWithNullFieldToArrays()
        {
            var obj = new { x = 1254, y = (object)null, z = (string)null };

            var arrays = TransformationProvider<IDbConnection>.ConvertObjectToArrays(obj);

            Assert.Equal(new[] { "x", "y", "z" }, arrays.Item1);
            Assert.Equal(new[] { "1254", null, null }, arrays.Item2);
        }

        #endregion
    }
}