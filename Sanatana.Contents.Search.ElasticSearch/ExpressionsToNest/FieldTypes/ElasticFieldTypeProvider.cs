using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Nest;
using System.Reflection;
using Elasticsearch.Net;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;
using Sanatana.Contents.Search.ElasticSearch.Connection;

namespace Sanatana.Contents.Search.ElasticSearch.ExpressionsToNest.FieldTypes
{
    /// <summary>
    /// Provides elastic field type information and field names after renaming by inferrer
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class ElasticFieldTypeProvider<TKey> : IElasticFieldTypeProvider
        where TKey : struct
    {
        //fields
        protected string[] _numericTypes = new string[]
        {
            "long", "integer", "short", "byte", "double", "float", "half_float", "scaled_float"
        };
        protected string[] _dateTypes = new string[]
        {
            "date"
        };
        protected ElasticSettings<TKey> _settings;
        protected ElasticClient _client;
        //key1: elastic type name, key2: indexed type member name
        protected Dictionary<string, Dictionary<string, MemberTypeMapping>> _mappings;
        private object _mappingsRequestLock = new object();


        //init
        public ElasticFieldTypeProvider(ElasticSettings<TKey> settings, IElasticClientFactory elasticClientFactory)
        {
            _settings = settings;
            _client = elasticClientFactory.GetClient();
        }


        //get mappings methods
        protected virtual Dictionary<string, Dictionary<string, MemberTypeMapping>> GetMappings()
        {
            if(_mappings == null)
            {
                lock (_mappingsRequestLock)
                {
                    if (_mappings == null)
                    {
                        _mappings = new Dictionary<string, Dictionary<string, MemberTypeMapping>>();

                        //request mappings stored in elastic search
                        GetElasticMappings(_mappings);

                        //add inferrer settings about members renamed and ignored
                        IConnectionSettingsValues csv = _client.ConnectionSettings;
                        foreach (KeyValuePair<MemberInfo, IPropertyMapping> inferrerMapping in csv.PropertyMappings)
                        {
                            SetInferrerSettings(_mappings, inferrerMapping);
                        }
                    }
                }
            }

            return _mappings;
        }

        protected virtual void GetElasticMappings(Dictionary<string, Dictionary<string, MemberTypeMapping>> mappings)
        {
            IGetMappingResponse response = _client.GetMapping(
                new GetMappingRequest(index: _settings.DefaultIndexName));

            foreach (IReadOnlyDictionary<string, TypeMapping> indexMapping in response.Mappings.Values)
            {
                foreach (KeyValuePair<string, TypeMapping> typeMap in indexMapping)
                {
                    var propertyMaps = new Dictionary<string, MemberTypeMapping>();
                    propertyMaps = typeMap.Value.Properties
                        .Select(x => new MemberTypeMapping
                        {
                            ElasticName = x.Key.Name,
                            ElasticType = x.Value.Type,
                            MemberName = x.Key.Name,
                        })
                        .ToDictionary(x => x.MemberName);

                    string elasticTypeName = typeMap.Key;
                    mappings.Add(elasticTypeName, propertyMaps);
                }
            }
        }

        protected virtual void SetInferrerSettings(Dictionary<string, Dictionary<string, MemberTypeMapping>> mappings,
            KeyValuePair<MemberInfo, IPropertyMapping> inferrerMapping)
        {
            string memberName = inferrerMapping.Key.Name;
            Type indexedType = inferrerMapping.Key.DeclaringType;
            string elasticName = inferrerMapping.Value.Name ?? string.Empty;
            bool ignore = inferrerMapping.Value.Ignore;

            string elasticTypeName = GetElasticTypeName(indexedType, true);

            //find by elastic type name
            bool elasticTypeExist = mappings.ContainsKey(elasticTypeName);
            if (elasticTypeExist == false)
            {
                throw new NotImplementedException(
                    $"Entity {indexedType.Name} does not have Elasticsearch mapping.");
            }
            Dictionary<string, MemberTypeMapping> typeMaps = mappings[elasticTypeName];

            //find by elastic property name
            MemberTypeMapping memberMap;
            bool elasticNameExists = typeMaps.TryGetValue(elasticName, out memberMap);
            if (elasticNameExists == false && ignore == true)
            {
                typeMaps.Add(memberName, new MemberTypeMapping
                {
                    ElasticName = elasticName,
                    ElasticType = null,
                    Ignore = ignore,
                    MemberName = memberName
                });
            }
            else if (elasticNameExists == false && ignore == false)
            {
                throw new NotImplementedException(
                    $"Entity {indexedType.Name} does not have Elasticsearch field mapping for {elasticName}.");
            }
            else if (elasticNameExists == true)
            {
                memberMap.MemberName = memberName;
                memberMap.Ignore = ignore;

                typeMaps.Add(memberName, memberMap);
                typeMaps.Remove(elasticName);
            }
        }


        //entity type name to elastic type name
        protected virtual string GetElasticTypeName(Type indexedType, bool checkBaseTypes)
        {
            ElasticEntitySettings entitySettings;
            if (checkBaseTypes)
            {
                entitySettings = _settings.EntitySettings
                    .FirstOrDefault(x =>
                    {
                        Type baseType = x.IndexType;
                        bool typeMatched = false;

                        while (baseType != null && typeMatched == false)
                        {
                            typeMatched = baseType == indexedType;
                            baseType = baseType.BaseType;
                        }

                        return typeMatched;
                    });
            }
            else
            {
                entitySettings = _settings.EntitySettings
                    .FirstOrDefault(x => x.IndexType == indexedType);
            }

            if(entitySettings == null)
            {
                throw new NotImplementedException(
                   $"Type {indexedType.Name} does not have a {nameof(ElasticEntitySettings)} configuration.");
            }

            return entitySettings.ElasticTypeName;
        }



        //entity member name to elastic field name mapping
        protected virtual MemberTypeMapping GetMemberMapping(string elasticTypeName, string entityMemberName)
        {
            Dictionary<string, Dictionary<string, MemberTypeMapping>> mappings = GetMappings();

            if(elasticTypeName == null)
            {
                throw new ArgumentException(
                   $"Parameter {elasticTypeName} can not be null.");
            }

            Dictionary<string, MemberTypeMapping> typeMap;
            bool typeMapExists = mappings.TryGetValue(elasticTypeName, out typeMap);
            if(typeMapExists == false)
            {
                throw new NotImplementedException(
                   $"Elasticseatch type {elasticTypeName} was not found.");
            }

            MemberTypeMapping memberMap;
            bool memberMapExists = typeMap.TryGetValue(entityMemberName, out memberMap);
            if (memberMapExists == false)
            {
                throw new NotImplementedException(
                    $"Elasticseatch type {elasticTypeName} does not have field representing entity {entityMemberName} member.");
            }

            if (memberMap.Ignore == true)
            {
                throw new ArgumentException(
                   $"Entity member {entityMemberName} is configured to be ignored for Elasticseatch type {elasticTypeName} but was used in expression.");
            }

            return memberMap;
        }
        
        public virtual string GetElasticFieldName(Type indexedType, string entityMemberName)
        {
            string elasticTypeName = GetElasticTypeName(indexedType, false);
            MemberTypeMapping memberMap = GetMemberMapping(elasticTypeName, entityMemberName);
            return memberMap.ElasticName;
        }

        public virtual ElasticQueryType GetElasticFieldType(Type indexedType, string entityMemberName)
        {
            string elasticTypeName = GetElasticTypeName(indexedType, false);
            MemberTypeMapping memberMap = GetMemberMapping(elasticTypeName, entityMemberName);
            return MatchFieldTypeToQueryType(memberMap.ElasticType);
        }

        protected virtual ElasticQueryType MatchFieldTypeToQueryType(TypeName elasticDataType)
        {
            if (_numericTypes.Contains(elasticDataType.Name))
            {
                return ElasticQueryType.Numeric;
            }
            else if(_dateTypes.Contains(elasticDataType.Name))
            {
                return ElasticQueryType.Date;
            }
            else
            {
                return ElasticQueryType.Term;
            }
        }

    }
}
