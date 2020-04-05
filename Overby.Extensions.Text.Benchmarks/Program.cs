using BenchmarkDotNet.Running;
using System;

namespace Overby.Extensions.Text.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            new CsvBenchmarks().tspense();
#else
            var summary = BenchmarkRunner.Run<CsvBenchmarks>();
            Console.WriteLine(summary);
            Console.ReadLine();
#endif


        }
    }
}
