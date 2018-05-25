using System.Collections.Generic;
using Nest;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;

namespace Sanatana.Contents.Search.ElasticSearch.Utilities
{
    public interface IHighlightMerger
    {
        List<IElasticSearchable> Merge<T>(List<IElasticSearchable> documents, List<IHit<T>> hits) 
            where T : class;
    }
}