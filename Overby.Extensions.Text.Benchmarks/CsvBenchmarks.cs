using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Globalization.CultureInfo;


namespace Overby.Extensions.Text.Benchmarks
{
    [DryJob]
    [MemoryDiagnoser]

    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public class CsvBenchmarks
    {
        readonly int _max = 100_000;

        [Benchmark(Baseline = true)]
        public void overby()
        {
            using var reader = OpenTextReader();
            foreach (var _ in CsvParsingExtensions.ReadCsv(reader, record: new List<string>(), trimValues: false).Take(_max)) ;
        }

        [Benchmark(Baseline = !true)]
        public void overbymixed()
        {
            using var reader = new StreamReader(filepathmixed);
            foreach (var _ in CsvParsingExtensions.ReadCsv(reader, record: new List<string>(), trimValues: false).Take(_max)) ;
        }

        //[Benchmark]
        //public void coretechs()
        //{
        //    using var reader = OpenTextReader();
        //    foreach (var _ in CoreTechs.Common.Text.CsvParsingExtensions.ReadCsv(reader).Take(_max)) ;
        //}

        //[Benchmark]
        //public void joshclose()
        //{
        //    using var reader = OpenTextReader();
        //    using var csv = new CsvHelper.CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(InvariantCulture)
        //    {
        //        BadDataFound = null
        //    });

        //    for (int i = 0; i < _max && csv.Read(); i++) ;
        //}

        //[Benchmark]
        //public void stevehansen()
        //{
        //    using var reader = OpenTextReader();
        //    foreach (var _ in Csv.CsvReader.Read(reader, new Csv.CsvOptions { })) ;
        //}


        //[Benchmark]
        //public void phatcher()
        //{
        //    // https://github.com/phatcher/CsvReader

        //    using var reader = OpenTextReader();
        //    using var csv = new LumenWorks.Framework.IO.Csv.CsvReader(reader, true)
        //    {
        //        //SupportsMultiline = true,
        //        //DefaultParseErrorAction =  LumenWorks.Framework.IO.Csv.ParseErrorAction.RaiseEvent
        //    };
        //    foreach (var _ in csv.Take(_max)) ;
        //}

        //[Benchmark]
        //public void vb()
        //{

        //    using var reader = OpenTextReader();
        //    var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(reader)
        //    {
        //        Delimiters = new[] { "," },
        //        TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited,
        //        TrimWhiteSpace = false,
        //        HasFieldsEnclosedInQuotes = true,
        //    };

        //    foreach (var _ in Read().Take(_max)) ;

        //    IEnumerable<string[]> Read()
        //    {
        //        while (true)
        //        {
        //            var record = parser.ReadFields();
        //            if (record == null)
        //                yield break;

        //            yield return record;
        //        }

        //    }
        //}

        //[Benchmark]
        //public void bytefish()
        //{

        //    // this library is … interesting
        //    // I managed to find this in the tests

        //    var csvParserOptions = new TinyCsvParser.CsvParserOptions(false, ',');
        //    var csvMapper = new TinyCsvParser.Mapping.CsvStringArrayMapping();
        //    var csvParser = new TinyCsvParser.CsvParser<string[]>(csvParserOptions, csvMapper);

        //    using var stream = OpenStream();

        //    foreach (var record in
        //        TinyCsvParser.CsvParserExtensions.ReadFromStream(csvParser, stream, Encoding.UTF8).Take(_max))
        //    {
        //        if (!record.IsValid)
        //            throw new Exception(record.Error.Value);
        //    }
        //}

        //[Benchmark]
        //public void MikeStall()
        //{
        //    using var stream = OpenStream();
        //    var builder = DataAccess.DataTable.New;
        //    foreach (var _ in DataAccess.DataTableBuilderExtensions.ReadLazy(builder, stream).Rows.Take(_max)) ;
        //}

        //[Benchmark]
        //public void linqToCsv()
        //{
        //    using var reader = OpenTextReader();
        //    foreach (var _ in new LINQtoCSV.CsvContext().Read<LINQtoCSV.DataRow>(reader, new LINQtoCSV.CsvFileDescription
        //    {
        //        SeparatorChar = ',',
        //        FirstLineHasColumnNames = true,
        //    }).Take(_max)) ;
        //}

        //[Benchmark] // they don't support streaming
        public void svcstack()
        {
            using var reader = OpenTextReader();
            foreach (var _ in ServiceStack.Text.CsvSerializer.DeserializeFromReader<IEnumerable<object>>(reader)) ;
        }

        [Benchmark]
        public void tspense()
        {
            using var reader = OpenTextReader();
            using var cr = new CSVFile.CSVReader(reader);
            foreach (var _ in cr.Take(_max)) ;
        }

        [Benchmark]
        public void tspenseasync()
        {
            using var reader = OpenTextReader();
            using var cr = new CSVFileAsync.CSVReader(reader);
            
        }

        [Benchmark]
        public void tspensemixed()
        {
            using var reader = new StreamReader(filepathmixed);
            using var cr = new CSVFile.CSVReader(reader);
            foreach (var _ in cr.Take(_max)) ;
        }

        const string filepath = @"C:\Users\ronnie.overby\Desktop\million_orders.csv";
        const string filepathmixed = @"C:\Users\ronnie.overby\Desktop\million_orders_mixed.csv";

        private Stream OpenStream() => File.OpenRead(filepath);
        private StreamReader OpenTextReader() => new StreamReader(filepath);
    }
}
