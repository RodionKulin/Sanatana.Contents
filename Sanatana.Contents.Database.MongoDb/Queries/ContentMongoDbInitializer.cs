using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Database.MongoDb.Context;

namespace Sanatana.Contents.Database.MongoDb.Queries
{
    public class ContentMongoDbInitializer
    {
        //properties
        public IContentMongoDbContext Context { get; set; }


        //init
        public ContentMongoDbInitializer(IContentMongoDbContext mongoDbContext)
        {
            Context = mongoDbContext;
        }


        //methods
        public void CreateContentIndex<TContent>()
            where TContent : Content<ObjectId>
        {
            //category index
            IndexKeysDefinition<TContent> categoryIndex = Builders<TContent>.IndexKeys
               .Ascending(p => p.PublishTimeUtc)
               .Ascending(p => p.State)
               .Ascending(p => p.CategoryId);

            //index is not unique
            //but to use continuation queries (next after some PublishTimeUtc) PublishTimeUtc must be unique
            CreateIndexOptions categoryOptions = new CreateIndexOptions()
            {
                Name = "PublishTimeUtc + State + CategoryId",
                Unique = false
            };
            

            //url index
            IndexKeysDefinition<TContent> urlIndex = Builders<TContent>.IndexKeys
               .Ascending(p => p.Url);

            CreateIndexOptions urlOptions = new CreateIndexOptions()
            {
                Name = "Url",
                Unique = true
            };


            IMongoCollection<TContent> collection = Context.GetCollection<TContent>();
            collection.Indexes.DropAll();

            string categoryName = collection.Indexes.CreateOne(categoryIndex, categoryOptions);
            string urlName = collection.Indexes.CreateOne(urlIndex, urlOptions);
        }
        
        public void CreateCommentsIndex<TComment>()
            where TComment : Comment<ObjectId>
        {
            //content
            IndexKeysDefinition<TComment> contentIndex = Builders<TComment>.IndexKeys
                .Ascending(p => p.ContentId);
            
            CreateIndexOptions contentOptions = new CreateIndexOptions()
            {
                Name = "ContentId",
                Unique = false
            };

            //user
            IndexKeysDefinition<TComment> userIndex = Builders<TComment>.IndexKeys
               .Ascending(p => p.AuthorId)
               .Ascending(p => p.AddTimeUtc);

            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "AuthorId + AddTimeUtc",
                Unique = false
            };

            IMongoCollection<TComment> collection = Context.GetCollection<TComment>();
            collection.Indexes.DropAll();

            string contentName = collection.Indexes.CreateOne(contentIndex, contentOptions);
            string userName = collection.Indexes.CreateOne(userIndex, userOptions);
        }

        
    }
}
