using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Sanatana.Contents.Caching.CacheProviders
{
    public class MemoryPersistentCacheProvider : ICacheProvider
    {
        //fields
        protected HashSet<string> _allCacheKeys = new HashSet<string>();
        protected Dictionary<string, List<string>> _depenencies = new Dictionary<string, List<string>>();
        protected object _dependenciesLock = new object();
        protected object _allKeysLock = new object();
        protected IMemoryCache _cache;


        //init
        public MemoryPersistentCacheProvider(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }



        //basic cache methods
        public virtual Task<bool> Add<T>(string key, T value
            , TimeSpan? expirationTime = null, List<string> dependFromKeys = null)
        {
            lock(_allKeysLock)
            {
                if (_allCacheKeys.Contains(key) == true)
                {
                    return Task.FromResult(false);
                }
                else
                {
                    _allCacheKeys.Add(key);
                }

                var options = new MemoryCacheEntryOptions()
                    .RegisterPostEvictionCallback(CacheEvictionCallback);
                if (expirationTime != null)
                {
                    options = options.SetAbsoluteExpiration(expirationTime.Value);
                }
                _cache.Set(key, value, options);

                if (dependFromKeys != null)
                {
                    RegisterDependencies(key, dependFromKeys);
                }
            }

            return Task.FromResult(true);
        }

        public virtual Task Set<T>(string key, T value
            , TimeSpan? expirationTime = null, List<string> dependFromKeys = null)
        {
            lock (_allKeysLock)
            {
                if (_allCacheKeys.Contains(key) == false)
                {
                    _allCacheKeys.Add(key);
                }

                var options = new MemoryCacheEntryOptions()
                    .RegisterPostEvictionCallback(CacheEvictionCallback);
                if (expirationTime != null)
                {
                    options = options.SetAbsoluteExpiration(expirationTime.Value);
                }
                _cache.Set(key, value, options);

                if (dependFromKeys != null)
                {
                    RegisterDependencies(key, dependFromKeys);
                }
            }

            return Task.FromResult(0);
        }

        public virtual Task<T> Get<T>(string key)
        {
            T value = (T)_cache.Get(key);
            return Task.FromResult(value);
        }

        public virtual Task Clear()
        {
            lock(_allKeysLock)
            {
                foreach (string cacheKey in _allCacheKeys)
                {
                    _cache.Remove(cacheKey);
                }
                _allCacheKeys.Clear();
            }

            return Task.FromResult(0);
        }

        public virtual Task Remove(string key)
        {
            lock (_allKeysLock)
            {
                _cache.Remove(key);
                _allCacheKeys.Remove(key);
            }

            return Task.FromResult(0);
        }

        public virtual Task Remove(IEnumerable<string> keys)
        {
            lock (_allKeysLock)
            {
                foreach (string cacheKey in keys)
                {
                    _cache.Remove(cacheKey);

                    string removeKey = cacheKey;
                    _allCacheKeys.Remove(removeKey);
                }
            }

            return Task.FromResult(0);
        }

        public virtual Task RemoveByRegex(string pattern)
        {
            lock (_allKeysLock)
            {
                Regex regex = new Regex(pattern);
                IEnumerable<string> matchedKeys = _allCacheKeys
                    .Where(x => regex.IsMatch(x))
                    .ToList();

                foreach (string cacheKey in matchedKeys)
                {
                    _cache.Remove(cacheKey);

                    string removeKey = cacheKey;
                    _allCacheKeys.Remove(removeKey);
                }
            }

            return Task.FromResult(0);
        }



        //dependency methods
        protected virtual void RegisterDependencies(string dependentKey, List<string> dependFromKeys)
        {
            lock (_dependenciesLock)
            {
                foreach (string key in dependFromKeys)
                {
                    if (_depenencies.ContainsKey(key) == false)
                    {
                        _depenencies[key] = new List<string>();
                    }
                    _depenencies[key].Add(dependentKey);
                }
            }
        }

        protected virtual void CacheEvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            lock (_dependenciesLock)
            {
                string evictedKey = (string)key;

                if (_depenencies.ContainsKey(evictedKey))
                {
                    List<string> dependentCacheKeys = _depenencies[evictedKey];

                    lock (_allCacheKeys)
                    {
                        foreach (string dependentKey in dependentCacheKeys)
                        {
                            _cache.Remove(dependentKey);
                            _allCacheKeys.Remove(dependentKey);
                        }
                    }

                    _depenencies.Remove(evictedKey);
                }
            }
        }



        //IDisposable
        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}
