using MongoDB.Bson;
using Sanatana.Contents.Database.MongoDb;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Database.MongoDb.Queries;
using Sanatana.Contents.Database.MongoDbSpecs.TestTools.Interfaces;
using Sanatana.Contents.Objects.Entities;
using Sanatana.MongoDb;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database.MongoDbSpecs.TestTools.Providers
{
    public class DependenciesProvider : Behavior<INeedMongoDbContext>
    {
        public override void SpecInit(INeedMongoDbContext instance)
        {
            var settings = new MongoDbConnectionSettings
            {
                DatabaseName = "SanatanaContentsSpecs",
                Host = "localhost",
                Port = 27017,
            };

            instance.MockContainer.Configure(cfg =>
            {
                cfg.For<MongoDbConnectionSettings>().Use(settings);
                cfg.For<EntitiesDatabaseNameMapping>().Use<EntitiesDatabaseNameMapping>()
                    .SelectConstructor(() => new EntitiesDatabaseNameMapping());
                cfg.For<IContentMongoDbContext>().Use<ContentMongoDbContext>();
                cfg.For<MongoDbCommentQueries<Content<ObjectId>, Comment<ObjectId>>>().Use<MongoDbCommentQueries<Content<ObjectId>, Comment<ObjectId>>>();
            });

            instance.MongoDbContext = instance.MockContainer.GetInstance<IContentMongoDbContext>();
        }
    }
}
