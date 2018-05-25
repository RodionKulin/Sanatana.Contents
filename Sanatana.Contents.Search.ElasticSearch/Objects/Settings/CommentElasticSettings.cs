using AutoMapper;
using Nest;
using Sanatana.Contents.Html;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Search.ElasticSearch.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Settings
{
    public class CommentElasticSettings<TKey> : ElasticEntitySettings<Comment<TKey>, CommentIndexed<TKey>>
        where TKey : struct
    {
        //init
        public CommentElasticSettings()
        {
            ElasticTypeName = ElasticTypeNames.comments.ToString();
            IdProperty = e => e.CommentId;
        }


        //methods
        public override void ApplyInferMappingSettings(ConnectionSettings connection, string defaultIndexName)
        {
            connection.InferMappingFor<CommentIndexed<TKey>>(typeMapping => typeMapping
                .IndexName(defaultIndexName)
                .TypeName(ElasticTypeName)
                .IdProperty(IdProperty)
            );
        }

        public override void ApplyAutomapperSettings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<CommentIndexed<TKey>, Comment<TKey>>();

            configuration.CreateMap<Comment<TKey>, CommentIndexed<TKey>>()
                .BeforeMap((source, target) =>
                {
                    target.Text = HtmlTagRemover.StripHtml(source.Text ?? string.Empty);
                    target.Suggest = new CompletionField
                    {
                        Input = new List<string>(target.Text.Split(' '))
                        {
                            target.Text
                        }
                    };
                })
                .ForMember(target => target.Content, conf => conf.Ignore());
        }

        public override void ApplyElasticFieldMapping(MappingsDescriptor mappings, string defaultAnalyzer)
        {
            mappings.Map<CommentIndexed<TKey>>(map => map
                .AllField(allField => allField
                    .Enabled(false)
                )
                .Dynamic(false)
                .Properties(properties => properties
                    .Completion(field => field
                        .Name(name => name.Suggest)
                        .Analyzer(defaultAnalyzer)
                    )
                    .Text(field => field
                        .Name(name => name.AllText)
                        .Analyzer(defaultAnalyzer)
                        .Index(true)
                    )
                    .Text(field => field
                        .Name(name => name.Text)
                        .Analyzer(defaultAnalyzer)
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
                        .Name(name => name.CommentId)
                        .Index(true)
                    )
                )
            );
        }
    }
}
