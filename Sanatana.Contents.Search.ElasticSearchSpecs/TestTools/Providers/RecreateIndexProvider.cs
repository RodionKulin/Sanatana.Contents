using Nest;
using Sanatana.Contents.Search.ElasticSearch;
using Sanatana.Contents.Search.ElasticSearch.Connection;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using Sanatana.Contents.Search.ElasticSearch.Queries;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Interfaces;
using Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Providers
{
    public class RecreateIndexProvider : Behavior<INeedClient>
    {
        //fields
        private static bool _isInitialized;

        //methods
        public override void SpecInit(INeedClient instance)
        {
            if (_isInitialized)
            {
                return;
            }

            ElasticSettings<long> settings = instance.MockContainer.GetInstance<ElasticSettings<long>>();
            IElasticClientFactory clientFactory = instance.MockContainer.GetInstance<IElasticClientFactory>();
            var initializer = new ElasticInitializeQueries<long>(settings, clientFactory);

            initializer.DropIndex();
            initializer.CreateIndex(1, 0);

            _isInitialized = true;
        }

    }
}
