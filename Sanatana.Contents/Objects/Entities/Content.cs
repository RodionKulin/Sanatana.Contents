using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Objects.Entities
{
    public class Content<TKey>
        where TKey : struct
    {
        //properties
        public TKey ContentId { get; set; }
        public TKey CategoryId { get; set; }
        public long Version { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string FullText { get; set; }
        public string ShortText { get; set; }
        public TKey AuthorId { get; set; }
        public DateTime AddTimeUtc { get; set; }
        public DateTime PublishTimeUtc { get; set; }
        public int State { get; set; }
        public bool IsIndexed { get; set; }
        public bool NeverIndex { get; set; }
        public int CommentsCount { get; set; }
        public int ViewsCount { get; set; }
        

    }
}
