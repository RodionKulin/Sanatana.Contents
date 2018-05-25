using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search
{
    public interface ISearchQueries<TKey>
        where TKey : struct
    {
        Task Insert(List<object> items);
        Task<List<object>> Suggest(string input, int page, int pageSize);
        Task<object> FindById(TKey id, Type indexType);
        Task<SearchResult<object>> FindByInput(SearchParams parameters);
        Task Update(List<object> items);
        Task Delete(List<object> itemsWithIds);
    }
}
