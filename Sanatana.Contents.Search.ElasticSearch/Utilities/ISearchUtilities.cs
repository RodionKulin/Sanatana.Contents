using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Sanatana.Contents.Search.ElasticSearch.Utilities
{
    public interface ISearchUtilities
    {
        string CleanInput(string input);
        string LimitLength(string input, int limit);
        int ToSkipNumber(int page, int itemsPerPage);
        List<IElasticSearchable> MapEntitiesToIndexItems(IEnumerable<object> items);
        List<object> MapIndexItemsToEntities(IEnumerable<IElasticSearchable> documents);
        List<IElasticSearchable> MapJsonToIndexItems(IEnumerable<dynamic> documents);
        ElasticEntitySettings FindSettings(Type indexItemType);
    }
}