using Nest;
using Sanatana.Contents.Search.ElasticSearch;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects
{
    public class SampleEntityDerivedIndexed : SampleEntityDerived, IElasticSearchable
    {
        public string ElasticTypeName { get; set; }
        public CompletionField Suggest { get; set; }
        public string AllText { get; }
    }
}
