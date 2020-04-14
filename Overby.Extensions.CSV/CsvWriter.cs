using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Globalization.CultureInfo;

namespace Overby.Extensions.Text
{
    public class CsvWriter
    {
        readonly TextWriter _writer;
        readonly char _delim;
        readonly char _tq;
        bool _newRec = true;

        public Qual QualifySetting { get; set; } = Qual.Auto;

        public enum Qual
        {
            Auto, // qualify text if needed
            Force, // qualify text
            Prevent // do not qualify text
        }

        public CsvWriter(TextWriter writer, char delimiter = ',', char textQualifier = '"')
        {
            _writer = writer;
            _delim = delimiter;
            _tq = textQualifier;
        }

        public CsvWriter AddFields(IEnumerable data)
        {
            if (data is string)
                data = new[] { data };

            foreach (var item in data)
                AddField(item);

            return this;
        }

        public CsvWriter AddFields(params object[] data)
        {
            return AddFields(data.AsEnumerable());
        }

        public CsvWriter AddRecord(IEnumerable data)
        {
            if (data is string)
                data = new[] { data };

            return AddFields(data).EndRecord();
        }

        public CsvWriter AddRecord(ICsvWritable data)
        {
            return AddRecord(data.GetCsvFields());
        }

        public CsvWriter AddRecord(params object[] data)
        {
            return AddFields(data.AsEnumerable()).EndRecord();
        }

        public CsvWriter AddField(object obj)
        {
            if (!(obj is string s))
                s = obj?.ToString() ?? "";

            if (QualifySetting == Qual.Prevent)
                return AddRawField(s);

            if (QualifySetting == Qual.Force || ShouldQualifyValue())
                return AddRawField(Qualify(s, _tq));

            return AddRawField(s);

            bool ShouldQualifyValue()
            {
                foreach (char c in s)
                    if (c == _delim || c == '\r' || c == '\n' || c == _tq)
                        return true;

                return false;
            }
        }

        public CsvWriter AddRawField(string s)
        {
            if (!_newRec)
                _writer.Write(_delim);

            _writer.Write(s ?? "");

            _newRec = false;
            return this;
        }

        public static string Qualify(in string s, in char qualifier) =>
            qualifier + Escape(s, qualifier) + qualifier;

        public static string Escape(in string s, in char qualifier)
        {
            var q = qualifier.ToString();
            return s.Replace(q, q + qualifier);
        }

        public CsvWriter EndRecord()
        {
            _writer.Write(Environment.NewLine);
            _newRec = true;
            return this;
        }

        public static void Write<T>(IEnumerable<T> data, TextWriter writer, char delimiter = ',', char textQualifier = '"')
            where T : ICsvWritable
        {
            var csv = new CsvWriter(writer, delimiter, textQualifier);
            using var it = data.GetEnumerator();
            if (it.MoveNext())
                csv.AddRecord(it.Current.GetCsvHeadings())
                    .AddRecord(it.Current.GetCsvFields());

            while (it.MoveNext())
                csv.AddRecord(it.Current.GetCsvFields());
        }
    }
}