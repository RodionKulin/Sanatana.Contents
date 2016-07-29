using Common.Initializer;
using ContentManagementBackend.ElasticSearch;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.InitializerModules
{
    public class ESInstallModule : IInitializeModule
    {
        //поля
        private ESSettings _settings;


        //события
        public event ProgressDelegate ProgressUpdated;


        //инициализация
        public ESInstallModule(ESSettings settings)
        {
            _settings = settings;
        }


        //методы
        public string IntroduceSelf()
        {
            return string.Format("Create ElasticSearch index");
        }

        public Task Execute()
        {
            var initializer = new ESInitializer<ObjectId>(_settings);
            initializer.DeleteIndex();
            initializer.CreateIndex(numberOfReplicas: 0, numberOfShards: 1);
            return Task.FromResult(0);
        }
    }
}
