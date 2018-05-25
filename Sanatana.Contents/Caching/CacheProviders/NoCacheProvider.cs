using Sanatana.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching.CacheProviders
{
    public class NoCacheProvider: ICacheProvider
    {

        //methods
        public virtual Task<bool> Add<T>(string key, T value
            , TimeSpan? expirationTime = null, List<string> dependencyKeys = null)
        {
            return Task.FromResult(true);
        }

        public virtual Task Set<T>(string key, T value
            , TimeSpan? expirationTime = null, List<string> dependencyKeys = null)
        {
            return Task.FromResult(0);
        }

        public virtual Task<T> Get<T>(string key)
        {
            T empty = default(T);
            return Task.FromResult<T>(empty);
        }

        public virtual Task Clear()
        {
            return Task.FromResult(0);
        }

        public virtual Task Remove(string key)
        {
            return Task.FromResult(0);
        }

        public Task Remove(IEnumerable<string> keys)
        {
            return Task.FromResult(0);
        }
        
        public Task RemoveByRegex(string pattern)
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {

        }
    }
}
