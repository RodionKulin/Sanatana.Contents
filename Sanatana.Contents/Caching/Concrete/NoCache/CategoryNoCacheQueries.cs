using Sanatana.Contents.Database;
using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching.Concrete
{
    public class CategoryNoCacheQueries<TKey, TCategory> : ICategoryQueries<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        //fields
        protected ICategoryQueries<TKey, TCategory> _queries;


        //init
        public CategoryNoCacheQueries(ICategoryQueries<TKey, TCategory> queries)
        {
            _queries = queries;
        }


        //methods
        public virtual Task InsertMany(IEnumerable<TCategory> categories)
        {
            return _queries.InsertMany(categories);
        }

        public virtual Task<List<TCategory>> SelectMany(
            Expression<Func<TCategory, bool>> filterConditions)
        {
            return _queries.SelectMany(filterConditions);
        }

        public virtual Task<long> UpdateMany(IEnumerable<TCategory> categories
            , params Expression<Func<TCategory, object>>[] propertiesToUpdate)
        {
            return _queries.UpdateMany(categories, propertiesToUpdate);
        }

        public virtual Task<long> DeleteMany(Expression<Func<TCategory, bool>> filterConditions)
        {
            return _queries.DeleteMany(filterConditions);
        }

    }
}
