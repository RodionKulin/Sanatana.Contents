using Nest;
using System;

namespace Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest.FieldTypes
{
    public interface IElasticFieldTypeProvider
    {
        string GetElasticFieldName(Type indexedType, string entityMemberName);
        ElasticQueryType GetElasticFieldType(Type indexedType, string entityMemberName);
    }
}