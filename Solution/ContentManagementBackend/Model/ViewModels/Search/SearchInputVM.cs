using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class SearchInputVM<TKey>
        where TKey : struct
    {
        public string Input { get; set; }
        public TKey? CategoryID { get; set; }
        public int Page { get; set; }
    }
}
