using ContentManagementBackend.MongoDbStorage;
using Common.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Initializer;

namespace ContentManagementBackend.InitializerModules
{
    public class ESInsertDataModule : IInitializeModule
    {
        //поля
        private ISearchQueries<ObjectId> _searchQueries;
        private MongoDbContext _context;


        //события
        public event ProgressDelegate ProgressUpdated;


        //инициализация
        public ESInsertDataModule(ISearchQueries<ObjectId> searchQueries, MongoDbConnectionSettings mongoSettings)
        {
            _searchQueries = searchQueries;
            _context = new MongoDbContext(mongoSettings);

        }


        //методы
        public string IntroduceSelf()
        {
            return "Add all posts to search index";
        }

        public async Task Execute()
        {
            IAsyncCursor<ContentBase<ObjectId>> cursor = await _context.Posts.FindAsync(p => true);

            while (await cursor.MoveNextAsync())
            {
                List<ContentBase<ObjectId>> content = cursor.Current.ToList()
                    .Where(p => p.IsIndexed)
                    .ToList();

                if(content.Count > 0)
                {
                    bool indexed = await _searchQueries.Insert(content);
                }
            }
        }
    }
}
