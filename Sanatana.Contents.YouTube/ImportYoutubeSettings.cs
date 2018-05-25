using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents
{
    public class ImportYoutubeSettings<TKey>
        where TKey : struct
    {
        public TKey CategoryId { get; set; }
        public TKey AuthorId { get; set; }
        public string ChannelUrl { get; set; }
        public List<string> VideoFilterExcludeByTitle { get; set; }
        public int NewContentState { get; set; }
    }
}
