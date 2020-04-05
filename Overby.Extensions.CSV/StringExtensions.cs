using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Overby.Extensions.Text
{
    public static class StringExtensions
    {
        public static byte[] Encode(this string s, Encoding encoding = null)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            encoding ??= Encoding.UTF8;
            return encoding.GetBytes(s);
        }

        public static string Decode(this IEnumerable<byte> bytes, Encoding encoding = null)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            encoding ??= Encoding.UTF8;
            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Gets substring by index and length.
        /// Doesn't fail if the index and length do not refer to a location within the string.
        /// </summary>
        public static string SafeSubstring(this string s, int startIndex, int length)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            // originally implemented as:
            //   return string.Concat(s.Skip(startIndex).Take(length));
            // but the following code is ~123x faster

            if (s.Length < startIndex)
                return "";

            if (startIndex < 0)
                startIndex = 0;

            if (length < 0)
                length = 0;

            unchecked
            {
                var totalLength = startIndex + length;

                // overflow?
                if (totalLength < 0)
                    totalLength = int.MaxValue;

                if (totalLength > s.Length)
                    length = s.Length - startIndex;
            }

            return s.Substring(startIndex, length);
        }

        public static IEnumerable<string> ToTextElements(this string source)
        {
            var e = StringInfo.GetTextElementEnumerator(source);
            while (e.MoveNext())
                yield return e.GetTextElement();
        }

        /// <summary>
        /// Splits a string where each character satisfies the predicate.
        /// </summary>
        public static IEnumerable<string> SplitWhere(this string s, Func<char, bool> predicate, bool returnSplitChars = false)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var curr = new StringBuilder();
            foreach (var c in s)
            {
                if (predicate(c))
                {
                    if (curr.Length > 0)
                    {
                        yield return curr.ToString();
                        curr.Clear();
                    }

                    if (returnSplitChars)
                        yield return c.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    curr.Append(c);
                }
            }

            if (curr.Length > 0)
                yield return curr.ToString();
        }

        public static bool IsNullOrEmpty(this string s) =>
            string.IsNullOrEmpty(s);

        public static bool IsNullOrWhiteSpace(this string s) => 
            string.IsNullOrWhiteSpace(s);


        public static StringReader ToStringReader(this string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            return new StringReader(s);
        }

        public static IEnumerable<string> ReadLines(this string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            return s.ToStringReader().ReadLines();
        }
    }
}