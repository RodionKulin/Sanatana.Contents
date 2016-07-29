using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.MongoDbStorage
{
    public class MongoDbCategoryQueries : ICategoryQueries<ObjectId>
    {
        //поля
        protected MongoDbContext _mongoContext;
        protected ICommonLogger _logger;



        //инициализация
        public MongoDbCategoryQueries(ICommonLogger logger)
        {
            _logger = logger;
        }
        public MongoDbCategoryQueries(ICommonLogger logger, MongoDbConnectionSettings settings)
        {
            _logger = logger;
            _mongoContext = new MongoDbContext(settings);
        }


        

        //методы
        public virtual async Task<bool> Insert(Category<ObjectId> category)
        {
            bool hasExceptions = false;

            try
            {
                await _mongoContext.Categories.InsertOneAsync(category);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return !hasExceptions;
        }

        public virtual async Task<QueryResult<List<Category<ObjectId>>>> Select()
        {
            bool hasExceptions = false;
            List<Category<ObjectId>> result = null;

            var filter = Builders<Category<ObjectId>>.Filter.Where(p => true);

            var options = new FindOptions()
            {
            };

            try
            {
                result = await _mongoContext.Categories.Find(filter, options)
                    .ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            if (result == null)
                result = new List<Category<ObjectId>>();

            return new QueryResult<List<Category<ObjectId>>>(result, hasExceptions);
        }

        public virtual async Task<bool> Update(Category<ObjectId> category)
        {
            bool hasExceptions = false;

            var filter = Builders<Category<ObjectId>>.Filter.Where(
                p => p.CategoryID == category.CategoryID);

            var update = Builders<Category<ObjectId>>.Update
                .Set(p => p.Name, category.Name)
                .Set(p => p.ParentCategoryID, category.ParentCategoryID);

            var options = new UpdateOptions()
            {
                IsUpsert = false
            };

            try
            {
                UpdateResult result = await _mongoContext.Categories.UpdateOneAsync(
                    filter, update, options);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return !hasExceptions;
        }

        public virtual async Task<bool> Delete(ObjectId categoryID)
        {
            bool hasExceptions = false;

            var filter = Builders<Category<ObjectId>>.Filter.Where(
                p => p.CategoryID == categoryID);
            
            try
            {
                DeleteResult result = await _mongoContext.Categories.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return !hasExceptions;
        }

    }
}
