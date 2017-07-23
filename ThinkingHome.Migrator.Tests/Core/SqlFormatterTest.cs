using System;
using Xunit;
using ThinkingHome.Migrator.Framework;
using ThinkingHome.Migrator.Framework.Extensions;
using ThinkingHome.Migrator.Providers;

namespace ThinkingHome.Migrator.Tests.Core
{
    /// <summary>
    /// Тестирование форматирования строк для экранирования зарезервированных слов в запросах
    /// </summary>
    public class SqlFormatterTest
    {
        private static string Convert(object arg)
        {
            return string.Format("<{0}>", arg);
        }

        private static readonly SqlFormatter Formatter = new SqlFormatter(Convert);

        [Fact]
        public void CanFormatObject()
        {
            string sql = string.Format(Formatter, "update {0:NAME} set {1:NAME} = '{2}', {1:NAME} = '{2}'", "test1", "column1", "value1");
            Assert.Equal(sql, "update <test1> set <column1> = 'value1', <column1> = 'value1'");
        }

        [Fact]
        public void CanFormatCollection2()
        {
            string sql = string.Format(Formatter, "insert into {0:NAME} ({1:COLS}) values ('{2}','{3}')",
                "test1", new[] { "column1", "column2" }, "value1", "value2");
            Assert.Equal(sql, "insert into <test1> (<column1>,<column2>) values ('value1','value2')");
        }

        [Fact]
        public void CanFormatWithInnerFormatter()
        {
            string strDate = new DateTime(2011, 4, 26).ToString("yyyy-MM:dd", Formatter);
            Assert.Equal("2011-04:26", strDate);
        }

        [Fact]
        public void CanFormatSchemaQualifiedObjectName()
        {
            var table = "Moo".WithSchema("Xxx");
            string sql = string.Format(Formatter, "select * from {0:NAME}", table);

            Assert.Equal(sql, "select * from <Xxx>.<Moo>");
        }

        [Fact]
        public void CanFormatObjectNameWithoutSchema()
        {
            var table = new SchemaQualifiedObjectName {Name = "Moo"};
            string sql = string.Format(Formatter, "select * from {0:NAME}", table);

            Assert.Equal(sql, "select * from <Moo>");
        }
    }
}