using Sanatana.Contents;
using Sanatana.Contents.Caching.CacheProviders;
using Sanatana.Contents.Database;
using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching.Concrete
{
    public class CategoryFullyCachedQueries<TKey, TCategory> : ICategoryQueries<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        //fields
        protected ICategoryQueries<TKey, TCategory> _queries;
        protected IQueryCache _queryCache;
        protected ICacheProvider _cacheProvider;

        //init
        public CategoryFullyCachedQueries(ICategoryQueries<TKey, TCategory> queries
            , IQueryCache queryCache, ICacheProvider cacheProvider)
        {
            _queries = queries;
            _queryCache = queryCache;
            _cacheProvider = cacheProvider;
        }


        //methods
        public virtual async Task InsertMany(IEnumerable<TCategory> categories)
        {
            await _queries.InsertMany(categories).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TCategory>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
        }

        public virtual async Task<List<TCategory>> SelectMany(Expression<Func<TCategory, bool>> filterConditions)
        {
            string cacheKey = UrnId.Create<List<TCategory>>();

            List<TCategory> categories = await _queryCache
                .ToOptimizedResultUsingCache(cacheKey, () =>
                {
                    return _queries.SelectMany(x => true);
                }
                , ContentsConstants.DEFAULT_CACHE_EXPIRY_PERIOD
                , _queryCache.GetDataChangeNotifier<TCategory>())
                .ConfigureAwait(false);
            
            return categories.Where(filterConditions.Compile()).ToList();
        }

        public virtual async Task<long> UpdateMany(IEnumerable<TCategory> categories
            , params Expression<Func<TCategory, object>>[] propertiesToUpdate)
        {
            long result = await _queries.UpdateMany(categories, propertiesToUpdate).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TCategory>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }
        
        public virtual async Task<long> DeleteMany(Expression<Func<TCategory, bool>> filterConditions)
        {
            long result = await _queries.DeleteMany(filterConditions).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<TCategory>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }

    }
}
