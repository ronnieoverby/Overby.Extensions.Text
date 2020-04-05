using System.Collections.Generic;

namespace Overby.Extensions.Text
{
    public interface ICsvWritable
    {
        IEnumerable<object> GetCsvHeadings();
        IEnumerable<object> GetCsvFields();
    }
}