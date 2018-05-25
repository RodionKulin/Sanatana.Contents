using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Settings
{
    public interface IElasticSearchable
    {
        string ElasticTypeName { get; set; }
        long Version { get; set; }
        CompletionField Suggest { get; set; }
        string AllText { get; }
    }
}
