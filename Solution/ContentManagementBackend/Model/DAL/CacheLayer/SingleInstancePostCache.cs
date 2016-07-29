using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using System.Web;

namespace ContentManagementBackend
{
    public class SingleInstancePostCache<TKey> : IContentQueries<TKey>
        where TKey : struct
    {
        //поля
        protected IContentQueries<TKey> _queries;
        protected ICacheProvider _cache;


        //свойства
        protected virtual QueryResult<List<ContentCategoryGroup<TKey>>> LatestFromEachCategory
        {
            get
            {
                return (QueryResult<List<ContentCategoryGroup<TKey>>>)
                    _cache.Get(Constants.CACHE_LATEST_POSTS_EACH_CATEGORY_KEY);
            }
            set
            {
                DateTime expiryUtc = DateTime.UtcNow.Add(Constants.CACHE_LATEST_POSTS_EACH_CATEGORY_EXPIRY_PERIOD);
                _cache.Set(Constants.CACHE_LATEST_POSTS_EACH_CATEGORY_KEY, value, expiryUtc);
            }
        }
        protected virtual QueryResult<List<ContentBase<TKey>>> Popular
        {
            get
            {
                return (QueryResult<List<ContentBase<TKey>>>)
                    _cache.Get(Constants.CACHE_POPULAR_POSTS_KEY);
            }
            set
            {
                DateTime expiryUtc = DateTime.UtcNow.Add(Constants.CACHE_POPULAR_POSTS_EXPIRY_PERIOD);
                _cache.Set(Constants.CACHE_POPULAR_POSTS_KEY, value, expiryUtc);
            }
        }



        //инициализация
        public SingleInstancePostCache(IContentQueries<TKey> queries, ICacheProvider cache)
        {
            _queries = queries;
            _cache = cache;
        }
        

        

        //Insert
        public async Task<QueryResult<bool>> Insert(ContentBase<TKey> post)
        {
            QueryResult<bool> result = await _queries.Insert(post);

            bool isPublished = post.IsPublished && post.PublishTimeUtc <= DateTime.UtcNow;

            if (!result.HasExceptions && result.Result
                && isPublished && LatestFromEachCategory != null)
            {                
                _cache.Remove(Constants.CACHE_LATEST_POSTS_EACH_CATEGORY_KEY);
            }

            return result;
        }



        //Select
        public async Task<QueryResult<List<ContentBase<TKey>>>> SelectShortPopular(
            TimeSpan period, int count, List<TKey> excludeCategoryIDs, bool useAllPostsToMatchCount)
        {
            if (Popular != null)
            {
                return Popular;
            }

            QueryResult<List<ContentBase<TKey>>> result = 
                await _queries.SelectShortPopular(period, count, excludeCategoryIDs, useAllPostsToMatchCount);

            if (!result.HasExceptions)
            {
                Popular = result;
            }

            return result;
        }

        public async Task<QueryResult<List<ContentCategoryGroup<TKey>>>> SelectLatestFromEachCategory(
            int count, List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null)
        {
            if(LatestFromEachCategory != null)
            {
                return LatestFromEachCategory;
            }

            QueryResult<List<ContentCategoryGroup<TKey>>> result = 
                await _queries.SelectLatestFromEachCategory(count, categoryIDs, excludeCategoryIDs);

            if(!result.HasExceptions)
            {
                LatestFromEachCategory = result;
            }

            return result;
        }
        
        
        public Task<QueryResult<int>> Count(
            bool onlyPublished, List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null)
        {
            return _queries.Count(onlyPublished, categoryIDs, excludeCategoryIDs);
        }


        public async Task<QueryResult<ContentBase<TKey>>> SelectFull(string url, bool incrementView, bool onlyPublished)
        {
            QueryResult<ContentBase<TKey>> result = await _queries.SelectFull(url, incrementView, onlyPublished);
            
            if (!result.HasExceptions && incrementView)
            {
                url = url.ToLowerInvariant();

                if (LatestFromEachCategory != null)
                {
                    foreach (ContentCategoryGroup<TKey> group in LatestFromEachCategory.Result)
                    {
                        ContentBase<TKey> latestPost = group.Content.FirstOrDefault(p => p.Url == url);

                        if (latestPost != null)
                        {
                            latestPost.ViewsCount += 1;
                        }
                    }
                }

                if (Popular != null)
                {
                    ContentBase<TKey> popularPost = Popular.Result.FirstOrDefault(p => p.Url == url);

                    if (popularPost != null)
                    {
                        popularPost.ViewsCount += 1;
                    }
                }
            }

            return result;
        }

        public async Task<QueryResult<ContentBase<TKey>>> SelectFull(
            TKey contentID, bool incrementView, bool onlyPublished)
        {
            QueryResult<ContentBase<TKey>> result = await _queries.SelectFull(contentID, incrementView, onlyPublished);
            
            if (!result.HasExceptions && incrementView)
            {
                if (LatestFromEachCategory != null)
                {
                    foreach (ContentCategoryGroup<TKey> group in LatestFromEachCategory.Result)
                    {
                        ContentBase<TKey> latestPost = group.Content
                            .FirstOrDefault(p => EqualityComparer<TKey>.Default.Equals(p.ContentID, contentID));

                        if (latestPost != null)
                        {
                            latestPost.ViewsCount += 1;
                        }
                    }
                }

                if (Popular != null)
                {
                    ContentBase<TKey> popularPost = Popular.Result
                        .FirstOrDefault(p => EqualityComparer<TKey>.Default.Equals(p.ContentID, contentID));

                    if (popularPost != null)
                    {
                        popularPost.ViewsCount += 1;
                    }
                }
            }

            return result;
        }

        public Task<QueryResult<List<ContentBase<TKey>>>> SelectFullNotIndexed(
            DateTime fromDateUtc, List<TKey> excludeCategoryIDs, int count)
        {
            return _queries.SelectFullNotIndexed(fromDateUtc, excludeCategoryIDs, count);
        }

        public Task<QueryResult<ContentBase<TKey>>> SelectShort(TKey contentID, bool includeShortContent)
        {
            return _queries.SelectShort(contentID, includeShortContent);
        }

        public Task<QueryResult<List<ContentBase<TKey>>>> SelectShortList(
            List<TKey> contentIDs, bool onlyPublished, bool includeShortContent)
        {
            return _queries.SelectShortList(contentIDs, onlyPublished, includeShortContent);
        }

        public Task<QueryResult<List<ContentBase<TKey>>>> SelectShortList(
            int page, int pageSize, bool onlyPublished, bool includeShortContent
            , List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null)
        {
            return _queries.SelectShortList(page, pageSize, onlyPublished
                , includeShortContent, categoryIDs, excludeCategoryIDs);
        }

        public Task<QueryResult<List<ContentBase<TKey>>>> SelectShortListСontinuation(
            DateTime? lastPublishTimeUtc, int pageSize, bool onlyPublished
            , bool includeShortContent, List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null)
        {
            return _queries.SelectShortListСontinuation(lastPublishTimeUtc, pageSize, onlyPublished
                , includeShortContent, categoryIDs, excludeCategoryIDs);
        }

        public Task<QueryResult<ContentBase<TKey>>> SelectShortNext(
            ContentBase<TKey> post, bool onlyPublished, bool includeShortContent
            , List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null)
        {
            return _queries.SelectShortNext(post, onlyPublished, includeShortContent
                , categoryIDs, excludeCategoryIDs);
        }

        public Task<QueryResult<ContentBase<TKey>>> SelectShortPrevious(
            ContentBase<TKey> post, bool onlyPublished, bool includeShortContent
            , List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null)
        {
            return _queries.SelectShortPrevious(post, onlyPublished, includeShortContent
                , categoryIDs, excludeCategoryIDs);
        }

        

        //Update
        public async Task<bool> IncrementCommentCount(TKey contentID, int increment)
        {
            bool result = await _queries.IncrementCommentCount(contentID, increment);

            if (result)
            {
                if (LatestFromEachCategory != null)
                {
                    foreach (ContentCategoryGroup<TKey> group in LatestFromEachCategory.Result)
                    {
                        ContentBase<TKey> latestPost = group.Content
                            .FirstOrDefault(p => EqualityComparer<TKey>.Default.Equals(p.ContentID, contentID));

                        if(latestPost != null)
                        {
                            latestPost.CommentsCount += increment;
                        }
                    }
                }

                if (Popular != null)
                {
                    ContentBase<TKey> popularPost = Popular.Result
                        .FirstOrDefault(p => EqualityComparer<TKey>.Default.Equals(p.ContentID, contentID));

                    if(popularPost != null)
                    {
                        popularPost.CommentsCount += increment;
                    }
                }
            }

            return result;
        }

        public Task<bool> SetIndexed(List<TKey> contentIDs, bool value)
        {
            return _queries.SetIndexed(contentIDs, value);
        }

        public async Task<ContentUpdateResult> Update(ContentBase<TKey> post, string updateNonce, bool matchNonce)
        {
            ContentUpdateResult result = await _queries.Update(post, updateNonce, matchNonce);
            
            if (result == ContentUpdateResult.Success)
            {
                if(LatestFromEachCategory != null)
                {
                    _cache.Remove(Constants.CACHE_LATEST_POSTS_EACH_CATEGORY_KEY);
                }

                bool requeryPopular = Popular != null
                    && Popular.Result.Any(p => EqualityComparer<TKey>.Default.Equals(p.ContentID, post.ContentID));

                if (requeryPopular)
                {                    
                    _cache.Remove(Constants.CACHE_POPULAR_POSTS_KEY);
                }
            }

            return result;
        }



        //Delete
        public async Task<bool> Delete(TKey contentID)
        {
            bool result = await _queries.Delete(contentID);

            if (result)
            {
                bool requeryLatest = LatestFromEachCategory != null
                    && LatestFromEachCategory.Result.Any(gr => gr.Content
                        .Any(p => EqualityComparer<TKey>.Default.Equals(p.ContentID, contentID)));

                if (requeryLatest)
                {
                    _cache.Remove(Constants.CACHE_LATEST_POSTS_EACH_CATEGORY_KEY);
                }

                bool requeryPopular = Popular != null
                    && Popular.Result.Any(p => EqualityComparer<TKey>.Default.Equals(p.ContentID, contentID));

                if (requeryPopular)
                {
                    _cache.Remove(Constants.CACHE_POPULAR_POSTS_KEY);
                }
            }

            return result;
        }


    }
}
