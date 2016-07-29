using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.MongoDbStorage
{
    public class MongoDbCommentQueries : ICommentQueries<ObjectId>
    {
        //поля
        protected MongoDbContext _mongoContext;
        protected ICommonLogger _logger;



        //инициализация
        public MongoDbCommentQueries(ICommonLogger logger)
        {
            _logger = logger;
        }
        public MongoDbCommentQueries(ICommonLogger logger, MongoDbConnectionSettings settings)
        {
            _logger = logger;
            _mongoContext = new MongoDbContext(settings);
        }

        
        
        //методы
        public virtual async Task<bool> Insert(Comment<ObjectId> comment)
        {
            bool hasExceptions = false;

            try
            {
                comment.CommentID = ObjectId.GenerateNewId();
                await _mongoContext.Comments.InsertOneAsync(comment);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return !hasExceptions;
        }
        
        public virtual async Task<QueryResult<List<Comment<ObjectId>>>> Select(
            List<CommentState> states, int page, int pageSize, ObjectId? contentID = null)
        {
            bool hasExceptions = false;
            List<Comment<ObjectId>> result = null;
            int skip = Common.MongoDb.MongoPaging.ToSkipNumber(page, pageSize);

            var filter = Builders<Comment<ObjectId>>.Filter.Where(
                p => states.Contains(p.State));

            if(contentID != null)
            {
                filter = Builders<Comment<ObjectId>>.Filter.And(
                    Builders<Comment<ObjectId>>.Filter.Where(p => p.ContentID == contentID.Value)
                    , filter);
            }

            var options = new FindOptions();

            try
            {
                IFindFluent<Comment<ObjectId>, Comment<ObjectId>> fluent = _mongoContext.Comments.Find(filter, options);

                //sort by insert time
                if (contentID == null)
                {
                    fluent = fluent.SortByDescending(p => p.CommentID);
                }

                result = await fluent
                    .Skip(skip)
                    .Limit(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            if (result == null)
                result = new List<Comment<ObjectId>>();

            return new QueryResult<List<Comment<ObjectId>>>(result, hasExceptions);
        }

        public virtual async Task<QueryResult<bool>> UpdateContent(
            Comment<ObjectId> comment, bool matchAuthorID
            , params Expression<Func<Comment<ObjectId>, object>>[] fieldsToUpdate)
        {
            bool hasExceptions = false;
            bool updated = false;

            var filter = Builders<Comment<ObjectId>>.Filter.Where(
                p => p.ContentID == comment.ContentID
                && p.AddTimeUtc == comment.AddTimeUtc
                && p.State != CommentState.Deleted);

            if(matchAuthorID)
            {
                filter = Builders<Comment<ObjectId>>.Filter.And(filter
                    , Builders<Comment<ObjectId>>.Filter.Where(p => p.AuthorID == p.AuthorID));
            }

            var update = Builders<Comment<ObjectId>>.Update.Combine();
            
            foreach (Expression<Func<Comment<ObjectId>, object>> exp in fieldsToUpdate)
            {
                object value = exp.Compile().Invoke(comment);
                update = update.Set(exp, value);
            }

            var options = new UpdateOptions()
            {
                IsUpsert = false
            };

            try
            {
                UpdateResult result = await _mongoContext.Comments.UpdateOneAsync(
                    filter, update, options);

                updated = result.MatchedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return new QueryResult<bool>(updated, hasExceptions);
        }
        
        public virtual async Task<bool> Delete(Comment<ObjectId> comment)
        {
            bool hasExceptions = false;

            var filter = Builders<Comment<ObjectId>>.Filter.Where(
               p => p.ContentID == comment.ContentID
               && p.AddTimeUtc == comment.AddTimeUtc);

            try
            {
                DeleteResult result = await _mongoContext.Comments.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return !hasExceptions;
        }

        public virtual async Task<bool> Delete(ObjectId contentID)
        {
            bool hasExceptions = false;

            var filter = Builders<Comment<ObjectId>>.Filter.Where(
               p => p.ContentID == contentID);

            try
            {
                DeleteResult result = await _mongoContext.Comments.DeleteManyAsync(filter);
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
