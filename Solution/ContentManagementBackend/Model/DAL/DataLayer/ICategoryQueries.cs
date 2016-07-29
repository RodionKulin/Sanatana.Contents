using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public interface ICategoryQueries<TKey>
        where TKey : struct
    {
        Task<QueryResult<List<Category<TKey>>>> Select();
        Task<bool> Insert(Category<TKey> category);
        Task<bool> Update(Category<TKey> category);
        Task<bool> Delete(TKey categoryID);
    }
}
