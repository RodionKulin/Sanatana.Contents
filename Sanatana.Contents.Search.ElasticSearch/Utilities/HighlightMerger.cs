using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using Sanatana.Contents.Search.ElasticSearch.Objects.Settings;

namespace Sanatana.Contents.Search.ElasticSearch.Utilities
{
    public class HighlightMerger<TKey> : IHighlightMerger
        where TKey : struct
    {
        //fields
        protected Dictionary<Type, List<PropertyInfo>> _highlitedProperties;
        protected ElasticSettings<TKey> _settings;
        protected ISearchUtilities _utilities;


        //init
        public HighlightMerger(ElasticSettings<TKey> settings, ISearchUtilities utilities)
        {
            _settings = settings;
            _utilities = utilities;
            _highlitedProperties = new Dictionary<Type, List<PropertyInfo>>();
        }


        //methods
        public virtual List<IElasticSearchable> Merge<T>(List<IElasticSearchable> documents, List<IHit<T>> hits)
            where T : class
        {
            var results = new List<IElasticSearchable>();

            for (int i = 0; i < hits.Count; i++)
            {
                HighlightFieldDictionary highlights = hits[i].Highlights;
                IElasticSearchable document = documents[i];

                Type docType = document.GetType();
                ElasticEntitySettings entitySettings = _utilities.FindSettings(docType);

                List<PropertyInfo> textProperties = GetHighlitableProperties(entitySettings.IndexType);
                foreach (PropertyInfo property in textProperties)
                {
                    SetHighlightedField(property, highlights, document);
                }

                results.Add(document);
            }

            return results;
        }

        public virtual List<T> Merge<T>(List<T> documents, List<IHit<T>> hits)
            where T : class
        {
            var results = new List<T>();

            for (int i = 0; i < hits.Count; i++)
            {
                HighlightFieldDictionary highlights = hits[i].Highlights;
                T document = documents[i];

                Type docType = document.GetType();
                ElasticEntitySettings entitySettings = _settings.EntitySettings
                    .First(x => x.IndexType == docType);

                List<PropertyInfo> textProperties = GetHighlitableProperties(entitySettings.IndexType);
                foreach (PropertyInfo property in textProperties)
                {
                    SetHighlightedField(property, highlights, document);
                }

                results.Add(document);
            }

            return results;
        }

        protected virtual List<PropertyInfo> GetHighlitableProperties(Type type)
        {
            if(_highlitedProperties.ContainsKey(type))
            {
                return _highlitedProperties[type];
            }

            List<PropertyInfo> textProperties = type
                .GetProperties()
                .Where(x => x.PropertyType == typeof(string))
                .ToList();

            _highlitedProperties[type] = textProperties;
            return textProperties;
        }

        protected virtual void SetHighlightedField(PropertyInfo property
            , HighlightFieldDictionary fieldHighlights, object target)
        {
            string propertyName = property.Name;
            if (fieldHighlights.ContainsKey(propertyName) == false)
            {
                return;
            }

            string value = fieldHighlights[propertyName].Highlights.First();
            property.SetValue(target, value, null);
        }


    }
}
