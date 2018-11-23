using Sanatana.MongoDb;
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
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Objects.DTOs;
using MongoDB.Driver.Core.Events;

namespace Sanatana.Contents.Database.MongoDb.Context
{
    public class ContentMongoDbContext : IContentMongoDbContext
    {
        //fields
        private static bool _isMapped = false;
        private static object _mapLock = new object();
        protected MongoDbConnectionSettings _connectionSettings;
        protected EntitiesDatabaseNameMapping _entitiesDatabaseNameMapping;

        //properties
        public IMongoDatabase Database { get; }
        

        //init
        public ContentMongoDbContext(MongoDbConnectionSettings connectionSettings
            , EntitiesDatabaseNameMapping entitiesDatabaseNameMapping)
        {
            _connectionSettings = connectionSettings;
            _entitiesDatabaseNameMapping = entitiesDatabaseNameMapping;
            Database = GetDatabase(connectionSettings);

            if (_isMapped == false)
            {
                lock (_mapLock)
                {
                    if (_isMapped == false)
                    {
                        _isMapped = true;
                        RegisterConventions();
                        MapEntities();
                    }
                }
            }
        }


        //methods
        public static void ApplyGlobalSerializationSettings()
        {
            var dateSerializer = new DateTimeSerializer(DateTimeKind.Utc);
            BsonSerializer.RegisterSerializer(typeof(DateTime), dateSerializer);
            
            BsonSerializer.UseNullIdChecker = true;
            BsonSerializer.UseZeroIdChecker = true;
        }

        protected virtual IMongoDatabase GetDatabase(MongoDbConnectionSettings connectionSettings)
        {
            var clientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress(connectionSettings.Host, connectionSettings.Port),
                WriteConcern = WriteConcern.Acknowledged,
                ReadPreference = ReadPreference.PrimaryPreferred,
                Credential = connectionSettings.Credential
            };

            clientSettings.ClusterConfigurator = cb => {
                cb.Subscribe<CommandStartedEvent>(e => {
                    System.Diagnostics.Trace.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
                });
            };

            MongoClient client = new MongoClient(clientSettings);
            return client.GetDatabase(connectionSettings.DatabaseName);
        }

        protected virtual void RegisterConventions()
        {
            var pack = new ConventionPack();
            pack.Add(new EnumRepresentationConvention(BsonType.Int32));
            pack.Add(new IgnoreIfNullConvention(true));
            pack.Add(new IgnoreIfDefaultConvention(false));

            Assembly dalAssembly = typeof(ContentMongoDbContext).GetTypeInfo().Assembly;
            Assembly entitiesAssembly = typeof(Content<>).GetTypeInfo().Assembly;
            ConventionRegistry.Register("Content pack", pack,
                t => t.GetTypeInfo().Assembly == dalAssembly 
                || t.GetTypeInfo().Assembly == entitiesAssembly);
        }

        protected virtual void MapEntities()
        {
            BsonClassMap.RegisterClassMap<Category<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.CategoryId));
            });

            BsonClassMap.RegisterClassMap<Content<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.ContentId));
            });

            BsonClassMap.RegisterClassMap<Comment<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.CommentId));
            });

            BsonClassMap.RegisterClassMap<ContentCategoryGroupResult<ObjectId, Content<ObjectId>>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.CategoryId));
            });

            BsonClassMap.RegisterClassMap<CategoryRolePermission<ObjectId>>(cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(m => m.CategoryRolePermissionId));
            });
        }
        
        public virtual IMongoCollection<TDocument> GetCollection<TDocument>()
        {
            string collectionName = _entitiesDatabaseNameMapping.GetEntityName<TDocument>();
            IMongoCollection<TDocument> collection = Database.GetCollection<TDocument>(collectionName);
            return collection;
        }

    }
}
