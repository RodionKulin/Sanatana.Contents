using ContentManagementBackend.MongoDbStorage;
using Common.Identity2_1.MongoDb;
using Common.MongoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;

namespace ContentManagementBackend.InitializerModules
{
    public class CreateMongoDbIndexModule<TUser> : IInitializeModule
        where TUser : MongoIdentityUser
    {
        //поля
        protected MongoDbConnectionSettings _connetionSettings;


        //события
        public event ProgressDelegate ProgressUpdated;


        //инициализация
        public CreateMongoDbIndexModule(MongoDbConnectionSettings connetionSettings)
        {
            _connetionSettings = connetionSettings;
        }


        //методы
        public string IntroduceSelf()
        {
            return string.Format("Create MongoDb indexes");
        }

        public Task Execute()
        {
            var initializer = new MongoDbInitializer(_connetionSettings);
            initializer.CreateAllIndexes();

            var identityInitializer = new IdentityMongoInitializer<TUser>(_connetionSettings);
            identityInitializer.CreateAllIndexes();

            return Task.FromResult(true);
        }

    }
}
