using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Globalization.CultureInfo;

namespace Overby.Extensions.Text.Benchmarks
{
    [DryJob]
    //[ShortRunJob]
    [MemoryDiagnoser]

    [Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
    public class CsvBenchmarks
    {
        [Params(35_000)]
        public int Max { get; set; }

        [Params(
            "orders 97",
            //"multiqual 97",
            "someEsc 97",
            "somenls 97",
            "somews 97",
            //"allqual 97",
            //"longvals 97",
            //"shortvals 97",
            //"emptyvals 97",
            //"tonsnls 97",
            "codebase 3"
            )]
        public string FileFields { get; set; }

        string _file;
        int _fields;

        [GlobalSetup]
        public void Globalsetup()
        {
            (_file, _fields) = ParseFileAndFields(FileFields);
        }


        private Stream OpenStream() => File.OpenRead(GetPath(_file));
        private StreamReader OpenTextReader() => new StreamReader(GetPath(_file));

        private string GetPath(string file) =>
            $@"C:\Users\ronnie.overby\Desktop\bmcsv\{file}.csv";


        [Benchmark(Baseline = true)]
        public void overby()
        {
            using var reader = OpenTextReader();
            foreach (var rec in CsvParsingExtensions.ReadCsv(reader, record: new List<string>(), trimValues: false).Take(Max))
            {
                EnsureFieldCount(rec.Count);
            }
        }

        
        [Benchmark]
        public void tspense()
        {
            if (_file.Contains("tonsnls"))
                throw new Exception("too slow to benchmark");
            
            if (_file.Contains("codebase"))
                throw new Exception("too slow to benchmark");

            using var reader = OpenTextReader();
            using var cr = new CSVFile.CSVReader(reader);
            foreach (var rec in cr.Take(Max))
            {
                EnsureFieldCount(rec.Length);
            }
        }

        //[Benchmark]
        public void coretechs()
        {
            using var reader = OpenTextReader();
            foreach (var rec in CoreTechs.Common.Text.CsvParsingExtensions.ReadCsv(reader).Take(Max))
            {
                EnsureFieldCount(rec.Length);
            }
        }

        //[Benchmark]
        public void joshclose()
        {
            using var reader = OpenTextReader();
            using var csv = new CsvHelper.CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(InvariantCulture)
            {
                BadDataFound = null,
                  
            });

            for (int i = 0; i < Max && csv.Read(); i++)
                EnsureFieldCount(csv.Context.Record.Length);
        }

        //[Benchmark]
        public void stevehansen()
        {
            using var reader = OpenTextReader();
            foreach (var rec in Csv.CsvReader.Read(reader, new Csv.CsvOptions { AllowNewLineInEnclosedFieldValues = true, Separator = ',',  }).Take(Max))
            {
                EnsureFieldCount(rec.Values.Length);
            }
        }


        //[Benchmark]
        public void phatcher()
        {
            // https://github.com/phatcher/CsvReader

            using var reader = OpenTextReader();
            using var csv = new LumenWorks.Framework.IO.Csv.CsvReader(reader, true)
            {
                //SupportsMultiline = true,
                //DefaultParseErrorAction =  LumenWorks.Framework.IO.Csv.ParseErrorAction.RaiseEvent
            };
            foreach (var rec in csv.Take(Max))
            {
                EnsureFieldCount(rec.Length);
            }
        }

        //[Benchmark]
        public void vb()
        {

            using var reader = OpenTextReader();
            var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(reader)
            {
                Delimiters = new[] { "," },
                TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited,
                TrimWhiteSpace = false,
                HasFieldsEnclosedInQuotes = true,
            };

            foreach (var rec in Read().Take(Max))
            {
                EnsureFieldCount(rec.Length);
            }

            IEnumerable<string[]> Read()
            {
                while (true)
                {
                    var record = parser.ReadFields();
                    if (record == null)
                        yield break;

                    yield return record;
                }

            }
        }

        //[Benchmark]
        public void bytefish()
        {

            // this library is … interesting
            // I managed to find this in the tests

            var csvParserOptions = new TinyCsvParser.CsvParserOptions(false, ',');
            var csvMapper = new TinyCsvParser.Mapping.CsvStringArrayMapping();
            var csvParser = new TinyCsvParser.CsvParser<string[]>(csvParserOptions, csvMapper);

            using var stream = OpenStream();

            foreach (var record in
                TinyCsvParser.CsvParserExtensions.ReadFromStream(csvParser, stream, System.Text.Encoding.UTF8).Take(Max))
            {
                if (!record.IsValid)
                    throw new Exception(record.Error.Value);

                EnsureFieldCount(record.Result.Length);
            }
        }

        //[Benchmark]
        public void MikeStall()
        {
            using var stream = OpenStream();
            var builder = DataAccess.DataTable.New;
            foreach (var rec in DataAccess.DataTableBuilderExtensions.ReadLazy(builder, stream).Rows.Take(Max)) 
            {
                EnsureFieldCount(rec.Values.Count);
            }

        }

        //[Benchmark]
        public void linqToCsv()
        {
            using var reader = OpenTextReader();
            foreach (var rec in new LINQtoCSV.CsvContext().Read<LINQtoCSV.DataRow>(reader, new LINQtoCSV.CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
            }).Take(Max))
            {
                EnsureFieldCount(rec.Count);
            }
        }

        //[Benchmark] // they don't support streaming
        public void svcstack()
        {
            using var reader = OpenTextReader();
            foreach (var _ in ServiceStack.Text.CsvSerializer.DeserializeFromReader<IEnumerable<object>>(reader))
            {
                // can't count the columns
            }
        }





        private void EnsureFieldCount(int count)
        {
            if (count != _fields)
                throw new InvalidOperationException($"wrong fields: {count}/{_fields}");
        }


        (string file, int fields) ParseFileAndFields(string s)
        {
            var split = s.Split();
            return (split[0], int.Parse(split[1]));

        }


    }

}
