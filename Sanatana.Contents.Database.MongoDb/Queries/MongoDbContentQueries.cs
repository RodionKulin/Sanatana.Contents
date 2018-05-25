using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Utilities;

namespace Sanatana.Contents.Database.MongoDb.Queries
{
    public class MongoDbContentQueries<TContent> : IContentQueries<ObjectId, TContent>
        where TContent : Content<ObjectId>
    {
        //fields
        protected IContentMongoDbContext _mongoDbContext;
        protected IMongoCollection<TContent> _contentCollection;


        //init
        public MongoDbContentQueries(IContentMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
            _contentCollection = mongoDbContext.GetCollection<TContent>();
        }


        //common
        protected virtual FilterDefinition<TContent> ToFilter(Expression<Func<TContent, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            var visitedCondition = (Expression<Func<TContent, bool>>)visitor.Visit(filterConditions);
            return Builders<TContent>.Filter.Where(visitedCondition);
        }

        protected virtual ProjectionDefinition<TContent> ToProjection(DataAmount dataAmmount)
        {
            var projection = Builders<TContent>.Projection.Combine();
            if (dataAmmount == DataAmount.ShortContent)
            {
                projection = Builders<TContent>.Projection
                    .Exclude(p => p.FullText);
            }
            else if (dataAmmount == DataAmount.DescriptionOnly)
            {
                projection = Builders<TContent>.Projection
                    .Exclude(p => p.FullText)
                    .Exclude(p => p.ShortText);
            }

            return projection;
        }
        

        //insert
        public virtual async Task<ContentInsertResult> InsertOne(TContent content)
        {
            try
            {
                content.ContentId = ObjectId.GenerateNewId();
                await _contentCollection
                    .InsertOneAsync(content)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string urlField = FieldDefinitions.GetFieldMappedName<TContent>(x => x.Url);
                if (MongoDbUtility.IsDuplicateException(ex, urlField))
                {
                    return ContentInsertResult.UrlIsNotUnique;
                };

                string publishTimeUtcField = FieldDefinitions.GetFieldMappedName<TContent>(x => x.PublishTimeUtc);
                if (MongoDbUtility.IsDuplicateException(ex, publishTimeUtcField))
                {
                    return ContentInsertResult.PublishTimeUtcIsNotUnique;
                }

                throw ex;
            }

            return ContentInsertResult.Success;
        }

        public virtual Task InsertMany(IEnumerable<TContent> contents)
        {
            contents.ToList().ForEach(x =>
            {
                x.ContentId = ObjectId.GenerateNewId();
            });
            return _contentCollection.InsertManyAsync(contents);
        }


        //count
        public virtual Task<long> Count(Expression<Func<TContent, bool>> filterConditions)
        {
            FilterDefinition<TContent> filter = ToFilter(filterConditions);

            var options = new CountOptions()
            {
            };

            //string explain = _mongoDbContext.Content.ExplainCount(ExplainVerbosity.QueryPlanner
            //    , filter, options).Result.ToJsonIntended();
          
            return _contentCollection.CountAsync(filter, options);
        }
        

        //select
        public virtual Task<TContent> SelectOne(bool incrementViewCount
            , DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions)
        {
            FilterDefinition<TContent> filter = ToFilter(filterConditions);
            ProjectionDefinition<TContent> projection = ToProjection(dataAmmount);

            if (incrementViewCount)
            {
                var update = Builders<TContent>.Update.Inc(p => p.ViewsCount, 1);
                var options = new FindOneAndUpdateOptions<TContent>()
                {
                    IsUpsert = false,
                    ReturnDocument = MongoDB.Driver.ReturnDocument.After,
                    Projection = projection
                };

                return _contentCollection.FindOneAndUpdateAsync(
                    filter, update, options);
            }
            else
            {
                var options = new FindOptions();
                return _contentCollection
                    .Find(filter, options)
                    .Project<TContent>(projection)
                    .FirstOrDefaultAsync();
            }
        }

        public virtual Task<List<TContent>> SelectMany(
            int page, int pageSize, DataAmount dataAmmount, bool orderDescending
            , Expression<Func<TContent, bool>> filterConditions)
        {
            int skip = MongoDbUtility.ToSkipNumber(page, pageSize);
            FilterDefinition<TContent> filter = ToFilter(filterConditions);            
            ProjectionDefinition<TContent> projection = ToProjection(dataAmmount);
            var options = new FindOptions();

            //string explain = _contentCollection.ExplainFind<ContentBase<ObjectId>, ContentBase<ObjectId>>(
            //    ExplainVerbosity.QueryPlanner, filter).Result.ToJsonIntended();

            var query = _contentCollection.Find(filter, options)
                .Skip(skip)
                .Limit(pageSize)
                .Project<TContent>(projection);
                
            if (orderDescending)
            {
                query = query.SortByDescending(p => p.PublishTimeUtc);
            }
            else
            {
                query = query.SortBy(p => p.PublishTimeUtc);
            }

            return query.ToListAsync();
        }
        
        public virtual Task<List<TContent>> SelectTopViews(int pageSize
            , DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions)
        {
            FilterDefinition<TContent> filter = ToFilter(filterConditions);
            ProjectionDefinition<TContent> projection = ToProjection(dataAmmount);
            var options = new FindOptions();

            //string explain = _mongoDbContext.Content.ExplainFind<ContentBase<ObjectId>, ContentBase<ObjectId>>(
            //    ExplainVerbosity.QueryPlanner, filter).Result.ToJsonIntended();

            return _contentCollection.Find(filter, options)
                .SortByDescending(p => p.ViewsCount)
                .Limit(pageSize)
                .Project<TContent>(projection)
                .ToListAsync();
        }

        public virtual Task<List<ContentCategoryGroupResult<ObjectId, TContent>>> SelectLatestFromEachCategory(
            int eachCategoryCount, DataAmount dataAmmount
            , Expression<Func<TContent, bool>> filterConditions)
        {
            FilterDefinition<TContent> filter = ToFilter(filterConditions);
            string fullText = FieldDefinitions.GetFieldMappedName<TContent>(x => x.FullText);
            string shortText = FieldDefinitions.GetFieldMappedName<TContent>(x => x.ShortText);
            string categoryId = FieldDefinitions.GetFieldMappedName<TContent>(x => x.CategoryId);
            string contentsField = FieldDefinitions.GetFieldMappedName<ContentCategoryGroupResult<ObjectId, TContent>>(x => x.Contents);
            
            var options = new AggregateOptions()
            {
                AllowDiskUse = false
            };

            IAggregateFluent<BsonDocument> aggregateFluent = _contentCollection.Aggregate(options)
                .SortByDescending(p => p.PublishTimeUtc)
                .Match(filter)
                .Group(new BsonDocument
                {
                    {
                        "_id", "$" + categoryId
                    },
                    { contentsField, new BsonDocument("$push", "$$ROOT" ) }
                });

            BsonDocument projectStep = new BsonDocument
            {
                {
                    contentsField,
                    new BsonDocument("$slice", new BsonArray() { "$" + contentsField, 0, eachCategoryCount } )
                }
            };
            if (dataAmmount == DataAmount.ShortContent)
            {
                projectStep.AddRange(new Dictionary<string, object> {
                    {
                        contentsField ,
                        new BsonDocument(new Dictionary<string, object> {
                            {
                                fullText ,
                                0
                            }
                        })
                    }
                });
            }
            else if (dataAmmount == DataAmount.DescriptionOnly)
            {
                projectStep.AddRange(new Dictionary<string, object> {
                    {
                        contentsField ,
                        new BsonDocument(new Dictionary<string, object> {
                            {
                                fullText ,
                                0
                            },
                            {
                                shortText ,
                                0
                            }
                        })
                    }
                });
            }

            IAggregateFluent<ContentCategoryGroupResult<ObjectId, TContent>> aggregateProjected = aggregateFluent
                .Project<ContentCategoryGroupResult<ObjectId, TContent>>(projectStep);

            //List<BsonDocument> aggrStages = _mongoDbContext.Content
            //    .GetAggregateStagesBson<ContentBase<ObjectId>, BsonDocument>(aggregateFluent);
            //string explanation = _mongoDbContext.Content.ExplainAggregation(
            //    aggregateFluent, ExplainVerbosity.ExecutionStats).Result.ToJsonIntended();

            return aggregateProjected.ToListAsync();
        }



        //update
        public virtual async Task<long> UpdateMany(TContent values
            , Expression<Func<TContent, bool>> filterConditions
            , params Expression<Func<TContent, object>>[] propertiesToUpdate)
        {
            if(propertiesToUpdate.Length == 0)
            {
                return 0;
            }

            FilterDefinition<TContent> filter = ToFilter(filterConditions);

            var update = Builders<TContent>.Update.Combine();
            foreach (Expression<Func<TContent, object>> exp in propertiesToUpdate)
            {
                object value = exp.Compile().Invoke(values);
                update = update.Set(exp, value);
            }

            UpdateResult result = await _contentCollection
                .UpdateOneAsync(filter, update)
                .ConfigureAwait(false);
            return result.ModifiedCount;
        }

        public virtual async Task<OperationStatus> UpdateOne(TContent content
            , long prevVersion, bool matchVersion)
        {
            var requests = new List<WriteModel<TContent>>();

            //content update
            var filter = Builders<TContent>.Filter.Where(
                p => p.ContentId == content.ContentId);

            if (matchVersion)
            {
                filter = Builders<TContent>.Filter.And(filter
                    , Builders<TContent>.Filter.Where(p => p.Version == prevVersion));
            }

            UpdateDefinition<TContent> update = Builders<TContent>.Update
                .SetAllMappedMembers(content);

            requests.Add(new UpdateOneModel<TContent>(filter, update)
            {
                IsUpsert = false
            });


            // content id update (check if content exists)
            filter = Builders<TContent>.Filter.Where(
                p => p.ContentId == content.ContentId);

            update = Builders<TContent>.Update
                .Set(p => p.ContentId, content.ContentId);

            requests.Add(new UpdateOneModel<TContent>(filter, update)
            {
                IsUpsert = false
            });


            // BulkWrite
            var options = new BulkWriteOptions()
            {
                IsOrdered = true
            };

            BulkWriteResult<TContent> bulkResult = await _contentCollection
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);

            OperationStatus result = bulkResult.MatchedCount == 0
                    ? OperationStatus.NotFound
                : bulkResult.MatchedCount == 1
                    ? OperationStatus.VersionChanged
                    : OperationStatus.Success;

            return result;
        }

