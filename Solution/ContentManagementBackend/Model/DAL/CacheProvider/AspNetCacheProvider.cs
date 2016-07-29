using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class AspNetCacheProvider : ICacheProvider
    {      
        //поля
        protected HttpContextBase _httpContext;


        //инициализация
        public AspNetCacheProvider(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }



        //методы
        public virtual void Set(string key, object value, DateTime? expiryUtc = null)
        {
            if(expiryUtc == null)
            {
                _httpContext.Cache.Insert(key, value);
            }
            else
            {
                _httpContext.Cache.Insert(key, value
                    , null, expiryUtc.Value, System.Web.Caching.Cache.NoSlidingExpiration);
            }
           
        }

        public virtual object Get(string key)
        {
            return _httpContext.Cache[key];
        }

        public virtual void Remove(string key)
        {
            _httpContext.Cache.Remove(key);
        }

        public virtual void ClearAll()
        {
            try
            {
                foreach (System.Collections.DictionaryEntry entry in _httpContext.Cache)
                {
                    HttpContext.Current.Cache.Remove((string)entry.Key);
                }
            }
            catch
            {

            }
        }

    }
}
