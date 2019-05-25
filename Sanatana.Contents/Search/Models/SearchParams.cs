using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Search
{
    public class SearchParams
    {
        public string Input { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool Highlight { get; set; }
        public int? HighlightFragmentSize { get; set; } = 400;
        public string HighlightPreTags { get; set; } = "<em>";
        public string HighlightPostTags { get; set; } = "</em>";
        public List<EntitySearchParams> TypesToSearch { get; set; }
    }
}
