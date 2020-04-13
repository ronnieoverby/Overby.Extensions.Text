using BenchmarkDotNet.Running;
using System;

namespace Overby.Extensions.Text.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            var bm = new CsvBenchmarks { FileFields = "orders 97", Max = 50000 };

            bm.Globalsetup();
            bm.overby();
#else
            var summary = BenchmarkRunner.Run<CsvBenchmarks>();
            Console.WriteLine(summary);
            Console.ReadLine();
#endif


        }
    }
}
