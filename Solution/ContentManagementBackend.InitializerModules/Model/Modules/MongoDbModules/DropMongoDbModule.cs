using ContentManagementBackend.MongoDbStorage;
using Common.MongoDb;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;

namespace ContentManagementBackend.InitializerModules
{
    public class DropMongoDbModule : IInitializeModule
    {
        //поля
        private MongoDbConnectionSettings _settings;


        //события
        public event ProgressDelegate ProgressUpdated;


        //инициализация
        public DropMongoDbModule(MongoDbConnectionSettings settings)
        {
            _settings = settings;
        }


        //методы
        public string IntroduceSelf()
        {            
            return string.Format("Drop MongoDb database");
        }
        public Task Execute()
        {
            MongoDbContext mongoContext = new MongoDbContext(_settings);
            IMongoDatabase mainDb = mongoContext.Posts.Database;
            return mainDb.Client.DropDatabaseAsync(mainDb.DatabaseNamespace.DatabaseName);
        }

    }
}
