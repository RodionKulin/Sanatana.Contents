using Sanatana.Contents.Search.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using AutoMapper;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;

namespace Sanatana.Contents.Search.ElasticSearchSpecs.TestTools.Objects
{
    public class SampleEntityElasticSettings : ElasticEntitySettings<SampleEntity, SampleEntityIndexed>
    {

        //init
        public SampleEntityElasticSettings()
        {
            this.ElasticTypeName = "SampleEntity";
        }

        //methods
        public override void ApplyInferMappingSettings(ConnectionSettings connection, string defaultIndexName)
        {
            connection.InferMappingFor<SampleEntityIndexed>(typeMapping => typeMapping
              .IndexName(defaultIndexName)
              .TypeName(ElasticTypeName)
              .IdProperty(x => x.IntNotNullProperty)
              .Ignore(x => x.IntProperty)
              .Rename(x => x.StringProperty, "RenamedField")
              .Rename(x => x.ContentId, "ReContentId")
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
                    .Number(field => field
                        .Name(name => name.IntNotNullProperty)
                        .Index(true)
                    )
                )
            );
        }
    }
}
