using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.ElasticSearch
{
    public class ESSettings
    {
        //свойства
        public Uri NodeUri { get; set; }
        public string PostIndexName { get; set; }
        public int? HighlightFragmentSize { get; set; }
        public int? MaxNumSegments { get; set; }



        //инициализация
        public static ESSettings FromConfig()
        {
            string indexName = ConfigurationManager.AppSettings["ESPostIndexName"];
            string uri = ConfigurationManager.AppSettings["ESNodeUri"];
                 
            return new ESSettings()
            {
                PostIndexName = indexName.ToLowerInvariant(),
                NodeUri = new Uri(uri),
                HighlightFragmentSize = TryGetValue<int>("ESHighlightFragmentSize"),
                MaxNumSegments = TryGetValue<int>("ESMaxNumSegments")
            };

        }
        protected static T? TryGetValue<T>(string key)
            where T :struct
        {
            string stringValue = ConfigurationManager.AppSettings[key];
            
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (!converter.IsValid(stringValue))
            {
                return null;
            }

            return (T)converter.ConvertFromInvariantString(stringValue);
        }
        
    }
}