        public virtual async Task<OperationStatus> UpdateOne(TContent content, long prevVersion
            , bool matchVersion, params Expression<Func<TContent, object>>[] propertiesToUpdate)
        {
            var requests = new List<WriteModel<TContent>>();

            //content update
            var filter = Builders<TContent>.Filter.Where(
                p => p.ContentId == content.ContentId);

            if (matchVersion)
            {
                filter = Builders<TContent>.Filter.And(filter
                    , Builders<TContent>.Filter.Where(p => p.Version == prevVersion));
            }

            UpdateDefinition<TContent> update = Builders<TContent>.Update.Combine();
            foreach (Expression<Func<TContent, object>> exp in propertiesToUpdate)
            {
                object value = exp.Compile().Invoke(content);
                update = update.Set(exp, value);
            }

            requests.Add(new UpdateOneModel<TContent>(filter, update)
            {
                IsUpsert = false
            });


            // content id update (check if content exists)
            filter = Builders<TContent>.Filter.Where(
                p => p.ContentId == content.ContentId);

            update = Builders<TContent>.Update
                .Set(p => p.ContentId, content.ContentId);

            requests.Add(new UpdateOneModel<TContent>(filter, update)
            {
                IsUpsert = false
            });


            // BulkWrite
            var options = new BulkWriteOptions()
            {
                IsOrdered = true
            };

            BulkWriteResult<TContent> bulkResult = await _contentCollection
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);

            OperationStatus result = bulkResult.MatchedCount == 0
                    ? OperationStatus.NotFound
                : bulkResult.MatchedCount == 1
                    ? OperationStatus.VersionChanged
                    : OperationStatus.Success;

            return result;
        }

        public virtual async Task<long> IncrementCommentsCount(int increment
            , Expression<Func<TContent, bool>> filterConditions)
        {
            FilterDefinition<TContent> filter = ToFilter(filterConditions);

            var update = Builders<TContent>.Update
                .Inc(p => p.CommentsCount, increment);

            UpdateResult result = await _contentCollection
                .UpdateOneAsync(filter, update)
                .ConfigureAwait(false);

            return result.ModifiedCount;
        }



        //delete
        public virtual async Task<long> DeleteMany(Expression<Func<TContent, bool>> filterConditions)
        {
            FilterDefinition<TContent> filter = ToFilter(filterConditions);

            DeleteResult result = await _contentCollection
                .DeleteManyAsync(filter)
                .ConfigureAwait(false);

            return result.DeletedCount;
        }

    }
}
