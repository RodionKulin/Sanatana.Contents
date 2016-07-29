using ContentManagementBackend;
using ContentManagementBackend.AmazonS3Files;
using ContentManagementBackend.MongoDbStorage;
using Common.DataGenerator;
using Common.Utility;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;

namespace ContentManagementBackend.InitializerModules
{
    public class AvatarsModule : IInitializeModule
    {
        //поля
        private AvatarImageQueries _avatarQueries;


        //события
        public event ProgressDelegate ProgressUpdated;



        //инициализация
        public AvatarsModule(AvatarImageQueries avatarQueries)
        {
            _avatarQueries = avatarQueries;
        }



        //методы
        public string IntroduceSelf()
        {
            return "Create default avatars";
        }

        public async Task Execute()
        {
            FileInfo[] files = ContentProvider.GetAvatarFiles();

            foreach (FileInfo file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file.FullName);
                byte[] fileBytes = File.ReadAllBytes(file.FullName);
                bool completed = await _avatarQueries.CreateStatic(fileBytes, name);
            }
        }
    }
}
