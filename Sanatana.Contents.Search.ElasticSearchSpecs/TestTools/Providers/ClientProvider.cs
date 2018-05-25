using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using StructureMap;
using SpecsFor.Configuration;
using System.IO;
using Sanatana.Contents.Search.ElasticSearch;
using Nest;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects;
using Sanatana.Contents.Search.ElasticSearch.Connection;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Providers
{
    public class ClientProvider : Behavior<INeedClient>
    {
        //methods
        public override void SpecInit(INeedClient instance)
        {
            var clientFactory = instance.MockContainer.GetInstance<IElasticClientFactory>();
            instance.Client = clientFactory.GetClient();
        }
    }
}
