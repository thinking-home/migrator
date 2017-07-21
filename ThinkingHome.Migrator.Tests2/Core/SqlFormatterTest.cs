using System;
using NUnit.Framework;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;
using ThinkingHome.Migrator.Providers;

namespace ThinkingHome.Migrator.Tests.Core
{
    /// <summary>
    /// Тестирование форматирования строк для экранирования зарезервированных слов в запросах
    /// </summary>
    [TestFixture]
    public class SqlFormatterTest
    {
        private static string Convert(object arg)
        {
            return string.Format("<{0}>", arg);
        }

        private static readonly SqlFormatter Formatter = new SqlFormatter(Convert);

        [Test]
        public void CanFormatObject()
        {
            string sql = string.Format(Formatter, "update {0:NAME} set {1:NAME} = '{2}', {1:NAME} = '{2}'", "test1", "column1", "value1");
            Assert.AreEqual(sql, "update <test1> set <column1> = 'value1', <column1> = 'value1'");
        }

        [Test]
        public void CanFormatCollection2()
        {
            string sql = string.Format(Formatter, "insert into {0:NAME} ({1:COLS}) values ('{2}','{3}')",
                "test1", new[] { "column1", "column2" }, "value1", "value2");
            Assert.AreEqual(sql, "insert into <test1> (<column1>,<column2>) values ('value1','value2')");
        }

        [Test]
        public void CanFormatWithInnerFormatter()
        {
            string strDate = new DateTime(2011, 4, 26).ToString("yyyy-MM:dd", Formatter);
            Assert.AreEqual("2011-04:26", strDate);
        }

        [Test]
        public void CanFormatSchemaQualifiedObjectName()
        {
            var table = "Moo".WithSchema("Xxx");
            string sql = string.Format(Formatter, "select * from {0:NAME}", table);

            Assert.AreEqual(sql, "select * from <Xxx>.<Moo>");
        }

        [Test]
        public void CanFormatObjectNameWithoutSchema()
        {
            var table = new SchemaQualifiedObjectName {Name = "Moo"};
            string sql = string.Format(Formatter, "select * from {0:NAME}", table);

            Assert.AreEqual(sql, "select * from <Moo>");
        }
    }
}