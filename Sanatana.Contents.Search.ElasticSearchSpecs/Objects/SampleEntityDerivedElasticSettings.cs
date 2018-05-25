using AutoMapper;
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
    public class SampleEntityDerivedElasticSettings : ElasticEntitySettings<SampleEntityDerived, SampleEntityDerivedIndexed>
    {

        //init
        public SampleEntityDerivedElasticSettings()
        {
            this.ElasticTypeName = "SampleEntityDerived";
        }

        //methods
        public override void ApplyInferMappingSettings(ConnectionSettings connection, string defaultIndexName)
        {
            connection.InferMappingFor<SampleEntityIndexed>(typeMapping => typeMapping
              .IndexName(defaultIndexName)
              .TypeName(ElasticTypeName)
              .IdProperty(x => x.IntNotNullProperty)
              .Ignore(x => x.IntProperty)
              .Rename(x => x.StringProperty, "RenamedField2")
              .Rename(x => x.ContentId, "ReContentID2")
            );
        }

        public override void ApplyAutomapperSettings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<SampleEntityIndexed, SampleEntity>();

            configuration.CreateMap<SampleEntity, SampleEntityIndexed>();
        }

        public override void ApplyElasticFieldMapping(MappingsDescriptor mappings, string defaultAnalyzer)
        {
            mappings.Map<SampleEntityIndexed>(map => map
                .AllField(allField => allField
                    .Enabled(false)
                )
                .Properties(properties => properties
                    .Completion(field => field
                        .Name(p => p.Suggest)
                        .Analyzer(defaultAnalyzer)
                    )
                    .Text(field => field
                        .Name(name => name.AllText)
                        .Analyzer(defaultAnalyzer)
                        .Index(true)
                    )
                    .Text(field => field
                        .Name(name => name.StringProperty)
                        .Analyzer(defaultAnalyzer)
                        .Index(false)
                        .CopyTo(copy => copy.Field(f => f.AllText))
                    )
                    .Keyword(field => field
                        .Name(name => name.GuidProperty)
                        .Index(true)
                    )
                    .Keyword(field => field
                        .Name(name => name.GuidNullableProperty)
                        .Index(true)
                    )
                    .Date(field => field
                        .Name(name => name.DateProperty)
                        .Index(false)
                    )
                    .Keyword(field => field
                        .Name(name => name.ElasticTypeName)
                        .Index(false)
                    )
                    .Keyword(field => field
                        .Name(name => name.ContentId)
                        .Index(true)
                    )
                )
            );
        }
    }
}
