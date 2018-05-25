using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Selectors.Comments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanatana.Contents.Selectors.Categories
{
    public interface ICategorySelector<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        Task<bool> CheckPermission(string url, long permission, TKey? userId);
        Task<List<TCategory>> SelectAllowed(long permission, TKey? userId);
        Task<List<TCategory>> SelectForbidden(long permission, TKey? userId);
        Task<List<TKey>> SelectForbiddenIds(long permission, TKey? userId);
        List<ParentVM<TCategory>> Sort(List<TCategory> categories);
        List<ParentVM<TCategory>> SortFlat(List<TCategory> categories, bool onlyApexCategories);
    }
}