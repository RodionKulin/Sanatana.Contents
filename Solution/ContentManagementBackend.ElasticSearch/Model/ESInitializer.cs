using Nest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.ElasticSearch
{
    public class ESInitializer<TKey>
        where TKey : struct
    {
        //поля
        protected static string _analyzerKey = "morphology_analyzer";
        protected static string _ruStopwordsKey = "ru_stopwords";
        protected static string _enStopwordsKey = "en_stopwords";
        protected ElasticClient _client;
        protected ESSettings _settings;


        //инициализация
        public ESInitializer(ESSettings settings)
        {
            _settings = settings;
            _settings.PostIndexName = _settings.PostIndexName.ToLowerInvariant();
            _client = CreateElasticClient(settings);
        }
        protected virtual ElasticClient CreateElasticClient(ESSettings settings)
        {
            ConnectionSettings connection = new ConnectionSettings(
                settings.NodeUri, settings.PostIndexName);
            ElasticClient client = new ElasticClient(connection);
            return client;
        }


        //методы
        public virtual void DeleteIndex()
        {
            _client.DeleteIndex(new DeleteIndexRequest(new IndexNameMarker()
            {
                Name = _settings.PostIndexName
            }));
        }

        public virtual void CreateIndex(int numberOfReplicas, int numberOfShards)
        {
            RootObjectMapping objectMapping = CreateMapping();
            AnalyzerBase analyzer = CreateAnalyzer();

            _client.CreateIndex(c => c
                .NumberOfReplicas(numberOfReplicas)
                .NumberOfShards(numberOfShards)
                .Index(_settings.PostIndexName)
                .AddMapping<ContentIndexed<TKey>>(m => m
                    .InitializeUsing(objectMapping)
                )
                .Analysis(analysis => analysis
                    .Analyzers(analyzers => analyzers
                        .Add(_analyzerKey, analyzer)
                    )
                    .TokenFilters(filters => filters
                        .Add(_enStopwordsKey, new StopTokenFilter
                        {
                            Stopwords = new List<string>() { "_english_" }
                        })
                        .Add(_ruStopwordsKey, new StopTokenFilter
                        {
                            Stopwords = new List<string>() { "_russian_" }
                        })
                    )
                )
                .AddWarmer(wd => wd
                    .Type<ContentIndexed<TKey>>()
                    .WarmerName("warmer_" + _settings.PostIndexName)
                    .Search<ContentIndexed<TKey>>(s => s
                        .Query(q => 
                            q.Term(p => p.FullContent, "warmer-value")
                            && q.Term(ct => ct.CategoryID, "warmer-value")
                        )
                    )
                )
            );
        }

        protected virtual RootObjectMapping CreateMapping()
        {
            var objectMappings = new RootObjectMapping
            {
                Properties = new Dictionary<PropertyNameMarker, IElasticType>(),
                AllFieldMapping = new AllFieldMapping()
                {
                    Enabled = false
                },
            };
            
            objectMappings.Properties.Add(
                ReflectionUtility.GetCamelCaseName<ContentIndexed<TKey>>(p => p.CategoryID)
                , new StringMapping
            {
                Index = FieldIndexOption.NotAnalyzed,
                Store = true
            });
            objectMappings.Properties.Add(
                ReflectionUtility.GetCamelCaseName<ContentIndexed<TKey>>(p => p.Title)
                , new StringMapping
            {
                Index = FieldIndexOption.Analyzed,
                Store = false,
                Analyzer = _analyzerKey
            });
            objectMappings.Properties.Add(
                ReflectionUtility.GetCamelCaseName<ContentIndexed<TKey>>(p => p.FullContent)
                , new StringMapping
            {
                Index = FieldIndexOption.Analyzed,
                Store = false,
                Analyzer = _analyzerKey
            });

            return objectMappings;
        }

        protected virtual AnalyzerBase CreateAnalyzer()
        {
            return new CustomAnalyzer()
            {
                CharFilter = new List<string>() { },
                Tokenizer = "standard",
                Filter = new List<string>()
                {
                    "lowercase", "russian_morphology", "english_morphology"
                    , _enStopwordsKey, _ruStopwordsKey
                }
            };
        }

       
    }
}
