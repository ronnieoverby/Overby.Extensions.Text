using Overby.Extensions.Text;
using Overby.Extensions.Text.Tests.TestFiles;
using System.Collections.Generic;
using Xunit;

namespace Tests.Text
{
    public class CsvParsingTests
    {
        [Fact]
        public void Simple()
        {
            using var rdr = TestFiles.Simple.ToStringReader();
            using var it = rdr.ReadCsv().GetEnumerator();

            it.MoveNext();
            var record = it.Current;

            Assert.Equal(new[] { "A", "B", "C" }, record);

            it.MoveNext();
            record = it.Current;
            Assert.Equal(new[] { "D", "E", "F" }, record);

            Assert.False(it.MoveNext());
        }

        [Fact]
        public void SimpleTextQualified()
        {
            using var rdr = TestFiles.SimpleTextQualified.ToStringReader();
            using var it = rdr.ReadCsv().GetEnumerator();

            it.MoveNext();
            var record = it.Current;
            Assert.Equal(new[] { "A", "B", "C" }, record);

            it.MoveNext();
            record = it.Current;
            Assert.Equal(new[] { "D", "E", "F" }, record);

            Assert.False(it.MoveNext());
        }

        [Fact]
        public void AdvancedTextQualified()
        {
            using var rdr = TestFiles.AdvancedTextQualified.ToStringReader();
            using var it = rdr.ReadCsv().GetEnumerator();

            it.MoveNext();
            var record = it.Current;
            Assert.Equal(new[] { "A,B,C", "D,E,F" }, record);

            it.MoveNext();
            record = it.Current;
            Assert.Equal(new[] { "G,H,I", "J,K,L" }, record);

            it.MoveNext();
            record = it.Current;
            Assert.Equal(new[] { "Ronnie \"Dwanye\" Overby", "\"Tiner\" Overby", "\"THIS\",\r\n\"THAT\",\r\n\"THE OTHER!\"" }, record);

            Assert.False(it.MoveNext());
        }

        [Fact]
        public void FieldDataIsNotTrimmed()
        {
            using var rdr = TestFiles.FieldDataIsTrimmed.ToStringReader();
            using var it = rdr.ReadCsv(trimValues: false).GetEnumerator();

            AssertEqual("  Ronnie  ", "  Overby  ");
            AssertEqual("    Tina  ", "   Overby  ");
            AssertEqual("    Anna     Lukus    ");
            Assert.False(it.MoveNext());

            void AssertEqual(params string[] strings) =>
                Assert.Equal(strings, it.MoveNext() ? it.Current : default);
        }

        [Fact]
        public void FieldDataIsTrimmed()
        {
            using var rdr = TestFiles.FieldDataIsTrimmed.ToStringReader();
            using var it = rdr.ReadCsv(trimValues: true).GetEnumerator();

            AssertEqual("Ronnie", "Overby");
            AssertEqual("  Tina  ", "  Overby  ");
            AssertEqual("  Anna    Lukus  ");
            Assert.False(it.MoveNext());

            void AssertEqual(params string[] strings) =>
                Assert.Equal(strings, it.MoveNext() ? it.Current : default);
        }

        [Fact]
        public void EmptyFields()
        {
            using var rdr = TestFiles.EmptyFields.ToStringReader();
            using var it = rdr.ReadCsv(trimValues: false).GetEnumerator();

            AssertEqual("", "");
            AssertEqual("A", "");
            AssertEqual("", "B");
            AssertEqual("", "C", "");
            AssertEqual("", "D", "E", "");
            AssertEqual("", "F", "", "G", "");
            AssertEqual("", "", " ", "");
            Assert.False(it.MoveNext());

            void AssertEqual(params string[] strings) =>
                Assert.Equal(strings, it.MoveNext() ? it.Current : default);
        }

        [Fact]
        public void WithHeader()
        {
            using var rdr = TestFiles.WithHeader.ToStringReader();
            using var it = rdr.ReadCsvWithHeader(trimValues: true).GetEnumerator();

            var record = GetNextOrDefault();
            Assert.Equal("Ronnie", record["Name"]);
            Assert.Equal("30", record["Age"]);
            Assert.Equal("Male", record["Gender"]);
            Assert.Equal("Core Techs", record["EMPLOYER"]);

            record = GetNextOrDefault();
            Assert.Equal("Tina", record["Name"]);
            Assert.Equal("30", record["Age"]);
            Assert.Equal("Female", record["Gender"]);
            Assert.Equal("HPU", record["employer"]);

            record = GetNextOrDefault();
            Assert.Equal("Lukus", record["Name"]);
            Assert.Equal("8", record["Age"]);
            Assert.Equal("Male", record["Gender"]);
            Assert.Empty(record["Employer"]);

            record = GetNextOrDefault();
            Assert.Equal("Anna", record[0]);
            Assert.Equal("3", record[1]);
            Assert.Equal("Female", record[2]);
            Assert.Null(record["Employer"]);
            Assert.Throws<KeyNotFoundException>(() => record["ASDF"].GetHashCode());

            Assert.False(it.MoveNext());

            CsvRecord GetNextOrDefault() =>
                it.MoveNext() ? it.Current : default;
        }

        [Fact]
        public void RecordIsReused()
        {
            using var rdr = TestFiles.WithHeader.ToStringReader();
            var record = new List<string>();
            using var it = rdr.ReadCsvWithHeader(trimValues: false, record: record).GetEnumerator();

            while (it.MoveNext()) ;

            Assert.Equal(3, record.Count);
          
            Assert.Equal("Anna", record[0]);
            Assert.Equal("3", record[1]);
            Assert.Equal("Female", record[2]);

        }
    }
}
