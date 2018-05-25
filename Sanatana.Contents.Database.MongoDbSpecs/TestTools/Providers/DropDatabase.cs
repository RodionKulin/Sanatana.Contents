using MongoDB.Bson;
using MongoDB.Driver;
using Sanatana.Contents.Database.MongoDb;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Database.MongoDbSpecs.TestTools.Interfaces;
using Sanatana.MongoDb;
using SpecsFor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database.MongoDbSpecs.TestTools.Providers
{
    public class DropDatabase : Behavior<INeedMongoDbContext>
    {
        //fields
        private static bool _isInitialized;

        //methods
        public override void SpecInit(INeedMongoDbContext instance)
        {
            if (_isInitialized)
            {
                return;
            }

            MongoDbConnectionSettings settings = instance.MockContainer.GetInstance<MongoDbConnectionSettings>();
            IContentMongoDbContext context = instance.MockContainer.GetInstance<IContentMongoDbContext>();
            context.Database.Client.DropDatabase(settings.DatabaseName);
            
            var command = new JsonCommand<BsonDocument>("{ profile: 2 }");
            BsonDocument result =  context.Database.RunCommand(command);

            var command2 = new JsonCommand<BsonDocument>("{ profile }");
            BsonDocument result2 = context.Database.RunCommand(command);

            _isInitialized = true;
        }

    }
}
