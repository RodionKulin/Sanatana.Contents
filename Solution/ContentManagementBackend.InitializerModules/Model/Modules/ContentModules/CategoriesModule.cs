using ContentManagementBackend;
using ContentManagementBackend.AmazonS3Files;
using ContentManagementBackend.MongoDbStorage;
using Common.DataGenerator;
using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;

namespace ContentManagementBackend.InitializerModules
{
    //http://ckeditor.com/demo
    //http://ru.texthandler.com/?module=remove_line_breaks
    //<p><iframe allowfullscreen=\"\" frameborder=\"0\" height=\"360\" src=\"//www.youtube.com/embed/2SnQaG_L1KY\" width=\"640\"></iframe></p>

    public class CategoriesModule : IInitializeModule
    {
        //поля
        private ICategoryQueries<ObjectId> _categoryQueries;
        private CategoriesSettings _settings;


        //события
        public event ProgressDelegate ProgressUpdated;


        //инициализация
        public CategoriesModule(ICategoryQueries<ObjectId> categoryQueries, CategoriesSettings settings)
        {
            _categoryQueries = categoryQueries;
            _settings = settings;
        }

        
        //методы
        public string IntroduceSelf()
        {
            return "Create categories";
        }

        public async Task Execute()
        {
            foreach (Category<ObjectId> category in _settings.Categories)
            {
                if (category.CategoryID == ObjectId.Empty)
                    category.CategoryID = ObjectId.GenerateNewId();

                category.Url = Translit.RussianToTranslitUrl(category.Name.ToLowerInvariant());
                category.AddTimeUtc = DateTime.UtcNow;
                category.SortOrder = 0;

                bool completed = await _categoryQueries.Insert(category);
            }

        }

    
    }
}
