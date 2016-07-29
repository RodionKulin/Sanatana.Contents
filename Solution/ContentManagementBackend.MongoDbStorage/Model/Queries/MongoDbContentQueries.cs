using Common.MongoDb;
using Common.Utility;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend.MongoDbStorage
{
    public class MongoDbContentQueries : IContentQueries<ObjectId>
    {
        //поля
        protected MongoDbContext _mongoContext;
        protected ICommonLogger _logger;



        //инициализация
        public MongoDbContentQueries(ICommonLogger logger)
        {
            _logger = logger;
        }
        public MongoDbContentQueries(ICommonLogger logger, MongoDbConnectionSettings settings)
        {
            _logger = logger;
            _mongoContext = new MongoDbContext(settings);
        }



        //insert
        public virtual async Task<QueryResult<bool>> Insert(ContentBase<ObjectId> content)
        {
            bool hasExceptions = false;
            bool isUrlUnique = true;

            try
            {
                await _mongoContext.Posts.InsertOneAsync(content);
                isUrlUnique = true;
            }
            catch (Exception ex)
            {
                bool isUrlDuplicate = MongoExceptions.IsDuplicateException(ex, nameof(content.Url));

                if (isUrlDuplicate)
                {
                    isUrlUnique = false;
                }
                else
                {
                    _logger.Exception(ex);
                    hasExceptions = true;
                }
            }

            return new QueryResult<bool>(isUrlUnique, hasExceptions);
        }



        //count
        public virtual async Task<QueryResult<int>> Count(
            bool onlyPublished, List<ObjectId> categoryIDs = null, List<ObjectId> excludeCategoryIDs = null)
        {
            bool hasExceptions = false;
            long count = 0;

            var filter = Builders<ContentBase<ObjectId>>.Filter.Where(p => true);

            if (categoryIDs != null && categoryIDs.Count > 0)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => categoryIDs.Contains(p.CategoryID)));
            }
            if (excludeCategoryIDs != null && excludeCategoryIDs.Count > 0)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => !excludeCategoryIDs.Contains(p.CategoryID)));
            }
            if (onlyPublished)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => p.IsPublished == true
                    && p.PublishTimeUtc < DateTime.UtcNow));
            }
            else
            {
                //to use index
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(
                        p => p.PublishTimeUtc <= DateTime.MaxValue));
            }

            var options = new CountOptions()
            {
            };

            try
            {
                //string explain = _mongoContext.Posts.ExplainCount(ExplainVerbosity.QueryPlanner
                //    , filter, options).Result.ToJsonIntended();

                count = await _mongoContext.Posts.CountAsync(filter, options);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return new QueryResult<int>((int)count, hasExceptions);
        }



        //select full
        public virtual Task<QueryResult<ContentBase<ObjectId>>> SelectFull(
            ObjectId contentID, bool incrementView, bool onlyPublished)
        {
            FilterDefinition<ContentBase<ObjectId>> filter
                = Builders<ContentBase<ObjectId>>.Filter.Where(p => p.ContentID == contentID);

            return SelectFull(filter, incrementView, onlyPublished);
        }

        public virtual Task<QueryResult<ContentBase<ObjectId>>> SelectFull(
            string url, bool incrementView, bool onlyPublished)
        {
            url = (url ?? string.Empty).ToLowerInvariant();
            FilterDefinition<ContentBase<ObjectId>> filter
                = Builders<ContentBase<ObjectId>>.Filter.Where(p => p.Url == url);

            return SelectFull(filter, incrementView, onlyPublished);
        }

        protected virtual async Task<QueryResult<ContentBase<ObjectId>>> SelectFull(
            FilterDefinition<ContentBase<ObjectId>> filter, bool incrementView, bool onlyPublished)
        {
            bool hasExceptions = false;
            ContentBase<ObjectId> content = null;

            if (onlyPublished)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => p.IsPublished == true));
            }

            var update = Builders<ContentBase<ObjectId>>.Update
                .SetOnInsert(p => p.ContentID, ObjectId.Empty);

            if (incrementView)
            {
                update = update.Inc(p => p.ViewsCount, 1);
            }

            var options = new FindOneAndUpdateOptions<ContentBase<ObjectId>>()
            {
                IsUpsert = false,
                ReturnDocument = MongoDB.Driver.ReturnDocument.After
            };

            try
            {
                content = await _mongoContext.Posts.FindOneAndUpdateAsync(
                    filter, update, options);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return new QueryResult<ContentBase<ObjectId>>(content, hasExceptions);
        }

        public virtual async Task<QueryResult<List<ContentBase<ObjectId>>>> SelectFullNotIndexed(
            DateTime fromDateUtc, List<ObjectId> excludeCategoryIDs, int count)
        {
            bool hasExceptions = false;
            List<ContentBase<ObjectId>> posts = null;

            var filter = Builders<ContentBase<ObjectId>>.Filter.Where(
                p => p.PublishTimeUtc >= fromDateUtc
                && p.IsPublished == true
                && !excludeCategoryIDs.Contains(p.CategoryID)
                && !p.IsIndexed);

            var options = new FindOptions()
            {
            };

            try
            {
                posts = await _mongoContext.Posts.Find(filter, options)
                    .Limit(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return new QueryResult<List<ContentBase<ObjectId>>>(posts, hasExceptions);
        }



        //select short
        public virtual async Task<QueryResult<ContentBase<ObjectId>>> SelectShort(
            ObjectId contentID, bool includeShortContent)
        {
            bool hasExceptions = false;
            ContentBase<ObjectId> content = null;

            var filter = Builders<ContentBase<ObjectId>>.Filter
                .Where(p => p.ContentID == contentID);

            var projection = Builders<ContentBase<ObjectId>>.Projection
                .Exclude(p => p.FullContent);

            if (!includeShortContent)
            {
                projection = projection.Exclude(p => p.ShortContent);
            }

            var options = new FindOptions()
            {
            };

            try
            {
                content = await _mongoContext.Posts.Find(filter, options)
                    .Project<ContentBase<ObjectId>>(projection).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return new QueryResult<ContentBase<ObjectId>>(content, hasExceptions);
        }

        public virtual async Task<QueryResult<ContentBase<ObjectId>>> SelectShortPrevious(
            ContentBase<ObjectId> content, bool onlyPublished, bool includeShortContent
            , List<ObjectId> categoryIDs = null, List<ObjectId> excludeCategoryIDs = null)
        {
            bool hasExceptions = false;
            ContentBase<ObjectId> postBefore = null;

            var filter = Builders<ContentBase<ObjectId>>.Filter
                .Where(p => p.PublishTimeUtc < content.PublishTimeUtc);

            if (onlyPublished)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter, Builders<ContentBase<ObjectId>>.Filter.Where(
                    p => p.IsPublished == true
                    && p.PublishTimeUtc < DateTime.UtcNow));
            }
            if (categoryIDs != null && categoryIDs.Count > 0)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => categoryIDs.Contains(p.CategoryID)));
            }
            if (excludeCategoryIDs != null && excludeCategoryIDs.Count > 0)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => !excludeCategoryIDs.Contains(p.CategoryID)));
            }

            var projection = Builders<ContentBase<ObjectId>>.Projection
                .Exclude(p => p.FullContent);

            if (!includeShortContent)
            {
                projection = projection.Exclude(p => p.ShortContent);
            }

            var options = new FindOptions()
            {
            };

            try
            {
                //string explain = _mongoContext.Posts.ExplainFind<ContentBase<ObjectId>, ContentBase<ObjectId>>(
                //    ExplainVerbosity.QueryPlanner, filter).Result.ToJsonIntended();

                postBefore = await _mongoContext.Posts.Find(filter, options)
                    .Project<ContentBase<ObjectId>>(projection).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return new QueryResult<ContentBase<ObjectId>>(postBefore, hasExceptions);
        }

        public virtual async Task<QueryResult<ContentBase<ObjectId>>> SelectShortNext(
            ContentBase<ObjectId> content, bool onlyPublished, bool includeShortContent
            , List<ObjectId> categoryIDs = null, List<ObjectId> excludeCategoryIDs = null)
        {
            bool hasExceptions = false;
            ContentBase<ObjectId> articleAfter = null;

            var filter = Builders<ContentBase<ObjectId>>.Filter
                .Where(p => p.PublishTimeUtc < content.PublishTimeUtc);

            if (onlyPublished)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter, Builders<ContentBase<ObjectId>>.Filter.Where(
                    p => p.IsPublished == true
                    && p.PublishTimeUtc < DateTime.UtcNow));
            }
            if (categoryIDs != null && categoryIDs.Count > 0)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => categoryIDs.Contains(p.CategoryID)));
            }
            if (excludeCategoryIDs != null && excludeCategoryIDs.Count > 0)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => !excludeCategoryIDs.Contains(p.CategoryID)));
            }

            var projection = Builders<ContentBase<ObjectId>>.Projection
                .Exclude(p => p.FullContent);

            if (!includeShortContent)
            {
                projection = projection.Exclude(p => p.ShortContent);
            }

            var options = new FindOptions()
            {
            };

            try
            {
                articleAfter = await _mongoContext.Posts.Find(filter, options)
                    .Project<ContentBase<ObjectId>>(projection).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return new QueryResult<ContentBase<ObjectId>>(articleAfter, hasExceptions);
        }
        
        public virtual async Task<QueryResult<List<ContentBase<ObjectId>>>> SelectShortList(
            int page, int pageSize, bool onlyPublished, bool includeShortContent
            , List<ObjectId> categoryIDs = null, List<ObjectId> excludeCategoryIDs = null)
        {
            bool hasExceptions = false;
            List<ContentBase<ObjectId>> posts = null;

            try
            {
                int skip = MongoPaging.ToSkipNumber(page, pageSize);

                var filter = Builders<ContentBase<ObjectId>>.Filter.Where(p => true);

                if (onlyPublished)
                {
                    filter = Builders<ContentBase<ObjectId>>.Filter.And(filter, Builders<ContentBase<ObjectId>>.Filter.Where(
                        p => p.IsPublished == true
                        && p.PublishTimeUtc < DateTime.UtcNow));
                }
                if (categoryIDs != null && categoryIDs.Count > 0)
                {
                    filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                        , Builders<ContentBase<ObjectId>>.Filter.Where(p => categoryIDs.Contains(p.CategoryID)));
                }
                if (excludeCategoryIDs != null && excludeCategoryIDs.Count > 0)
                {
                    filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                        , Builders<ContentBase<ObjectId>>.Filter.Where(p => !excludeCategoryIDs.Contains(p.CategoryID)));
                }

                var projection = Builders<ContentBase<ObjectId>>.Projection
                    .Exclude(p => p.FullContent);

                if (!includeShortContent)
                {
                    projection = projection.Exclude(p => p.ShortContent);
                }

                var options = new FindOptions()
                {
                };

                //string explain = _mongoContext.Posts.ExplainFind<ContentBase<ObjectId>, ContentBase<ObjectId>>(
                //    ExplainVerbosity.QueryPlanner, filter).Result.ToJsonIntended();

                posts = await _mongoContext.Posts.Find(filter, options)
                    .Project<ContentBase<ObjectId>>(projection)
                    .SortByDescending(p => p.PublishTimeUtc)
                    .Skip(skip)
                    .Limit(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            if (posts == null)
                posts = new List<ContentBase<ObjectId>>();

            return new QueryResult<List<ContentBase<ObjectId>>>(posts, hasExceptions);
        }

        public virtual async Task<QueryResult<List<ContentBase<ObjectId>>>> SelectShortList(
            List<ObjectId> contentIDs, bool onlyPublished, bool includeShortContent)
        {
            bool hasExceptions = false;
            List<ContentBase<ObjectId>> posts = null;

            var filter = Builders<ContentBase<ObjectId>>.Filter
              .Where(p => contentIDs.Contains(p.ContentID));

            if (onlyPublished)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => p.IsPublished == true));
            }

            var projection = Builders<ContentBase<ObjectId>>.Projection
                .Exclude(p => p.FullContent);

            if (!includeShortContent)
            {
                projection = projection.Exclude(p => p.ShortContent);
            }

            var options = new FindOptions()
            {
            };

            try
            {
                posts = await _mongoContext.Posts.Find(filter, options)
                    .Project<ContentBase<ObjectId>>(projection)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            if (posts == null)
                posts = new List<ContentBase<ObjectId>>();

            return new QueryResult<List<ContentBase<ObjectId>>>(posts, hasExceptions);
        }

        public virtual async Task<QueryResult<List<ContentBase<ObjectId>>>> SelectShortListСontinuation(
            DateTime? lastPublishTimeUtc, int pageSize, bool onlyPublished, bool includeShortContent
            , List<ObjectId> categoryIDs = null, List<ObjectId> excludeCategoryIDs = null)
        {
            bool hasExceptions = false;
            List<ContentBase<ObjectId>> posts = null;
            lastPublishTimeUtc = lastPublishTimeUtc ?? DateTime.UtcNow;

            var filter = Builders<ContentBase<ObjectId>>.Filter
                .Where(p => p.PublishTimeUtc < lastPublishTimeUtc);

            if (onlyPublished)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => p.IsPublished == true));
            }
            if (categoryIDs != null && categoryIDs.Count > 0)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => categoryIDs.Contains(p.CategoryID)));
            }
            if (excludeCategoryIDs != null && excludeCategoryIDs.Count > 0)
            {
                filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                    , Builders<ContentBase<ObjectId>>.Filter.Where(p => !excludeCategoryIDs.Contains(p.CategoryID)));
            }

            var projection = Builders<ContentBase<ObjectId>>.Projection
                .Exclude(p => p.FullContent);

            if (!includeShortContent)
            {
                projection = projection.Exclude(p => p.ShortContent);
            }

            var options = new FindOptions()
            {
            };

            try
            {
                //string explain = _mongoContext.Posts.ExplainFind<ContentBase<ObjectId>, ContentBase<ObjectId>>(
                //    ExplainVerbosity.QueryPlanner, filter, null).Result.ToJsonIntended();

                posts = await _mongoContext.Posts.Find(filter, options)
                    .Project<ContentBase<ObjectId>>(projection)
                    .SortByDescending(p => p.PublishTimeUtc)
                    .Limit(pageSize)
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            if (posts == null)
                posts = new List<ContentBase<ObjectId>>();

            return new QueryResult<List<ContentBase<ObjectId>>>(posts, hasExceptions);
        }
        
        public virtual Task<QueryResult<List<ContentBase<ObjectId>>>> SelectShortPopular(
            TimeSpan period, int count, List<ObjectId> excludeCategoryIDs, bool useAllPostsToMatchCount)
        {
            return useAllPostsToMatchCount
                ? SelectShortPopularFromAll(period, count, excludeCategoryIDs)
                : SelectShortPopularInPeriod(period, count, excludeCategoryIDs);
        }

        protected virtual async Task<QueryResult<List<ContentBase<ObjectId>>>> SelectShortPopularFromAll(
             TimeSpan period, int count, List<ObjectId> excludeCategoryIDs)
        {
            bool hasExceptions = false;
            List<ContentBase<ObjectId>> posts = new List<ContentBase<ObjectId>>();

            try
            {
                //query
                var filter = Builders<ContentBase<ObjectId>>.Filter.Where(
                    p => p.IsPublished == true
                    && p.PublishTimeUtc < DateTime.UtcNow
                    && p.ViewsCount > 0);

                if (excludeCategoryIDs != null && excludeCategoryIDs.Count > 0)
                {
                    filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                        , Builders<ContentBase<ObjectId>>.Filter.Where(p => !excludeCategoryIDs.Contains(p.CategoryID)));
                }

                var projection = Builders<ContentBase<ObjectId>>.Projection
                    .Exclude(p => p.FullContent);

                int batchSize = count * 5;
                var options = new FindOptions()
                {
                    BatchSize = batchSize > 150 ? 150 : batchSize
                };

                List<ContentBase<ObjectId>> latestPosts = await _mongoContext.Posts.Find(filter, options)
                    .Project<ContentBase<ObjectId>>(projection)
                    .SortByDescending(p => p.PublishTimeUtc)
                    .ToListAsync();


                //Pick top posts
                if (latestPosts.Count > 0)
                {
                    DateTime maxSearchTime = DateTime.UtcNow;
                    DateTime minSearchTime = DateTime.UtcNow - period;
                    DateTime minTimeSelected = latestPosts.Last().PublishTimeUtc;
                    int postsRequired = count;

                    do
                    {
                        List<ContentBase<ObjectId>> latestPeriodPosts = latestPosts.Where(
                               p => maxSearchTime >= p.PublishTimeUtc
                               && p.PublishTimeUtc > minSearchTime)
                           .OrderByDescending(p => p.ViewsCount)
                           .Take(postsRequired)
                           .ToList();

                        posts.AddRange(latestPeriodPosts);

                        postsRequired = count - posts.Count;
                        maxSearchTime -= period;
                        minSearchTime -= period;
                    }
                    while (postsRequired > 0 && maxSearchTime >= minTimeSelected);
                }

                posts = posts.OrderByDescending(p => p.ViewsCount).ToList();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            if (posts == null)
                posts = new List<ContentBase<ObjectId>>();

            return new QueryResult<List<ContentBase<ObjectId>>>(posts, hasExceptions);
        }

        protected virtual async Task<QueryResult<List<ContentBase<ObjectId>>>> SelectShortPopularInPeriod(
            TimeSpan period, int count, List<ObjectId> excludeCategoryIDs)
        {
            bool hasExceptions = false;
            List<ContentBase<ObjectId>> posts = null;
            DateTime minimalTime = DateTime.UtcNow - period;

            var filter = Builders<ContentBase<ObjectId>>.Filter.Where(
                p => p.PublishTimeUtc <= DateTime.UtcNow
                && p.PublishTimeUtc >= minimalTime
                && p.IsPublished == true
                && !excludeCategoryIDs.Contains(p.CategoryID)
                && p.ViewsCount > 0);

            var projection = Builders<ContentBase<ObjectId>>.Projection
                .Exclude(p => p.FullContent);

            var options = new FindOptions()
            {
            };

            try
            {
                //string explain = _mongoContext.Posts.ExplainFind<ContentBase<ObjectId>, ContentBase<ObjectId>>(
                //    ExplainVerbosity.QueryPlanner, filter).Result.ToJsonIntended();

                posts = await _mongoContext.Posts.Find(filter, options)
                    .Project<ContentBase<ObjectId>>(projection)
                    .SortByDescending(p => p.ViewsCount)
                    .Limit(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            if (posts == null)
                posts = new List<ContentBase<ObjectId>>();

            return new QueryResult<List<ContentBase<ObjectId>>>(posts, hasExceptions);
        }
        
        public virtual async Task<QueryResult<List<ContentCategoryGroup<ObjectId>>>> SelectLatestFromEachCategory(
            int count, List<ObjectId> categoryIDs = null, List<ObjectId> excludeCategoryIDs = null)
        {
            bool hasExceptions = false;
            List<ContentCategoryGroup<ObjectId>> list = null;

            try
            {
                var emptyItem = new Post<ObjectId>();
                var emptyGroup = new ContentCategoryGroup<ObjectId>();

                FilterDefinition<ContentBase<ObjectId>> filter = Builders<ContentBase<ObjectId>>.Filter.Where(
                    p => p.PublishTimeUtc < DateTime.UtcNow
                    && p.IsPublished == true);

                if (categoryIDs != null && categoryIDs.Count > 0)
                {
                    filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                        , Builders<ContentBase<ObjectId>>.Filter.Where(p => categoryIDs.Contains(p.CategoryID)));
                }
                if (excludeCategoryIDs != null && excludeCategoryIDs.Count > 0)
                {
                    filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                        , Builders<ContentBase<ObjectId>>.Filter.Where(p => !excludeCategoryIDs.Contains(p.CategoryID)));
                }

                var options = new AggregateOptions()
                {
                    AllowDiskUse = true
                };
                
                IAggregateFluent<ContentCategoryGroup<ObjectId>> aggregateFluent = _mongoContext.Posts.Aggregate(options)
                    .SortByDescending(p => p.PublishTimeUtc)
                    .Match(filter)
                    .Group(new BsonDocument
                    {
                        {
                            "_id", "$" + nameof(emptyItem.CategoryID)
                        },
                        { nameof(emptyGroup.Content), new BsonDocument("$push", "$$ROOT" ) }
                    })
                    .Project<ContentCategoryGroup<ObjectId>>(new BsonDocument
                    {
                        {
                            nameof(emptyGroup.Content),
                            new BsonDocument("$slice", new BsonArray() { "$" + nameof(emptyGroup.Content), 0, count } )
                        }
                    });

                //List<BsonDocument> aggrStages = _mongoContext.Posts
                //    .GetAggregateStagesBson<ContentBase<ObjectId>, BsonDocument>(aggregateFluent);
                //string explanation = _mongoContext.Posts.ExplainAggregation(
                //    aggregateFluent, ExplainVerbosity.ExecutionStats).Result.ToJsonIntended();

                list = await aggregateFluent.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            if (list == new List<ContentCategoryGroup<ObjectId>>())
                list = new List<ContentCategoryGroup<ObjectId>>();

            return new QueryResult<List<ContentCategoryGroup<ObjectId>>>(list, hasExceptions);
        }



        //update
        public virtual async Task<ContentUpdateResult> Update(
            ContentBase<ObjectId> content, string updateNonce, bool matchNonce)
        {
            ContentUpdateResult result = ContentUpdateResult.Success;
                        
            try
            {
                var requests = new List<WriteModel<ContentBase<ObjectId>>>();
                

                // content content update
                var filter = Builders<ContentBase<ObjectId>>.Filter.Where(
                    p => p.ContentID == content.ContentID);

                if (matchNonce)
                {
                    filter = Builders<ContentBase<ObjectId>>.Filter.And(filter
                        , Builders<ContentBase<ObjectId>>.Filter.Where(p => p.UpdateNonce == updateNonce));
                }

                var update = Builders<ContentBase<ObjectId>>.Update
                    .Set(p => p.CategoryID, content.CategoryID)
                    .Set(p => p.UpdateNonce, content.UpdateNonce)
                    .Set(p => p.Title, content.Title)
                    .Set(p => p.FullContent, content.FullContent)
                    .Set(p => p.ShortContent, content.ShortContent)
                    .Set(p => p.HasImage, content.HasImage)
                    .Set(p => p.Url, content.Url)
                    .Set(p => p.PublishTimeUtc, content.PublishTimeUtc)
                    .Set(p => p.IsPublished, content.IsPublished)
                    .Set(p => p.IsIndexed, content.IsIndexed);

                requests.Add(new UpdateOneModel<ContentBase<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });


                // content id update (check exists)
                filter = Builders<ContentBase<ObjectId>>.Filter.Where(
                    p => p.ContentID == content.ContentID);

                update = Builders<ContentBase<ObjectId>>.Update
                    .Set(p => p.ContentID, content.ContentID);

                requests.Add(new UpdateOneModel<ContentBase<ObjectId>>(filter, update)
                {
                    IsUpsert = false
                });


                // BulkWrite
                var options = new BulkWriteOptions()
                {
                    IsOrdered = true
                };

                BulkWriteResult<ContentBase<ObjectId>> bulkResult = await _mongoContext.Posts
                    .BulkWriteAsync(requests, options);

                result = bulkResult.MatchedCount == 0
                        ? ContentUpdateResult.NotFound
                    : bulkResult.MatchedCount == 1
                        ? ContentUpdateResult.NonceChanged
                        : ContentUpdateResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                result = ContentUpdateResult.HasException;
            }

            return result;
        }

        public virtual async Task<bool> IncrementCommentCount(ObjectId contentID, int increment)
        {
            bool hasExceptions = false;

            try
            {
                var filter = Builders<ContentBase<ObjectId>>.Filter.Where(
                  p => p.ContentID == contentID);

                var update = Builders<ContentBase<ObjectId>>.Update
                    .Inc(p => p.CommentsCount, increment);

                UpdateResult result = await _mongoContext.Posts.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return !hasExceptions;
        }

        public virtual async Task<bool> SetIndexed(List<ObjectId> contentIDs, bool value)
        {
            bool hasExceptions = false;

            var filter = Builders<ContentBase<ObjectId>>.Filter.Where(
                p => contentIDs.Contains(p.ContentID));

            var update = Builders<ContentBase<ObjectId>>.Update
                .Set(p => p.IsIndexed, value);

            try
            {
                UpdateResult result = await _mongoContext.Posts.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                hasExceptions = true;
            }

            return !hasExceptions;
        }



        //delete
        public virtual async Task<bool> Delete(ObjectId contentID)
        {
            bool hasExceptions = false;

            var filter = Builders<ContentBase<ObjectId>>.Filter
                .Where(p => p.ContentID == contentID);

            try
            {
                DeleteResult result = await _mongoContext.Posts.DeleteOneAsync(filter);
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
