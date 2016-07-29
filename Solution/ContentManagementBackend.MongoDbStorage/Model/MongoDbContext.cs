using Common.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.MongoDbStorage
{
    public class MongoDbContext
    {
        //поля
        private static bool _isMapped = false;
        private static object _mapLock = new object();
        private IMongoDatabase _database;


        //свойства
        public IMongoCollection<Category<ObjectId>> Categories
        {
            get
            {
                return _database.GetCollection<Category<ObjectId>>("Categories");
            }
        }
        public IMongoCollection<ContentBase<ObjectId>> Posts
        {
            get
            {
                return _database.GetCollection<ContentBase<ObjectId>>("Posts");
            }
        }
        public IMongoCollection<Comment<ObjectId>> Comments
        {
            get
            {
                return _database.GetCollection<Comment<ObjectId>>("Comments");
            }
        }
        public IMongoCollection<CommentVote<ObjectId>> CommentVotes
        {
            get
            {
                return _database.GetCollection<CommentVote<ObjectId>>("CommentVotes");
            }
        }
        public IMongoCollection<Keyword<ObjectId>> Keywords
        {
            get
            {
                return _database.GetCollection<Keyword<ObjectId>>("Keywords");
            }
        }



        //инициализация
        public MongoDbContext(MongoDbConnectionSettings settings)
        {
            _database = GetDatabase(settings);

            lock (_mapLock)
            {
                if (!_isMapped)
                {
                    _isMapped = true;
                    MapSerialization();
                    MapEntities();
                }
            }
        }


        //методы
        public static void ApplyGlobalSerializationSettings()
        {
            //сериализаторы
            var dateSerializer = new DateTimeSerializer(DateTimeKind.Utc);
            BsonSerializer.RegisterSerializer(typeof(DateTime), dateSerializer);

            //проверка Id
            BsonSerializer.UseNullIdChecker = true;
            BsonSerializer.UseZeroIdChecker = true;
        }

        protected virtual IMongoDatabase GetDatabase(MongoDbConnectionSettings settings)
        {
            var clientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress(settings.Host, settings.Port),
                WriteConcern = WriteConcern.Acknowledged,
                ReadPreference = ReadPreference.PrimaryPreferred
            };

            if (!string.IsNullOrEmpty(settings.Login) && !string.IsNullOrEmpty(settings.Password))
            {
                string authDatabase = string.IsNullOrEmpty(settings.AuthSource) ? settings.DatabaseName : settings.AuthSource;

                clientSettings.Credentials = new[]
                {
                    MongoCredential.CreateMongoCRCredential(authDatabase, settings.Login, settings.Password)
                };
            }
            
            MongoClient client = new MongoClient(clientSettings);
            return client.GetDatabase(settings.DatabaseName);
        }

        protected virtual void MapSerialization()
        {
            //соглашения
            var pack = new ConventionPack();
            pack.Add(new EnumRepresentationConvention(BsonType.Int32));
            pack.Add(new IgnoreIfNullConvention(true));
            pack.Add(new IgnoreIfDefaultConvention(false));

            Assembly thisAssembly = typeof(MongoDbContext).Assembly;
            Assembly entitiesAssembly = typeof(Post<>).Assembly;
            ConventionRegistry.Register("ContentManagementBackend custom pack",
                pack,
                t => t.Assembly == thisAssembly || t.Assembly == entitiesAssembly);
        }

        protected virtual void MapEntities()
        {
            BsonClassMap.RegisterClassMap<Category<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.CategoryID));
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<ContentBase<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.ContentID));
            });

            BsonClassMap.RegisterClassMap<Post<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.ContentID));
            });
            
            BsonClassMap.RegisterClassMap<YoutubePost<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.ContentID));
            });

            BsonClassMap.RegisterClassMap<Comment<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.CommentID));
            });

            BsonClassMap.RegisterClassMap<CommentVote<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.CommentID));
            });

            BsonClassMap.RegisterClassMap<Keyword<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.KeywordID));
            });

            BsonClassMap.RegisterClassMap<ContentCategoryGroup<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.CategoryID));
                cm.SetIgnoreExtraElements(true);
            });

            
        }

    }
}
