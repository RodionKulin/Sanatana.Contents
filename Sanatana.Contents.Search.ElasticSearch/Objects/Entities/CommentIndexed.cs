using Nest;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Entities
{
    public class CommentIndexed<TKey> : Comment<TKey>, IElasticSearchable
        where TKey : struct
    {
        //properties
        public string ElasticTypeName { get; set; } = ElasticTypeNames.comments.ToString();
        public CompletionField Suggest { get; set; }
        public string AllText { get; }
    }
}
