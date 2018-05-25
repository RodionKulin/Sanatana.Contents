using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest.FieldTypes
{
    public class MemberTypeMapping
    {
        public string MemberName { get; set; }
        public string ElasticName { get; set; }
        public bool Ignore { get; set; }
        public TypeName ElasticType { get; set; }
    }
}
