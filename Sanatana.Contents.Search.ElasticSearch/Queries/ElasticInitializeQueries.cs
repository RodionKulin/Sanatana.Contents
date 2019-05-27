using Nest;
using Sanatana.Contents.Search.ElasticSearch.Connection;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearch.Queries
{
    public class ElasticInitializeQueries<TKey> : IElasticInitializeQueries
        where TKey : struct
    {
        //fields
        protected ElasticClient _client;
        protected ElasticSettings<TKey> _settings;


        //init
        public ElasticInitializeQueries(
            ElasticSettings<TKey> settings, IElasticClientFactory elasticClientFactory)
        {
            _settings = settings;
            _client = elasticClientFactory.GetClient();
        }


        //methods
        public virtual void DropIndex()
        {
            string indexName = _settings.DefaultIndexName.ToLowerInvariant();

            IExistsResponse existsResponse = _client.IndexExists(indexName);
            if (existsResponse.Exists)
            {
                IDeleteIndexResponse deleteResponse = _client.DeleteIndex(indexName);
            }
        }

        public virtual void CreateIndex(int numberOfShards, int numberOfReplicas)
        {
            string indexName = _settings.DefaultIndexName.ToLowerInvariant();

            ICreateIndexResponse response = _client.CreateIndex(indexName, index => index
                .Settings(settings => settings
                    .NumberOfShards(numberOfShards)
                    .NumberOfReplicas(numberOfReplicas)
                    .Analysis(ConfigureAnalysis)
                )
                .Mappings(mappings =>
                {
                    foreach (ElasticEntitySettings entity in _settings.EntitySettings)
                    {
                        entity.ApplyElasticFieldMapping(mappings, _settings.DefaultAnalyzerName);
                    }
                    return mappings;
                })
            );
        }

        protected virtual AnalysisDescriptor ConfigureAnalysis(AnalysisDescriptor analysis)
        {
            return analysis
                .TokenFilters(tokenfilters => tokenfilters
                    .Stop("en_stopwords",
                        stopTokenFilter => stopTokenFilter.StopWords("_english_"))
                )
                .Analyzers(analyzers => analyzers
                    .Custom(_settings.DefaultAnalyzerName, customAnalyzer => customAnalyzer
                        .CharFilters(new List<string>())
                        .Tokenizer("standard")
                        .Filters(new List<string>()
                        {
                            "lowercase",
                            "en_stopwords"
                        }
                    )
                )
            );
        }

    }
}
