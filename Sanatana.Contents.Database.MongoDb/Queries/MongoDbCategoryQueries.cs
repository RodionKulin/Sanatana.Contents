using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Utilities;

namespace Sanatana.Contents.Database.MongoDb.Queries
{
    public class MongoDbCategoryQueries<TCategory> : ICategoryQueries<ObjectId, TCategory>
        where TCategory : Category<ObjectId>
    {
        //fields
        protected IContentMongoDbContext _mongoDbContext;
        protected IMongoCollection<TCategory> _categoryCollection;


        //init
        public MongoDbCategoryQueries(IContentMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
            _categoryCollection = mongoDbContext.GetCollection<TCategory>();
        }



        //common
        protected virtual FilterDefinition<TCategory> ToFilter(
            Expression<Func<TCategory, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            var visitedCondition = (Expression<Func<TCategory, bool>>)visitor.Visit(filterConditions);
            return Builders<TCategory>.Filter.Where(visitedCondition);
        }
        

        //methods
        public virtual Task InsertMany(IEnumerable<TCategory> categories)
        {
            foreach (TCategory category in categories)
            {
                category.CategoryId = ObjectId.GenerateNewId();
            }

            return _categoryCollection.InsertManyAsync(categories);
        }

        public virtual Task<List<TCategory>> SelectMany(
            Expression<Func<TCategory, bool>> filterConditions)
        {
            FilterDefinition<TCategory> filter = ToFilter(filterConditions);

            var options = new FindOptions();
            return _categoryCollection.Find(filter, options)
                .ToListAsync();
        }

        public async Task<long> UpdateMany(IEnumerable<TCategory> categories,
            params Expression<Func<TCategory, object>>[] propertiesToUpdate)
        {
            if(propertiesToUpdate.Count() == 0
                || categories.Count() == 0)
            {
                return 0;
            }

            var requests = new List<WriteModel<TCategory>>();

            foreach (TCategory item in categories)
            {
                var filter = Builders<TCategory>.Filter.Where(
                    p => p.CategoryId == item.CategoryId);

                UpdateDefinition<TCategory> update = 
                    Builders<TCategory>.Update.Combine();

                foreach (Expression<Func<TCategory, object>> field in propertiesToUpdate)
                {
                    object value = field.Compile().Invoke(item);
                    update = update.Set(field, value);
                }
                
                requests.Add(new UpdateOneModel<TCategory>(filter, update)
                {
                    IsUpsert = false
                });
            }

            // BulkWrite
            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<TCategory> bulkResult = await _categoryCollection
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);

            return bulkResult.ModifiedCount;
        }
        
        public async Task<long> DeleteMany(
            Expression<Func<TCategory, bool>> filterConditions)
        {
            FilterDefinition<TCategory> filter = ToFilter(filterConditions);

            DeleteResult result = await _categoryCollection
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);

            return result.DeletedCount;
        }
    }
}
