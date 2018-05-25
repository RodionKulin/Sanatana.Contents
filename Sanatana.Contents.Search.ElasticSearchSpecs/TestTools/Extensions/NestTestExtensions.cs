using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Extensions
{
    public static class NestTestExtensions
    {
        public static QueryBase GetContainedQuery(this QueryContainer queryContainer)
        {
            PropertyInfo[] internalProperties = typeof(QueryContainer)
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            PropertyInfo containedQueryProp = internalProperties
                .First(x => x.Name == "ContainedQuery");
            QueryBase containedQuery = (QueryBase)containedQueryProp.GetValue(queryContainer);
            return containedQuery;
        }
    }
}
