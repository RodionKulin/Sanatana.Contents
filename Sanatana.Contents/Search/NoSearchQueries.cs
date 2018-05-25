using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanatana.Contents.Search
{
    public class NoSearchQueries<TKey> : ISearchQueries<TKey>
        where TKey : struct
    {

        //methods
        public Task Insert(List<object> items)
        {
            return Task.FromResult(true);
        }

        public Task<object> FindById(TKey id, Type indexType)
        {
            throw new NotImplementedException();
        }

        public Task<SearchResult<object>> FindByInput(SearchParams parameters)
        {
            throw new NotImplementedException();
        }

        public Task<List<object>> Suggest(string input, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task Update(List<object> items)
        {
            return Task.FromResult(true);
        }

        public Task Delete(List<object> itemsWithIds)
        {
            return Task.FromResult(true);
        }

    }
}