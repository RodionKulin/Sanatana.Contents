using AutoMapper;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Settings
{
    public abstract class ElasticEntitySettings
    {
        //peroperties
        public string ElasticTypeName { get; set; }
        public abstract Type IndexType { get; }
        public abstract Type EntityType { get; }

        //methods
        public abstract void ApplyInferMappingSettings(ConnectionSettings connection, string indexName);
        public abstract void ApplyAutomapperSettings(IMapperConfigurationExpression configuration);
        public abstract void ApplyElasticFieldMapping(MappingsDescriptor mappings, string defaultAnalyzer);
    }
}
