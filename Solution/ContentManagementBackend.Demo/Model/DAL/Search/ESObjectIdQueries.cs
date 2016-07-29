using ContentManagementBackend.ElasticSearch;
using Common.Utility;
using MongoDB.Bson;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class ESObjectIdQueries : ESQueries<ObjectId>
    {

        //инициализация
        public ESObjectIdQueries(ICommonLogger logger)
            : base(logger)
        {
        }

        public ESObjectIdQueries(ICommonLogger logger, ESSettings settings)
            : base(logger, settings)
        {
        }

        protected override ElasticClient CreateElasticClient(ESSettings settings)
        {
            ConnectionSettings connection = new ConnectionSettings(
                settings.NodeUri, settings.PostIndexName.ToLowerInvariant());

            connection.AddContractJsonConverters(type =>
            {
                if (type == typeof(ObjectId))
                {
                    return new ObjectIdConverter();
                }

                return null;
            });
            
            return new ElasticClient(connection);
        }
    }
}