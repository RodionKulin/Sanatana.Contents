using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContentManagementBackend.MongoDbStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Common.Utility;
using Common.MongoDb;

namespace ContentManagementBackend.MongoDbStorage.Tests
{
    [TestClass()]
    public class MongoDbPostQueriesTests
    {
        [TestMethod()]
        public void InsertTest()
        {
            ICommonLogger logger = new ShoutExceptionLogger();
            MongoDbContentQueries queries = new MongoDbContentQueries(logger, MongoDbConnectionSettings.FromConfig());

            DateTime t = new DateTime(2016, 1, 1, 12, 0, 0);

            var post = new Post<ObjectId>()
            {
                ContentID = ObjectId.GenerateNewId(),
                FullContent = "full",
                Url = "url",
                PublishTimeUtc = t
            };
            QueryResult<bool> result = queries.Insert(post).Result;

            Assert.IsFalse(result.HasExceptions);
            Assert.IsTrue(result.Result);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            ICommonLogger logger = new ShoutExceptionLogger();
            MongoDbContentQueries queries = new MongoDbContentQueries(logger, MongoDbConnectionSettings.FromConfig());

            var post = new Post<ObjectId>()
            {
                ContentID = new ObjectId("5665cedd166b493c9c8f25ee"),
                FullContent = "full",
                Url = "url"
            };

            ContentUpdateResult result = queries.Update(post, "A6kdU3Qt3kWGu4P_T6Vlhw", true).Result;
            Assert.IsTrue(result == ContentUpdateResult.Success);
        }

        [TestMethod()]
        public async Task SelectLatestFromEachCategoryTest()
        {
            ICommonLogger logger = new ShoutExceptionLogger();
            MongoDbContentQueries queries = new MongoDbContentQueries(logger, MongoDbConnectionSettings.FromConfig());

            QueryResult<List<ContentCategoryGroup<ObjectId>>> result = await queries.SelectLatestFromEachCategory(5);
            Assert.IsFalse(result.HasExceptions);            
        }
        
        
    }
}