using System.Linq.Expressions;
using Nest;
using System;

namespace Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest
{
    public interface IExpressionsToNestConverter
    {
        QueryBase ToNestQuery(Expression expression, Type indexedType);
    }
}