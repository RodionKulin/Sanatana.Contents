using Sanatana.Contents.Search.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects
{
    public class SampleEntityIndexed : SampleEntity, IElasticSearchable
    {
        public string ElasticTypeName { get; set; }
        public CompletionField Suggest { get; set; }
        public string AllText { get; }
    }
}
