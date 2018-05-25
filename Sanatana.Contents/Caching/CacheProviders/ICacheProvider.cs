using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching.CacheProviders
{
    public interface ICacheProvider : IDisposable
    {
        /// <summary>
        /// Adds a new item into the cache at the specified cache key only if the cache is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationTime"></param>
        /// <param name="dependencyKeys"></param>
        /// <returns>Returns true is item was added, otherwise false.</returns>
        Task<bool> Add<T>(string key, T value, TimeSpan? expirationTime = null, List<string> relatedKeys = null);

        /// <summary>
        /// Sets an item into the cache at the cache key specified regardless if it already exists or not.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationTime"></param>
        /// <param name="dependencyKeys"></param>
        /// <returns></returns>
        Task Set<T>(string key, T value, TimeSpan? expirationTime = null, List<string> dependencyKeys = null);

        /// <summary>
        /// Retrieves the specified item from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> Get<T>(string key);

        /// <summary>
        /// Invalidates all data on the cache.
        /// </summary>
        /// <returns></returns>
        Task Clear();

        /// <summary>
        /// Removes the specified item from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>        
        Task Remove(string key);

        /// <summary>
        /// Removes the cache for all the keys provided.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Task Remove(IEnumerable<string> keys);

        /// <summary>
        /// Removes items from the cache based on the specified regular expression pattern
        /// </summary>
        /// <param name="pattern">Regular expression pattern to search cache keys</param>
        Task RemoveByRegex(string pattern);
    }
}