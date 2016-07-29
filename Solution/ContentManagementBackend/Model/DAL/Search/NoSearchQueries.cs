using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Common.Utility;

namespace ContentManagementBackend
{
    public class NoSearchQueries<TKey> : ISearchQueries<TKey>
        where TKey : struct
    {

        //методы
        public Task<bool> Delete(TKey id)
        {
            return Task.FromResult(true);
        }

        public Task<SearchResponse<TKey>> Find(
            string searchTerm, int page, int pageSize, bool highlight, TKey? category)
        {
            return Task.FromResult(new SearchResponse<TKey>());
        }

        public Task<bool> Insert(ContentBase<TKey> post)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Insert(List<ContentBase<TKey>> posts)
        {
            return Task.FromResult(true);
        }
        
        public Task<bool> Update(ContentBase<TKey> post)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Optimize()
        {
            return Task.FromResult(true);
        }
    }
}