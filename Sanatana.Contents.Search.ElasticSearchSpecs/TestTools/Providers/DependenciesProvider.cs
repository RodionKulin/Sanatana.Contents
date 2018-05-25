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
using Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest.FieldTypes;
using Sanatana.Contents.Search.ElasticSearch.Utilities;
using Sanatana.Contents.Search.ElasticSearch.Connection;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Providers
{
    public class DependenciesProvider : Behavior<INeedDependencies>
    {
        //methods
        public override void SpecInit(INeedDependencies instance)
        {
            ElasticSettings<long> settings = new ElasticSettings<long>()
            {
                NodeUri = new Uri("http://127.0.0.1:9200"),
                DefaultIndexName = "test-default",
                IsDebugMode = true,
                Username = "elastic",
                Password = "changeme"
            };
            settings.EntitySettings.Add(new SampleEntityElasticSettings());

            instance.MockContainer.Configure(cfg =>
            {
                cfg.For<ElasticSettings<long>>().Use(context => settings);
                cfg.For<IElasticClientFactory>().Use<ElasticClientFactory<long>>();
                cfg.For<IElasticSearchAutoMapperFactory>().Use<ElasticSearchAutoMapperFactory<long>>();
                cfg.For<ISearchUtilities>().Use<SearchUtilities<long>>();
                cfg.For<IHighlightMerger>().Use<HighlightMerger<long>>();
                cfg.For<IElasticFieldTypeProvider>().Use<ElasticFieldTypeProvider<long>>();
                cfg.For<IExpressionsToNestConverter>().Use<ExpressionsToNestConverter>();
            });
        }
    }
}
