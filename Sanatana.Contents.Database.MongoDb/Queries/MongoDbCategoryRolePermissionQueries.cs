using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Utilities;
using Sanatana.Contents.Database.MongoDb.Context;

namespace Sanatana.Contents.Database.MongoDb.Queries
{
    public class MongoDbCategoryRolePermissionQueries : ICategoryRolePermissionQueries<ObjectId>
    {
        //fields
        protected IContentMongoDbContext _mongoDbContext;
        protected IMongoCollection<CategoryRolePermission<ObjectId>> _categoryRolePermissionCollection;



        //init
        public MongoDbCategoryRolePermissionQueries(IContentMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
            _categoryRolePermissionCollection = mongoDbContext.GetCollection<CategoryRolePermission<ObjectId>>();
        }


        //common
        protected virtual FilterDefinition<CategoryRolePermission<ObjectId>> ToFilter(
            Expression<Func<CategoryRolePermission<ObjectId>, bool>> filterConditions)
        {
            var filter = Builders<CategoryRolePermission<ObjectId>>.Filter.Where(p => true);
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            return (Expression<Func<CategoryRolePermission<ObjectId>, bool>>)visitor.Visit(filterConditions);
        }
        

        //methods
        public Task InsertMany(IEnumerable<CategoryRolePermission<ObjectId>> categoryRolePermissions)
        {
            foreach (CategoryRolePermission<ObjectId> item in categoryRolePermissions)
            {
                if (item.CategoryRolePermissionId == ObjectId.Empty)
                {
                    item.CategoryRolePermissionId = ObjectId.GenerateNewId();
                }
            }

            return _categoryRolePermissionCollection.InsertManyAsync(categoryRolePermissions);
        }

        public Task<List<CategoryRolePermission<ObjectId>>> SelectMany(
            Expression<Func<CategoryRolePermission<ObjectId>, bool>> filterConditions)
        {
            FilterDefinition<CategoryRolePermission<ObjectId>> filter = ToFilter(filterConditions);
            var options = new FindOptions();
           
            return _categoryRolePermissionCollection.Find(filter, options)
                .ToListAsync();
        }

        public async Task<long> UpdateMany(
            IEnumerable<CategoryRolePermission<ObjectId>> categoryRolePermissions)
        {
            var requests = new List<WriteModel<CategoryRolePermission<ObjectId>>>();
            
            foreach (CategoryRolePermission<ObjectId> item in categoryRolePermissions)
            {
                var filter = Builders<CategoryRolePermission<ObjectId>>.Filter.Where(
                    p => p.CategoryRolePermissionId == item.CategoryRolePermissionId);

                var update = Builders<CategoryRolePermission<ObjectId>>.Update
                    .Set(x => x.CategoryId, item.CategoryId)
                    .Set(x => x.PermissionFlags, item.PermissionFlags)
                    .Set(x => x.RoleId, item.RoleId);

                requests.Add(new UpdateOneModel<CategoryRolePermission<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });
            }

            // BulkWrite
            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };

            BulkWriteResult<CategoryRolePermission<ObjectId>> bulkResult = await _categoryRolePermissionCollection
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);
            
            return bulkResult.ModifiedCount;
        }
        
        public async Task<long> DeleteMany(
            Expression<Func<CategoryRolePermission<ObjectId>, bool>> filterConditions)
        {
            FilterDefinition<CategoryRolePermission<ObjectId>> filter = ToFilter(filterConditions);

            DeleteResult result = await _categoryRolePermissionCollection
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);

            return result.DeletedCount;
        }
    }
}
