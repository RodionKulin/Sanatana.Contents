using AutoMapper;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearch.Utilities
{
    public class SearchUtilities<TKey> : ISearchUtilities
        where TKey : struct
    {
        //fields
        protected ElasticSettings<TKey> _settings;
        protected IMapper _mapper;


        //init
        public SearchUtilities(ElasticSettings<TKey> settings, IElasticSearchAutoMapperFactory searchMapperFactory)
        {
            _settings = settings;
            _mapper = searchMapperFactory.GetMapper();
        }



        //methods
        public virtual string CleanInput(string input)
        {
            if (input == null)
            {
                return null;
            }

            input = Regex.Replace(input, @"[^ \w]", " "); //remove all except whitespaces and alphanumerical chars
            input = Regex.Replace(input, @"\s+", " ");  //remove all but one chained whitespaces
            return input;
        }

        public virtual string LimitLength(string input, int limit)
        {
            return string.IsNullOrEmpty(input)
                ? string.Empty
                : string.Concat(input.Take(limit));
        }

        public virtual int ToSkipNumber(int page, int itemsPerPage)
        {
            if (itemsPerPage < 1)
            {
                throw new Exception($"Number of {nameof(itemsPerPage)} must be greater then 0.");
            }
                
            if (page < 1)
            {
                page = 1;
            }

            return (page - 1) * itemsPerPage;
        }

        public virtual ElasticEntitySettings FindSettings(Type indexItemType)
        {
            ElasticEntitySettings entitySettings = _settings.EntitySettings
               .FirstOrDefault(x => x.IndexType == indexItemType);

            if (indexItemType.GetInterface(nameof(IElasticSearchable)) == null)
            {
                throw new NotImplementedException($"Type of item does not implement {nameof(IElasticSearchable)}.");
            }
            if (entitySettings == null)
            {
                throw new NotImplementedException($"Type {indexItemType.FullName} does not have an {nameof(ElasticEntitySettings)} associated.");
            }

            return entitySettings;
        }

        public virtual List<IElasticSearchable> MapJsonToIndexItems(IEnumerable<dynamic> documents)
        {
            var results = new List<IElasticSearchable>();
            JsonSerializer serializer = new JsonSerializer();

            foreach (dynamic doc in documents)
            {
                JObject jObject = (JObject)doc;
                string typePropertyName = nameof(IElasticSearchable.ElasticTypeName);
                string elasticTypeName = (string)jObject[typePropertyName];

                ElasticEntitySettings entitySettings = _settings.EntitySettings
                    .FirstOrDefault(x => x.ElasticTypeName == elasticTypeName);
                if (entitySettings == null)
                {
                    throw new NotImplementedException($"{nameof(IElasticSearchable.ElasticTypeName)} {elasticTypeName} is not registered in any of the {nameof(ElasticEntitySettings)}.");
                }

                IElasticSearchable indexItem = (IElasticSearchable)Activator.CreateInstance(entitySettings.IndexType);
                serializer.Populate(jObject.CreateReader(), indexItem);

                results.Add(indexItem);
            }

            return results;
        }

        public virtual List<IElasticSearchable> MapEntitiesToIndexItems(IEnumerable<object> items)
        {
            var indexItems = new List<IElasticSearchable>();

            foreach (object item in items)
            {
                Type itemType = item.GetType();
                ElasticEntitySettings entitySettings = _settings.EntitySettings
                    .FirstOrDefault(x => x.EntityType == itemType);
                if (entitySettings == null)
                {
                    throw new NotImplementedException($"Type {itemType.FullName} does not have an {nameof(ElasticEntitySettings)} associated.");
                }

                IElasticSearchable indexItem = (IElasticSearchable)_mapper.Map(item
                    , entitySettings.EntityType, entitySettings.IndexType);
                indexItems.Add(indexItem);
            }
            
            return indexItems;
        }

        public virtual List<object> MapIndexItemsToEntities(IEnumerable<IElasticSearchable> documents)
        {
            var results = new List<object>();

            foreach (IElasticSearchable doc in documents)
            {
                Type docType = doc.GetType();
                ElasticEntitySettings entitySettings = FindSettings(docType);
                object entity = _mapper.Map(doc, entitySettings.IndexType, entitySettings.EntityType);
                results.Add(entity);
            }

            return results;
        }

    }
}
