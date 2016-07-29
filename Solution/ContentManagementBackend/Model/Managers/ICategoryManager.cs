using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;

namespace ContentManagementBackend
{
    public interface ICategoryManager<TKey>
        where TKey : struct
    {
        Task<MessageResult<Category<TKey>>> CheckPermission(TKey id, int permission, List<string> roles);
        Task<MessageResult<Category<TKey>>> CheckPermission(string url, int permission, List<string> roles);
        Task<QueryResult<List<Category<TKey>>>> SelectExcluded(int permission, List<string> roles);
        Task<QueryResult<List<Category<TKey>>>> SelectIncluded(int permission, List<string> roles);
        Task<QueryResult<List<Category<TKey>>>> SelectSorted();
        Task<MessageResult<List<TKey>>> FilterCategoriesPermission(
            List<TKey> ids, int permission, List<string> roles);
        bool CheckIsPublic(Category<TKey> category, int permission);
        Task<MessageResult<bool>> CheckIsPublic(TKey id, int permission);
    }
}