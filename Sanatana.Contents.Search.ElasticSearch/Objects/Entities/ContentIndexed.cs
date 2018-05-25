using Nest;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Entities
{
    public class ContentIndexed<TKey> : Content<TKey>, IElasticSearchable
        where TKey : struct
    {
        //properties
        public string ElasticTypeName { get; set; } = ElasticTypeNames.contents.ToString();
        public CompletionField Suggest { get; set; }
        public string AllText { get; }

    }
}
