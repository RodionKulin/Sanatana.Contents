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
    public class CategoryRolePermissionFullyCachedQueries<TKey> : ICategoryRolePermissionQueries<TKey>
        where TKey : struct
    {
        //fields
        protected ICategoryRolePermissionQueries<TKey> _queries;
        protected IQueryCache _queryCache;
        protected ICacheProvider _cacheProvider;


        //init
        public CategoryRolePermissionFullyCachedQueries(ICategoryRolePermissionQueries<TKey> queries
            , IQueryCache queryCache, ICacheProvider cacheProvider)
        {
            _queries = queries;
            _queryCache = queryCache;
            _cacheProvider = cacheProvider;
        }


        //methods
        public virtual async Task InsertMany(
            IEnumerable<CategoryRolePermission<TKey>> categoryRolePermissions)
        {
            await _queries.InsertMany(categoryRolePermissions).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<CategoryRolePermission<TKey>>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
        }

        public virtual async Task<List<CategoryRolePermission<TKey>>> SelectMany(
            Expression<Func<CategoryRolePermission<TKey>, bool>> filterConditions)
        {
            string cacheKey = UrnId.Create<List<CategoryRolePermission<TKey>>>();

            List<CategoryRolePermission<TKey>> categoryRolePermissions = await _queryCache
                .ToOptimizedResultUsingCache(cacheKey, () =>
                {
                    return _queries.SelectMany(x => true);
                }
                , ContentsConstants.DEFAULT_CACHE_EXPIRY_PERIOD
                , _queryCache.GetDataChangeNotifier<CategoryRolePermission<TKey>>())
                .ConfigureAwait(false);

            return categoryRolePermissions.Where(filterConditions.Compile()).ToList();
        }

        public virtual async Task<long> UpdateMany(IEnumerable<CategoryRolePermission<TKey>> categoryRolePermissions)
        {
            long result = await _queries.UpdateMany(categoryRolePermissions).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<CategoryRolePermission<TKey>>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }

        public virtual async Task<long> DeleteMany(Expression<Func<CategoryRolePermission<TKey>, bool>> filterConditions)
        {
            long result = await _queries.DeleteMany(filterConditions).ConfigureAwait(false);

            string cacheKey = UrnId.Create<List<CategoryRolePermission<TKey>>>();
            await _cacheProvider.Remove(cacheKey).ConfigureAwait(false);
            return result;
        }


    }
}
