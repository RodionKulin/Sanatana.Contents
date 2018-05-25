using Sanatana.Contents.Objects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanatana.Contents.Selectors.Permissions
{
    public interface IPermissionSelector<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        Task<bool> CheckIsAllowed(TKey categoryId, long permission, TKey? userId);
        Task<List<TKey>> FilterAllowedCategories(IEnumerable<TKey> categoryIds, long permission, TKey? userId);
        Task<List<TKey>> SelectAllowed(long permission, TKey? userId);
    }
}