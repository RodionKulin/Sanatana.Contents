using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Nest;
using Sanatana.Contents.Html;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Search.ElasticSearch.Objects.Entities;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Settings
{
    public class ContentElasticSettings<TKey> 
        : ElasticEntitySettings<Content<TKey>, ContentIndexed<TKey>>
        where TKey : struct
    {
        //init
        public ContentElasticSettings()
        {
            ElasticTypeName = ElasticTypeNames.contents.ToString();
            IdProperty = e => e.ContentId;
        }


        //methods
        public override void ApplyInferMappingSettings(ConnectionSettings connection, string defaultIndexName)
        {
            connection.InferMappingFor<ContentIndexed<TKey>>(typeMapping => typeMapping
                .IndexName(defaultIndexName)
                .TypeName(ElasticTypeName)
                .IdProperty(IdProperty)
            );
        }

        public override void ApplyAutomapperSettings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<ContentIndexed<TKey>, Content<TKey>>();

            configuration.CreateMap<Content<TKey>, ContentIndexed<TKey>>()
                .ForMember(target => target.FullText, conf => conf.MapFrom(
                    source => HtmlTagRemover.StripHtml(source.FullText ?? string.Empty)))
                .ForMember(target => target.Suggest, conf => conf.MapFrom(
                    source => new CompletionField
                    {
                        Input = new List<string>((source.Title ?? string.Empty).Split(' '))
                        {
                            source.Title
                        },
                        Weight = source.ViewsCount
                    })
                );
        }

        public override void ApplyElasticFieldMapping(MappingsDescriptor mappings, string defaultAnalyzer)
        {
            mappings.Map<ContentIndexed<TKey>>(map => map
                  .AllField(allField => allField
                    .Enabled(false)
                  )
                  .Dynamic(false)
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
                          .Name(name => name.Title)
                          .Analyzer(defaultAnalyzer)
                          .Boost(3)
                          .Index(false)
                          .CopyTo(copy => copy.Field(f => f.AllText))
                      )
                      .Text(field => field
                          .Name(name => name.FullText)
                          .Analyzer(defaultAnalyzer)
                          .Boost(2)
                          .Index(false)
                          .CopyTo(copy => copy.Field(f => f.AllText))
                      )
                      .Keyword(field => field
                          .Name(name => name.ElasticTypeName)
                          .Index(false)
                      )
                      .Keyword(field => field
                          .Name(name => name.ContentId)
                          .Index(true)
                      )
                      .Keyword(field => field
                          .Name(name => name.CategoryId)
                          .Index(true)
                      )
                      .Date(field => field
                          .Name(name => name.PublishedTimeUtc)
                          .Index(true)
                      )
                      .Keyword(field => field
                          .Name(name => name.State)
                          .Index(true)
                      )
                  )
            );
        }
    }
}
