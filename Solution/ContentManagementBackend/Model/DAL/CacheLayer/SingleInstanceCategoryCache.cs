using ContentManagementBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility;
using System.Web;

namespace ContentManagementBackend
{
    public class SingleInstanceCategoryCache<TKey> : ICategoryQueries<TKey>
        where TKey : struct
    {
        //поля
        protected ICategoryQueries<TKey> _queries;
        protected HttpContextBase _httpContext;
        protected ICacheProvider _cache;


        //свойства
        protected virtual QueryResult<List<Category<TKey>>> PersistentCategories
        {
            get
            {
                return (QueryResult<List<Category<TKey>>>)
                    _cache.Get(Constants.CACHE_CATEGORIES_KEY);
            }
            set
            {
                _cache.Set(Constants.CACHE_CATEGORIES_KEY, value);
            }
        }
        protected virtual QueryResult<List<Category<TKey>>> RequestCategories
        {
            get
            {
                return (QueryResult<List<Category<TKey>>>)
                    _httpContext.Items[Constants.CACHE_CATEGORIES_KEY];
            }
            set
            {
                _httpContext.Items[Constants.CACHE_CATEGORIES_KEY] = value;
            }
        }


        //инициализация
        public SingleInstanceCategoryCache(ICategoryQueries<TKey> queries, ICacheProvider cache, HttpContextBase httpContext)
        {
            _queries = queries;
            _cache = cache;
            _httpContext = httpContext;
        }


        //методы
        public async Task<bool> Insert(Category<TKey> category)
        {
            bool result = await _queries.Insert(category);

            _httpContext.Items.Remove(Constants.CACHE_CATEGORIES_KEY);
            _cache.Remove(Constants.CACHE_CATEGORIES_KEY);

            return result;
        }

        public async Task<QueryResult<List<Category<TKey>>>> Select()
        {
            if (RequestCategories != null)
            {
                return RequestCategories;
            }

            QueryResult<List<Category<TKey>>> result = PersistentCategories == null
                ? await _queries.Select().ConfigureAwait(false)
                : PersistentCategories;

            RequestCategories = result;
            if (!result.HasExceptions)
            {
                PersistentCategories = result;
            }
            
            return result;
        }

        public async Task<bool> Update(Category<TKey> category)
        {
            bool result = await _queries.Update(category);

            _httpContext.Items.Remove(Constants.CACHE_CATEGORIES_KEY);
            _cache.Remove(Constants.CACHE_CATEGORIES_KEY);

            return result;
        }

        public async Task<bool> Delete(TKey categoryID)
        {
            bool result = await _queries.Delete(categoryID);

            _httpContext.Items.Remove(Constants.CACHE_CATEGORIES_KEY);
            _cache.Remove(Constants.CACHE_CATEGORIES_KEY);

            return result;
        }

    }
}
