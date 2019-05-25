using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search
{
    public class SearchResult<T>
    {
        public List<T> Items { get; set; }
        public long Total { get; set; }
    }
}
