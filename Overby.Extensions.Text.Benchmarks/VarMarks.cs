using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overby.Extensions.Text.Benchmarks
{
    public class VarMarks
    {
        [Params("","ronnie overby")]
        public string StringValue { get; set; }

        [Benchmark]
        public void FirstCharacter1()
        {
            var first = StringValue.FirstOrDefault();
        }
        
        [Benchmark]
        public void FirstCharacter2()
        {
            var first = StringValue.ElementAtOrDefault(0);
        }
    }
}
