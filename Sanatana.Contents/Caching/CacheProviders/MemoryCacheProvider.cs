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
    public class MemoryCacheProvider : ICacheProvider
    {
        //fields
        protected object _lock = new object();
        /// <summary>
        /// key is parent and values are dependent children from parent key, that should be evicted all together with the parent eviction.
        /// </summary>
        protected Dictionary<string, HashSet<string>> _parentToChildrenDependencies;
        /// <summary>
        /// key is dependent child and values are parents that get removed one by one only on parent eviction.
        /// </summary>
        protected Dictionary<string, HashSet<string>> _childToParentsDependencies;
        protected IMemoryCache _cache;


        //init
        public MemoryCacheProvider(IMemoryCache memoryCache)
        {
            _parentToChildrenDependencies = new Dictionary<string, HashSet<string>>();
            _childToParentsDependencies = new Dictionary<string, HashSet<string>>();
            _cache = memoryCache;
        }



        //basic cache methods
        public virtual Task<bool> Add<T>(string key, T value,
            TimeSpan? expirationTime = null, List<string> dependencyKeys = null)
        {
            lock(_lock)
            {
                if (_childToParentsDependencies.TryGetValue(key, out _))
                {
                    return Task.FromResult(false);
                }

                var options = new MemoryCacheEntryOptions()
                    .RegisterPostEvictionCallback(CacheEvictionCallback);
                if (expirationTime != null)
                {
                    options = options.SetAbsoluteExpiration(expirationTime.Value);
                }
                _cache.Set(key, value, options);

                RemoveDependentChildren(key);
                UnregisterChildFromParentsDependencies(key);
                RegisterDependencies(key, dependencyKeys);
            }

            return Task.FromResult(true);
        }

        public virtual Task Set<T>(string key, T value,
            TimeSpan? expirationTime = null, List<string> dependencyKeys = null)
        {
            lock(_lock)
            {
                var options = new MemoryCacheEntryOptions()
                    .RegisterPostEvictionCallback(CacheEvictionCallback);
                if (expirationTime != null)
                {
                    options = options.SetAbsoluteExpiration(expirationTime.Value);
                }
                _cache.Set(key, value, options);

                RemoveDependentChildren(key);
                UnregisterChildFromParentsDependencies(key);
                RegisterDependencies(key, dependencyKeys);
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
            lock(_lock)
            {
                foreach (KeyValuePair<string, HashSet<string>> entry in _childToParentsDependencies)
                {
                    _cache.Remove(entry.Key);
                }

                _parentToChildrenDependencies.Clear();
                _childToParentsDependencies.Clear();
            }

            return Task.FromResult(0);
        }

        public virtual Task Remove(string key)
        {
            lock(_lock)
            {
                _cache.Remove(key);
            }

            return Task.FromResult(0);
        }

        public virtual Task Remove(IEnumerable<string> keys)
        {
            lock(_lock)
            {
                foreach (string cacheKey in keys)
                {
                    _cache.Remove(cacheKey);
                }
            }

            return Task.FromResult(0);
        }

        public virtual Task RemoveByRegex(string pattern)
        {
            Regex regex = new Regex(pattern);

            lock (_lock)
            {
                IEnumerable<string> matchedKeys = _childToParentsDependencies
                    .Select(x => x.Key)
                    .Where(x => regex.IsMatch(x))
                    .ToList();

                foreach (string cacheKey in matchedKeys)
                {
                    _cache.Remove(cacheKey);
                }
            }

            return Task.FromResult(0);
        }



        //dependency registration
        protected virtual void RegisterDependencies(string childDependentKey, 
            List<string> parentDependencyKeys = null)
        {
            _childToParentsDependencies[childDependentKey] = parentDependencyKeys == null
                ? new HashSet<string>()
                : new HashSet<string>(parentDependencyKeys);

            if (parentDependencyKeys == null)
            {
                return;
            }

            foreach (string parentKey in parentDependencyKeys)
            {
                if (_parentToChildrenDependencies.ContainsKey(parentKey))
                {
                    _parentToChildrenDependencies[parentKey].Add(childDependentKey);
                }
                else
                {
                    _parentToChildrenDependencies[parentKey] = new HashSet<string>() {
                        childDependentKey
                    };
                }
            }
        }

        /// <summary>
        /// Removed all children dependent from parent evicted cache key
        /// </summary>
        /// <param name="evictedKey"></param>
        protected virtual void RemoveDependentChildren(string evictedKey)
        {
            HashSet<string> childrenCacheKeys = _parentToChildrenDependencies[evictedKey];
            if (childrenCacheKeys != null)
            {
                foreach (string dependentChildKey in childrenCacheKeys)
                {
                    _cache.Remove(dependentChildKey);
                }
            }

            _parentToChildrenDependencies.Remove(evictedKey);
        }

        /// <summary>
        /// Remove evicted key from parents list. So when parent is evicted it won't effect the child.
        /// </summary>
        /// <param name="evictedKey"></param>
        protected virtual void UnregisterChildFromParentsDependencies(string evictedKey)
        {
            HashSet<string> parentCacheKeys = _childToParentsDependencies[evictedKey];
            if (parentCacheKeys != null)
            {
                foreach (string parent in parentCacheKeys)
                {
                    HashSet<string> childrenCacheKeys;
                    _parentToChildrenDependencies.TryGetValue(parent, out childrenCacheKeys);
                    if (childrenCacheKeys != null)
                    {
                        childrenCacheKeys.Remove(evictedKey);
                    }
                }
            }

            _childToParentsDependencies.Remove(evictedKey);
        }


        //Callback
        protected virtual void CacheEvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            string evictedKey = (string)key;
            lock (_lock)
            {
                RemoveDependentChildren(evictedKey);
                UnregisterChildFromParentsDependencies(evictedKey);
            }
        }


        //IDisposable
        public virtual void Dispose()
        {
            _cache.Dispose();
        }
    }
}
