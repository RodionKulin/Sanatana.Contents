using Sanatana.Contents.Caching.DataChangeNotifiers;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching
{
    public interface IQueryCache
    {
        IDataChangeNotifier GetDataChangeNotifier<T>(Expression<Func<T, bool>> filter = null);
        Task<T> ToOptimizedResultUsingCache<T>(string cacheKey, Func<Task<T>> selector, TimeSpan? expirationTime = null);
        Task<T> ToOptimizedResultUsingCache<T>(string cacheKey, Func<Task<T>> selector, TimeSpan? expirationTime = null, params IDataChangeNotifier[] changeNotifiers);
    }
}