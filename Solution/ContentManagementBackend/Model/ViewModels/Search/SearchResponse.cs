using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class SearchResponse<TKey>
        where TKey : struct
    {
        public List<ContentBase<TKey>> Content { get; set; }
        public List<List<string>> HighlightedFieldNames { get; set; }
        public long Total { get; set; }
        public bool HasExceptions { get; set; }
    }
}
