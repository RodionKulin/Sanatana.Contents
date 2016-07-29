using ContentManagementBackend.ElasticSearch;
using ContentManagementBackend.MongoDbStorage;
using Common.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentManagementBackend.AmazonS3Files;

namespace ContentManagementBackend.InitializerModules
{
    public static class StringExtensions
    {
        public static string ToDetailsString(this MongoDbConnectionSettings settings)
        {
            return string.Format("MongoDb host:{0} port:{1} db:{2}"
                , settings.Host, settings.Port, settings.DatabaseName);
        }

        public static string ToDetailsString(this ESSettings settings)
        {
            return string.Format("ElasticSearch uri:{0} index:{1}"
                , settings.NodeUri, settings.PostIndexName);
        }

        public static string ToDetailsString(this AmazonS3Settings settings)
        {
            return string.Format("AmazonS3 bucket:{0} domain:{1}"
                , settings.BucketName, settings.BucketDomain);
        }

    }
}
