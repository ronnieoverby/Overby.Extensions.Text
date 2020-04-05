using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Overby.Extensions.Text
{
    public static class CsvParsingExtensions
    {
        /// <summary>
        /// Parses delimited text.
        /// </summary>
        /// <param name="reader">The source of character data to parse.</param>
        /// <param name="delimiter">The character that separates fields of data.</param>
        /// <param name="textQualifier">The character that surrounds field text that may contain the delimiter or the text qualifier itself. For literal occurrences of this character within the field data, the character should be doubled.</param>
        /// <returns>An enumerable of string arrays.</returns>
        public static IEnumerable<List<string>> ReadCsv(this TextReader reader, char delimiter = ',', char textQualifier = '"', List<string> record = null, bool trimValues = false)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            const int readNothing = -1;

            bool reuseRecord = record != null;
            record ??= new List<string>();
            bool trimming = trimValues;
            bool inTxt = false;
            int read;
            StringBuilder field = new StringBuilder();
            int fieldLength = 0; // used to trim the ends of field values
            string value;

            while ((read = reader.Read()) != readNothing)
            {
                char c = (char)read;

                if (inTxt)
                {
                    // in text qualified mode

                    if (c == textQualifier)
                    {
                        // reached a qualifier character

                        if (reader.Peek() == textQualifier)
                        {
                            // escaped qualifier character
                            // treat the qualifier as a literal character within the field text
                            Append(c, true);

                            // skip the 2nd qualifier
                            reader.Read();
                        }
                        else
                        {
                            // qualifier terminator
                            // this text qualified field has come to an end
                            inTxt = false;
                            trimming = trimValues;
                        }
                    }
                    else
                    {
                        // reached a qualified character
                        // treat as a literal field character 
                        Append(c, true);
                    }
                }
                else
                {
                    // not in text qualified mode

                    if (c == delimiter)
                    {
                        // reached a field delimiter
                        FlushValue();
                        record.Add(value);
                    }
                    else if (c == textQualifier)
                    {
                        // reached qualifier
                        inTxt = true;
                    }
                    else if (IsNewLine(c))
                    {
                        // new line; new record

                        FlushValue();
                        if (record.Count != 0 || !string.IsNullOrWhiteSpace(value))
                        {
                            // this record is not empty

                            record.Add(value);
                            yield return record;

                            if (reuseRecord)
                                record.Clear();
                            else
                                // allocate the new record with the previous count
                                // because of the high chance each row has the same
                                // field count
                                record = new List<string>(record.Count);
                        }
                    }
                    else if (trimValues && char.IsWhiteSpace(c))
                    {
                        if (trimming)
                        {
                            // discarding whitespace

                            while (true)
                            {
                                int next = reader.Peek();

                                if (next == -1)
                                    // done!
                                    break;

                                char c2 = (char)next;
                                if (char.IsWhiteSpace(c2))
                                {
                                    if (IsNewLine(c2))
                                    {
                                        // never discard new lines, because:
                                        // consider this input:
                                        // `  " text " \n`
                                        // we've begun trimming again after " text "
                                        // discarding new lines would be a mistake here
                                        break;
                                    }

                                    // discard upcoming whitespace
                                    reader.Read();
                                }
                                else
                                {
                                    // next not whitespace
                                    trimming = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // not trimming (mid value whitespace)
                            Append(c, false);
                        }
                    }
                    else
                    {
                        // reached an unqualified character
                        // treat as a literal field character 

                        // character is negated as a way of 
                        // encoding a non-text-qualified character
                        Append(c, true);

                        // next whitespace characters should be preserved
                        trimming = false;
                    }
                }
            }

            FlushValue();
            if (record.Count != 0 || !string.IsNullOrWhiteSpace(value))
            {
                record.Add(value);
                yield return record;
            }

            void Append(char c, bool committed)
            {
                _ = field.Append(c);

                if (committed)
                    fieldLength = field.Length;
            }

            void FlushValue()
            {
                value = field.ToString(0, fieldLength);
                fieldLength = 0;
                field.Clear();
                trimming = trimValues;
            }

            static bool IsNewLine(char c) =>
                c == '\r' || c == '\n';
        }




        /// <summary>
        /// Parses delimited text. The first row of data should be a header record containing the names of the fields in the following records.
        /// </summary>
        /// <param name="reader">The source of character data to parse.</param>
        /// <param name="delimiter">The character that separates fields of data.</param>
        /// <param name="textQualifier">The character that surrounds field text that may contain the delimiter or the text qualifier itself. For literal occurrences of this character within the field data, the character should be doubled.</param>
        /// <returns>An enumerable of <see cref="CsvRecord"/> objects.</returns>
        public static IEnumerable<CsvRecord> ReadCsvWithHeader(this TextReader reader,
           StringComparer fieldKeyComparer = null,
           char delimiter = ',',
           char textQualifier = '"', List<string> record = null, bool trimValues = false)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            IEnumerator<IList<string>> it = reader.ReadCsv(
                delimiter: delimiter,
                textQualifier: textQualifier,
                record: record,
                trimValues: trimValues).GetEnumerator();

            if (!it.MoveNext())
                yield break;

            IList<string> header = it.Current;

            while (it.MoveNext())
                yield return new CsvRecord(header, it.Current, fieldKeyComparer ?? StringComparer.OrdinalIgnoreCase);
        }      
    }
}
