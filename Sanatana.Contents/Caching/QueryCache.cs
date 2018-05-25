using Sanatana.Contents.Caching.CacheProviders;
using Sanatana.Contents.Caching.DataChangeNotifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching
{
    public class QueryCache : IQueryCache
    {
        //fields
        protected ICacheProvider _cacheProvider;
        protected IDataChangeNotifiersRegistry _changeNotifiersRegistry;
        protected IDataChangeNotifierFactory _changeNotifierFactory;


        //init
        public QueryCache(ICacheProvider cacheProvider, IDataChangeNotifiersRegistry changeNotifiersRegistry
            , IDataChangeNotifierFactory changeNotifierFactory)
        {
            _cacheProvider = cacheProvider;
            _changeNotifiersRegistry = changeNotifiersRegistry;
            _changeNotifierFactory = changeNotifierFactory;
        }


        //methods
        public virtual async Task<T> ToOptimizedResultUsingCache<T>(string cacheKey
            , Func<Task<T>> selector, TimeSpan? expirationTime = null)
        {
            T value = await _cacheProvider.Get<T>(cacheKey).ConfigureAwait(false);
            if (value != null)
            {
                return value;
            }

            value = await selector().ConfigureAwait(false);
            await _cacheProvider.Set(cacheKey, value, expirationTime).ConfigureAwait(false);

            return value;
        }

        public virtual async Task<T> ToOptimizedResultUsingCache<T>(string cacheKey
            , Func<Task<T>> selector, TimeSpan? expirationTime = null
            , params IDataChangeNotifier[] changeNotifiers)
        {
            T value = await ToOptimizedResultUsingCache<T>(cacheKey, selector, expirationTime).ConfigureAwait(false);

            foreach (IDataChangeNotifier changeNotifier in changeNotifiers)
            {
                await _changeNotifiersRegistry.Register(changeNotifier, cacheKey).ConfigureAwait(false);
            }

            return value;
        }

        public virtual IDataChangeNotifier GetDataChangeNotifier<T>(
            Expression<Func<T, bool>> filter = null)
        {
            return _changeNotifierFactory.Create<T>(filter);
        }
    }
}
