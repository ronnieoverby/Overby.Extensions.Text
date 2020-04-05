using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Overby.Extensions.Text
{
    public static class TextReaderExtensions
    {
        public static IEnumerable<string> ReadLines(this TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    yield break;

                yield return line;
            }
        }

        public static async IAsyncEnumerable<string> ReadLinesAsync(this TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    yield break;

                yield return line;
            }
        }

        public static IEnumerable<(string Text, int Index)> ReadLinesIndexed(this TextReader reader) =>
            ReadLines(reader).Select((t, i) => (t, i));

        public static IAsyncEnumerable<(string Text, int Index)> ReadLinesIndexedAsync(this TextReader reader) =>
            ReadLinesAsync(reader).Select((t, i) => (t, i));
    }
}