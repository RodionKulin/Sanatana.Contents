using AutoMapper;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Settings
{
    public class ElasticEntitySettings<TEntity, TIndex> : ElasticEntitySettings
        where TEntity : class
        where TIndex : class, IElasticSearchable
    {
        //properties
        public Expression<Func<TIndex, object>> IdProperty { get; set; }
        public Func<IMappingExpression<TIndex, TEntity>, IMappingExpression<TIndex, TEntity>> Automap { get; set; }
        public Func<IMappingExpression<TEntity, TIndex>, IMappingExpression<TEntity, TIndex>> AutomapReverse { get; set; }
        public Func<TypeMappingDescriptor<TIndex>, TypeMappingDescriptor<TIndex>> FieldTypeMappings { get; set; }


        //dependent properties
        public override Type EntityType
        {
            get
            {
                return typeof(TEntity);
            }
        }
        public override Type IndexType
        {
            get
            {
                return typeof(TIndex);
            }
        }


        //init
        public ElasticEntitySettings()
        {
            Automap = config => config;
            AutomapReverse = config => config;
        }


        //methods
        public override void ApplyInferMappingSettings(ConnectionSettings connection, string defaultIndexName)
        {
            connection.InferMappingFor<TIndex>(typeMapping => typeMapping
                .IndexName(defaultIndexName)
                .TypeName(ElasticTypeName)
                .IdProperty(IdProperty)
            );
        }

        public override void ApplyAutomapperSettings(IMapperConfigurationExpression configuration)
        {
            IMappingExpression<TIndex, TEntity> expression =
                configuration.CreateMap<TIndex, TEntity>();
            Automap.Invoke(expression);

            IMappingExpression<TEntity, TIndex> expressionReverse =
                configuration.CreateMap<TEntity, TIndex>();
            AutomapReverse.Invoke(expressionReverse);
        }

        public override void ApplyElasticFieldMapping(MappingsDescriptor mappings, string defaultAnalyzer)
        {
            mappings.Map(FieldTypeMappings);
        }

    }
}
