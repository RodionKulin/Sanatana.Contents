using Sanatana.Contents.Caching.CacheProviders;
using Sanatana.Contents.Database;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching.Concrete
{
    public class ContentFullyCachedQueries<TKey, TContent> : IContentQueries<TKey, TContent>
        where TKey : struct
        where TContent : Content<TKey>
    {
        //fields
        protected IContentQueries<TKey, TContent> _queries;
        protected IQueryCache _queryCache;
        protected ICacheProvider _cacheProvider;

        
        //init
        public ContentFullyCachedQueries(IContentQueries<TKey, TContent> queries
            , IQueryCache queryCache, ICacheProvider queryProvider)
        {
            _queries = queries;
            _queryCache = queryCache;
            _cacheProvider = queryProvider;
        }


        //common
        protected virtual async Task<List<TContent>> SelectAll()
        {
            string cacheKey = UrnId.Create<List<TContent>>();

            List<TContent> list = await  _queryCache
                .ToOptimizedResultUsingCache(cacheKey, () =>
                {
                    return _queries.SelectMany(1, int.MaxValue, DataAmount.FullContent, true, x => true);
                }
                , ContentsConstants.DEFAULT_CACHE_EXPIRY_PERIOD
                , _queryCache.GetDataChangeNotifier<TContent>())
                .ConfigureAwait(false);

            return new List<TContent>(list);
        }


        //Insert
        public virtual async Task<ContentInsertResult> InsertOne(TContent content)
        {
            ContentInsertResult result = await _queries.InsertOne(content).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TContent>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }

        public virtual async Task InsertMany(IEnumerable<TContent> contents)
        {
            await _queries.InsertMany(contents).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TContent>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
        }



        //Select
        public virtual async Task<List<TContent>> SelectTopViews(int pageSize
            , DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions)
        {
            IEnumerable<TContent> allContent = await SelectAll().ConfigureAwait(false);
            allContent = allContent.Where(filterConditions.Compile());            
            return allContent.OrderByDescending(x => x.ViewsCount)
                .Take(pageSize)
                .ToList();
        }

        public virtual async Task<List<ContentCategoryGroupResult<TKey, TContent>>> SelectLatestFromEachCategory(
            int eachCategoryCount, DataAmount dataAmmount, Expression<Func<TContent, bool>> filterConditions)
        {
            IEnumerable<TContent> allContent = await SelectAll().ConfigureAwait(false);
            allContent = allContent.OrderByDescending(x => x.PublishTimeUtc);
            allContent = allContent.Where(filterConditions.Compile());
           
            IEnumerable<IGrouping<TKey, TContent>> groups = allContent.GroupBy(x => x.CategoryId);

            List<ContentCategoryGroupResult<TKey, TContent>> result = groups
                .Select(x => new ContentCategoryGroupResult<TKey, TContent>
                {
                    CategoryId = x.Key,
                    Contents = x.Take(eachCategoryCount).ToList()
                }).ToList();

            return result;
        }

        public virtual async Task<TContent> SelectOne(bool incrementViewCount, DataAmount dataAmmount
            , Expression<Func<TContent, bool>> filterConditions)
        {
            if (incrementViewCount)
            {
                TContent result = await _queries.SelectOne(incrementViewCount, dataAmmount, filterConditions).ConfigureAwait(false);

                string cacheKey = UrnId.Create<List<TContent>>();
                await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
                return result;
            }
            else
            {
                IEnumerable<TContent> allContent = await SelectAll().ConfigureAwait(false);
                return allContent.FirstOrDefault(filterConditions.Compile());
            }
        }

        public virtual async Task<List<TContent>> SelectMany(
            int page, int pageSize, DataAmount dataAmmount, bool orderDescending
            , Expression<Func<TContent, bool>> filterConditions)
        {
            IEnumerable<TContent> allContent = await SelectAll().ConfigureAwait(false);
            allContent = allContent.Where(filterConditions.Compile());

            if (orderDescending)
            {
                allContent = allContent.OrderByDescending(x => x.PublishTimeUtc);
            }
            else
            {
                allContent = allContent.OrderBy(x => x.PublishTimeUtc);
            }

            int skip = (page - 1) * pageSize;
            return allContent.Skip(skip).Take(pageSize).ToList();
        }
        

        //Count
        public virtual async Task<long> Count(Expression<Func<TContent, bool>> filterConditions)
        {
            IEnumerable<TContent> allContent = await SelectAll().ConfigureAwait(false);
            allContent = allContent.Where(filterConditions.Compile());
            return allContent.Count();
        }



        //Update
        public virtual async Task<long> IncrementCommentsCount(int increment, Expression<Func<TContent, bool>> filterConditions)
        {
            long result = await _queries.IncrementCommentsCount(increment, filterConditions).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TContent>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }

        public virtual async Task<long> UpdateMany(TContent values
            , Expression<Func<TContent, bool>> filterConditions
            , params Expression<Func<TContent, object>>[] propertiesToUpdate)
        {
            long result = await _queries
                .UpdateMany(values, filterConditions, propertiesToUpdate)
                .ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TContent>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result; 
        }

        public virtual async Task<OperationStatus> UpdateOne(TContent content
            , long prevVersion, bool matchVersion)
        {
            OperationStatus result = await _queries
                .UpdateOne(content, prevVersion, matchVersion)
                .ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TContent>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }

        public virtual async Task<OperationStatus> UpdateOne(TContent content
            , long prevVersion, bool matchVersion
            , params Expression<Func<TContent, object>>[] propertiesToUpdate)
        {
            OperationStatus result = await _queries
                .UpdateOne(content, prevVersion, matchVersion, propertiesToUpdate)
                .ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TContent>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }


        //Delete
        public virtual async Task<long> DeleteMany(Expression<Func<TContent, bool>> filterConditions)
        {
            long result = await _queries.DeleteMany(filterConditions).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TContent>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }



    }
}
