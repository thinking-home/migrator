using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using ThinkingHome.Migrator.Framework.Extensions;
using ThinkingHome.Migrator.Providers;

namespace ThinkingHome.Migrator.Tests.Core
{
    [TestFixture]
    public class OtherTest
    {
        [Test]
        public void CanGetMigrationHumanName()
        {
            Assert.AreEqual(
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

        [Test]
        public void CanConvertNullObjectToArrays()
        {
            var arrays = TransformationProvider<IDbConnection>.ConvertObjectToArrays(null);
            Assert.IsNull(arrays);
        }

        [Test]
        public void CanConvertObjectWithIndexedFieldToArrays()
        {
            var obj = new Gwelkghlw
            {
                Id = 1254,
                [12] = "qewgfwgewrgh",
                [13] = "eljpowwdoihgvwoihio"
            };

            var arrays = TransformationProvider<IDbConnection>.ConvertObjectToArrays(obj);

            Assert.AreEqual(new[] { "Id" }, arrays.Item1);
            Assert.AreEqual(new[] { "1254" }, arrays.Item2);
        }

        [Test]
        public void CanConvertObjectWithNullFieldToArrays()
        {
            var obj = new { x = 1254, y = (object)null, z = (string)null };

            var arrays = TransformationProvider<IDbConnection>.ConvertObjectToArrays(obj);

            Assert.AreEqual(new[] { "x", "y", "z" }, arrays.Item1);
            Assert.AreEqual(new[] { "1254", null, null }, arrays.Item2);
        }

        #endregion
    }
}