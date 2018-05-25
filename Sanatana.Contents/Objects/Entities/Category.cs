using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Objects.Entities
{
    public class Category<TKey>
        where TKey : struct
    {
        public TKey CategoryId { get; set; }
        public TKey? ParentCategoryId { get; set; }
        public DateTime AddTimeUtc { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int SortOrder { get; set; }
        public long Version { get; set; }
        public bool IsIndexed { get; set; }
        public bool NeverIndex { get; set; }
    }
}
