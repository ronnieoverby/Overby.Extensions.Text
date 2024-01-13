using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Overby.Extensions.Text.Tests
{
    public class CsvWriterTests
    {
        [Fact]
        public void Codebase()
        {
			using var writer = new StreamWriter(@"C:\Users\ronnie.overby\Desktop\bmcsv\codebase.csv");
			var csv = new CsvWriter(writer) { QualifySetting = CsvWriter.QualifyMode.WhenNeeded };

			csv.AddRecord("filepath", "size", "contents");

			var dir = new DirectoryInfo("c:\\code\\csharp-csv-reader\\");



			foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories).Where(Pass))
			{
				var txt = File.ReadAllText(file.FullName);
				csv.AddRecord(file.FullName, file.Length, txt);
			}

			bool Pass(FileInfo f)
			{
				if (string.IsNullOrWhiteSpace(f.Extension))
					return false;
				if (f.Extension == ".dll")
					return false;
				if (f.Extension == ".nupkg")
					return false;
				if (f.Extension == ".pdb")
					return false;
				if (f.Extension == ".idx")
					return false;
				if (f.Extension == ".pack")
					return false;
				if (f.Extension == ".v2")
					return false;
				if (f.Extension == ".suo")
					return false;
				if (f.Extension == ".cache")
					return false;
				if (f.Extension == ".p7s")
					return false;

				return true;
			}
		}

    }
}
