using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search.ElasticSearch.Objects.Settings
{
    public class ElasticSettings<TKey>
        where TKey : struct
    {
        //properties
        public Uri NodeUri { get; set; }
        public string DefaultIndexName { get; set; } = "default";
        public string DefaultAnalyzerName { get; set; } = "default_analyzer";
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsDebugMode { get; set; } = false;
        public List<ElasticEntitySettings> EntitySettings { get; set; }


        //init
        public ElasticSettings()
        {
            EntitySettings = new List<ElasticEntitySettings>()
            {
                new ContentElasticSettings<TKey>(),
                new CommentElasticSettings<TKey>(),
                new CategoryElasticSettings<TKey>()
            };
        }

    }
}
