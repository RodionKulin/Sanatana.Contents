using AutoMapper;
using Nest;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Search.ElasticSearch.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Settings
{
    public class CategoryElasticSettings<TKey> 
        : ElasticEntitySettings<Category<TKey>, CategoryIndexed<TKey>>
        where TKey : struct
    {
        //init
        public CategoryElasticSettings()
        {
            ElasticTypeName = ElasticTypeNames.categories.ToString();
            IdProperty = e => e.CategoryId;
        }


        //methods
        public override void ApplyAutomapperSettings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<CategoryIndexed<TKey>, Category<TKey>>();

            configuration.CreateMap<Category<TKey>, CategoryIndexed<TKey>>()
                .ForMember(target => target.Suggest, conf => conf.MapFrom(
                    source => new CompletionField
                    {
                        Input = new List<string>((source.Name ?? string.Empty).Split(' '))
                        {
                            source.Name
                        }
                    })
                );
        }

        public override void ApplyElasticFieldMapping(MappingsDescriptor mappings, string defaultAnalyzer)
        {
            mappings.Map<CategoryIndexed<TKey>>(map => map
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
                        .Name(name => name.Name)
                        .Analyzer(defaultAnalyzer)
                        .Index(false)
                        .CopyTo(copy => copy.Field(f => f.AllText))
                    )
                    .Keyword(field => field
                        .Name(name => name.ElasticTypeName)
                        .Index(false)
                    )
                    .Keyword(field => field
                        .Name(name => name.CategoryId)
                        .Index(true)
                    )
                    .Keyword(field => field
                        .Name(name => name.ParentCategoryId)
                        .Index(true)
                    )
                    .Keyword(field => field
                        .Name(name => name.SortOrder)
                        .Index(true)
                    )
                    .Keyword(field => field
                        .Name(name => name.Url)
                        .Index(true)
                    )
                )
            );
        }


    }
}
