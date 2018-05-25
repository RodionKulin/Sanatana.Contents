using AutoMapper;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Dynamic;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;

namespace Sanatana.Contents.Search.ElasticSearch.Utilities
{
    public class ElasticSearchAutoMapperFactory<TKey> : IElasticSearchAutoMapperFactory
        where TKey : struct
    { 
        //fields
        protected Lazy<IMapper> _mapper;
        protected ElasticSettings<TKey> _settings;

        //init
        public ElasticSearchAutoMapperFactory(ElasticSettings<TKey> settings)
        {
            _mapper = new Lazy<IMapper>(Create);
            _settings = settings;
        }


        //methods
        public virtual IMapper GetMapper()
        {
            return _mapper.Value;
        }

        protected virtual IMapper Create()
        {
            var configuration = new MapperConfiguration(Configure);
            return configuration.CreateMapper();
        }

        protected virtual void Configure(IMapperConfigurationExpression conf)
        {
            foreach (ElasticEntitySettings entity in _settings.EntitySettings)
            {
                entity.ApplyAutomapperSettings(conf);
            }
        }
    }
}
