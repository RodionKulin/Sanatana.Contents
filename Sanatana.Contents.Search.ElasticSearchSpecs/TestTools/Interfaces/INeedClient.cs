using Nest;
using Sanatana.Contents.Search.ElasticSearch;
using SpecsFor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Interfaces
{
    public interface INeedClient : INeedDependencies
    {
        ElasticClient Client { get; set; }
    }
}
