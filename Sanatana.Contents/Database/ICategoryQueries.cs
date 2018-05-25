using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database
{
    public interface ICategoryQueries<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        Task InsertMany(IEnumerable<TCategory> categories);
        Task<List<TCategory>> SelectMany(Expression<Func<TCategory, bool>> filterConditions);
        Task<long> UpdateMany(IEnumerable<TCategory> categories,
           params Expression<Func<TCategory, object>>[] propertiesToUpdate);
        Task<long> DeleteMany(Expression<Func<TCategory, bool>> filterConditions);
    }
}
