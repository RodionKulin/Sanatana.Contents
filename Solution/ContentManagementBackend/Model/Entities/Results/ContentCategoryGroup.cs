using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class ContentCategoryGroup<TKey>
        where TKey : struct
    {
        public TKey CategoryID { get; set; }
        public List<ContentBase<TKey>> Content { get; set; }
    }
}
