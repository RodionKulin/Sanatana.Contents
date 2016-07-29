using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public interface ISearchQueries<TKey>
        where TKey : struct
    {
        Task<bool> Insert(ContentBase<TKey> item);
        Task<bool> Insert(List<ContentBase<TKey>> items);
        Task<SearchResponse<TKey>> Find(
            string searchTerm, int page, int pageSize, bool highlight, TKey? category);
        Task<bool> Update(ContentBase<TKey> item);
        Task<bool> Delete(TKey id);
        Task<bool> Optimize();
    }
}
