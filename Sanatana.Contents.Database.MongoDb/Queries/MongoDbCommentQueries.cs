using Sanatana.MongoDb;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Bson.Serialization;
using System.Diagnostics;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Database.MongoDb.Context;
using Sanatana.Contents.Utilities;

namespace Sanatana.Contents.Database.MongoDb.Queries
{
    public class MongoDbCommentQueries<TContent, TComment>
        : ICommentQueries<ObjectId, TContent, TComment>
        where TContent : Content<ObjectId>
        where TComment : Comment<ObjectId>
    {
        //fields
        protected IContentMongoDbContext _mongoDbContext;
        protected IMongoCollection<TComment> _commentCollection;
        protected IMongoCollection<TContent> _contentCollection;


        //init
        public MongoDbCommentQueries(IContentMongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
            _commentCollection = mongoDbContext.GetCollection<TComment>();
            _contentCollection = mongoDbContext.GetCollection<TContent>();
        }


        //common
        protected virtual FilterDefinition<TComment> ToFilter(
            Expression<Func<TComment, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            var visitedCondition = (Expression<Func<TComment, bool>>)visitor.Visit(filterConditions);
            return Builders<TComment>.Filter.Where(visitedCondition);
        }
        
        protected virtual ProjectionDefinition<CommentJoinResult<ObjectId, TComment, TContent>> ToContentProjection(
            DataAmount dataAmmount)
        {
            var projection = Builders<CommentJoinResult<ObjectId, TComment, TContent>>.Projection.Combine();

            if (dataAmmount == DataAmount.ShortContent)
            {
                projection = Builders<CommentJoinResult<ObjectId, TComment, TContent>>.Projection
                    .Exclude(p => p.Content.FullText);
            }
            else if (dataAmmount == DataAmount.DescriptionOnly)
            {
                projection = Builders<CommentJoinResult<ObjectId, TComment, TContent>>.Projection
                    .Exclude(p => p.Content.FullText)
                    .Exclude(p => p.Content.ShortText);
            }

            return projection;
        }


        //insert
        public virtual async Task InsertMany(IEnumerable<TComment> comments)
        {
            foreach (TComment comment in comments)
            {
                comment.CommentId = ObjectId.GenerateNewId();
                comment.Content = null;
            }

            await _commentCollection.InsertManyAsync(comments)
                .ConfigureAwait(false);
        }


        //select
        public Task<long> Count(Expression<Func<TComment, bool>> filterConditions)
        {
            FilterDefinition<TComment> filter = ToFilter(filterConditions);
            var options = new CountOptions()
            {
            };

            return _commentCollection.CountAsync(filter, options);
        }

        public virtual async Task<TComment> SelectOne(
            Expression<Func<TComment, bool>> filterConditions)
        {
            FilterDefinition<TComment> filter = ToFilter(filterConditions);

            TComment result = await _commentCollection.Find(filter)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return result;
        }

        public virtual Task<List<TComment>> SelectMany(int page, int pageSize, bool orderDescending,
            Expression<Func<TComment, bool>> filterConditions)
        {
            int skip = MongoDbUtility.ToSkipNumber(page, pageSize);
            FilterDefinition<TComment> filter = ToFilter(filterConditions);
            var options = new FindOptions();

            IFindFluent<TComment, TComment> fluent = _commentCollection
                .Find(filter, options)
                .Skip(skip)
                .Limit(pageSize);

            if (orderDescending)
            {
                fluent = fluent.SortByDescending(x => x.AddTimeUtc);
            }
            else
            {
                fluent = fluent.SortBy(x => x.AddTimeUtc);
            }

            return fluent.ToListAsync();
        }
        
        public virtual Task<List<CommentJoinResult<ObjectId, TComment, TContent>>> SelectManyJoinedContent(
            int page, int pageSize, bool orderDescending, DataAmount contentDataAmmount,
            Expression<Func<CommentJoinResult<ObjectId, TComment, TContent>, bool>> filterConditions)
        {
            var visitor = new EqualityExpressionVisitor();  //rewrite Equals method calls to ==
            var visitedCondition = (Expression<Func<CommentJoinResult<ObjectId, TComment, TContent>, bool>>)visitor.Visit(filterConditions);

            string joinedContent = "content";
            int skip = MongoDbUtility.ToSkipNumber(page, pageSize);
            FilterDefinition<CommentJoinResult<ObjectId, TComment, TContent>> filter = visitedCondition;
            ProjectionDefinition<CommentJoinResult<ObjectId, TComment, TContent>> contentProjection
                = ToContentProjection(contentDataAmmount);

            IAggregateFluent<CommentJoinResult<ObjectId, TComment, TContent>> query = _commentCollection
                .Aggregate()
                .Lookup<TComment>(
                   _contentCollection.CollectionNamespace.CollectionName,
                   FieldDefinitions.GetFieldMappedName<TComment>(x => x.ContentId),
                   FieldDefinitions.GetFieldMappedName<TContent>(x => x.ContentId),
                   joinedContent)
                .Unwind(joinedContent)
                .Project(new JsonProjectionDefinition<BsonDocument>($@"{{
                    '_id': 0,
                    'Content': '${joinedContent}',
                    'Comment': '$$ROOT'
                }}"))
                .Project(new JsonProjectionDefinition<BsonDocument>($@"{{
                    'Comment.{joinedContent}': 0
                }}"))
                .As<CommentJoinResult<ObjectId, TComment, TContent>>();

            if (orderDescending)
            {
                query = query.SortByDescending(x => x.Comment.AddTimeUtc);
            }
            else
            {
                query = query.SortBy(x => x.Comment.AddTimeUtc);
            }

            return query
                 .Match(filter)
                 .Skip(skip)
                 .Limit(pageSize)
                 .Project<CommentJoinResult<ObjectId, TComment, TContent>>(contentProjection)
                 .ToListAsync();
        }
        

        //update
        public virtual async Task<long> UpdateMany(IEnumerable<TComment> comments
            , params Expression<Func<TComment, object>>[] propertiesToUpdate)
        {
            if (propertiesToUpdate.Count() == 0
                || comments.Count() == 0)
            {
                return 0;
            }

            var requests = new List<WriteModel<TComment>>();

            foreach (TComment comment in comments)
            {
                var filter = Builders<TComment>.Filter.Where(
                    p => p.CommentId == comment.CommentId);

                var update = Builders<TComment>.Update.Combine();

                foreach (Expression<Func<TComment, object>> exp in propertiesToUpdate)
                {
                    object value = exp.Compile().Invoke(comment);
                    update = update.Set(exp, value);
                }

                requests.Add(new UpdateOneModel<TComment>(filter, update)
                {
                    IsUpsert = false
                });
            }

            // BulkWrite
            var options = new BulkWriteOptions()
            {
                IsOrdered = false
            };
            BulkWriteResult<TComment> result = await _commentCollection
                .BulkWriteAsync(requests, options)
                .ConfigureAwait(false);

            return result.ModifiedCount;
        }


        //delete
        public virtual async Task<long> DeleteMany(IEnumerable<TComment> comments)
        {
            if (comments.Count() == 0)
            {
                return 0;
            }

            var filter = Builders<TComment>.Filter.And();

            foreach (TComment comment in comments)
            {
                filter = Builders<TComment>.Filter.Or(filter
                    , Builders<TComment>.Filter.Where(
                        p => p.CommentId == comment.CommentId
                        && p.ContentId == comment.ContentId));
            }

            DeleteResult result = await _commentCollection.DeleteOneAsync(filter)
                .ConfigureAwait(false);

            return result.DeletedCount;
        }

        public virtual async Task<long> DeleteMany(
            Expression<Func<TComment, bool>> filterConditions)
        {
            FilterDefinition<TComment> filter = ToFilter(filterConditions);

            DeleteResult result = await _commentCollection.DeleteManyAsync(filter)
                .ConfigureAwait(false);

            return result.DeletedCount;
        }

    }
}
