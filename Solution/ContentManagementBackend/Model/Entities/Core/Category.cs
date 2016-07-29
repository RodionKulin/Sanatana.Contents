using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class Category<TKey>
        where TKey : struct
    {
        public TKey CategoryID { get; set; }
        public TKey? ParentCategoryID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime AddTimeUtc { get; set; }
        public int SortOrder { get; set; }
        public List<KeyValuePair<int, string>> Permissions { get; set; }
    }
}
