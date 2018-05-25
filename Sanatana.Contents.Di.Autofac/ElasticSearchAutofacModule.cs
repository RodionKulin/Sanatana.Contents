using Autofac;
using Sanatana.Contents.Search;
using Sanatana.Contents.Search.ElasticSearch;
using Sanatana.Contents.Search.ElasticSearch.Connection;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using Sanatana.Contents.Search.ElasticSearch.Queries;
using Sanatana.Contents.Search.ElasticSearch.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Di.Autofac
{
    public class ElasticSearchAutofacModule<TKey> : Module
        where TKey : struct
    {
        //fields
        private ElasticSettings<TKey> _elasticSettings;


        //init
        public ElasticSearchAutofacModule(ElasticSettings<TKey> elasticSettings)
        {
            _elasticSettings = elasticSettings;
        }


        //methods
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_elasticSettings).AsSelf().SingleInstance();
            builder.RegisterType<ElasticClientFactory<TKey>>().As<IElasticClientFactory>();

            builder.RegisterType<ElasticSearchQueries<TKey>>().As<ISearchQueries<TKey>>();
            builder.RegisterType<ElasticInitializeQueries<TKey>>().As<IElasticInitializeQueries>();

            builder.RegisterType<SearchUtilities<TKey>>().As<ISearchUtilities>();
            builder.RegisterType<ElasticSearchAutoMapperFactory<TKey>>().As<IElasticSearchAutoMapperFactory>();
            builder.RegisterType<HighlightMerger<TKey>>().As<IHighlightMerger>();

            

        }
    }
}
