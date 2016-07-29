using Common.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.MongoDbStorage
{
    public class MongoDbInitializer
    {
        //свойства
        public MongoDbContext Context { get; set; }


        //инициализация
        public MongoDbInitializer(MongoDbConnectionSettings settings)
        {
            Context = new MongoDbContext(settings);
        }


        //методы
        public void CreateAllIndexes()
        {
            CreatePostsIndex();
            CreateCommentsIndex();
            CreateCommentVotesIndex();
        }
        
        public void CreatePostsIndex()
        {
            //category index
            IndexKeysDefinition<ContentBase<ObjectId>> categoryIndex = Builders<ContentBase<ObjectId>>.IndexKeys
               .Ascending(p => p.PublishTimeUtc)
               .Ascending(p => p.IsPublished)
               .Ascending(p => p.CategoryID);

            //время должно быть уникальным, чтобы запрашивать продолжение листа
            CreateIndexOptions categoryOptions = new CreateIndexOptions()
            {
                Name = "PublishTimeUtc + IsPublished + CategoryID",
                Unique = false
            };
            

            //url index
            IndexKeysDefinition<ContentBase<ObjectId>> urlIndex = Builders<ContentBase<ObjectId>>.IndexKeys
               .Ascending(p => p.Url);

            CreateIndexOptions urlOptions = new CreateIndexOptions()
            {
                Name = "Url",
                Unique = true
            };

            
            IMongoCollection<ContentBase<ObjectId>> collection = Context.Posts;
            collection.Indexes.DropAllAsync().Wait();

            string categoryName = collection.Indexes.CreateOneAsync(categoryIndex, categoryOptions).Result;

            string urlName = collection.Indexes.CreateOneAsync(urlIndex, urlOptions).Result;
        }

        public void CreateCommentsIndex()
        {
            //post
            IndexKeysDefinition<Comment<ObjectId>> articleIndex = Builders<Comment<ObjectId>>.IndexKeys
                .Ascending(p => p.ContentID);
            
            CreateIndexOptions articleOptions = new CreateIndexOptions()
            {
                Name = "ContentID",
                Unique = false
            };

            //user
            IndexKeysDefinition<Comment<ObjectId>> userIndex = Builders<Comment<ObjectId>>.IndexKeys
               .Ascending(p => p.AuthorID);
            
            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "AuthorID",
                Unique = false
            };

            IMongoCollection<Comment<ObjectId>> collection = Context.Comments;
            collection.Indexes.DropAllAsync().Wait();

            string articleName = collection.Indexes.CreateOneAsync(articleIndex, articleOptions).Result;
            string userName = collection.Indexes.CreateOneAsync(userIndex, userOptions).Result;
        }

        public void CreateCommentVotesIndex()
        {
            //commentID
            IndexKeysDefinition<CommentVote<ObjectId>> index = Builders<CommentVote<ObjectId>>.IndexKeys
               .Ascending(p => p.CommentID)
               .Ascending(p => p.UserID);
            
            CreateIndexOptions options = new CreateIndexOptions()
            {
                Name = "CommentID + UserID",
                Unique = true
            };

            IMongoCollection<CommentVote<ObjectId>> collection = Context.CommentVotes;
            collection.Indexes.DropAllAsync().Wait();

            string name = collection.Indexes.CreateOneAsync(index, options).Result;

        }

        

        //Alternative
        public void CreateCommentsAddtimeIndex()
        {
            //post
            IndexKeysDefinition<Comment<ObjectId>> articleIndex = Builders<Comment<ObjectId>>.IndexKeys
                .Ascending(p => p.ContentID)
                .Ascending(p => p.AddTimeUtc);

            CreateIndexOptions articleOptions = new CreateIndexOptions()
            {
                Name = "ContentID + AddTimeUtc",
                Unique = false
            };

            //user
            IndexKeysDefinition<Comment<ObjectId>> userIndex = Builders<Comment<ObjectId>>.IndexKeys
               .Ascending(p => p.AuthorID)
               .Ascending(p => p.AddTimeUtc);

            CreateIndexOptions userOptions = new CreateIndexOptions()
            {
                Name = "AuthorID + AddTimeUtc",
                Unique = false
            };
            
            //addtime
            IndexKeysDefinition<Comment<ObjectId>> addTimeIndex = Builders<Comment<ObjectId>>.IndexKeys
               .Ascending(p => p.AddTimeUtc);

            CreateIndexOptions addTimeOptions = new CreateIndexOptions()
            {
                Name = "AddTimeUtc",
                Unique = false
            };

            IMongoCollection<Comment<ObjectId>> collection = Context.Comments;
            collection.Indexes.DropAllAsync().Wait();

            string articleName = collection.Indexes.CreateOneAsync(articleIndex, articleOptions).Result;
            string userName = collection.Indexes.CreateOneAsync(userIndex, userOptions).Result;
            string addTimeName = collection.Indexes.CreateOneAsync(addTimeIndex, addTimeOptions).Result;

        }

    }
}
