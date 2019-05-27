using Sanatana.Contents.Caching.CacheProviders;
using Sanatana.Contents.Database;
using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Caching.Concrete
{
    public class CategoryRolePermissionNoCacheQueries<TKey> 
        : ICategoryRolePermissionQueries<TKey>
        where TKey : struct
    {
        //fields
        protected ICategoryRolePermissionQueries<TKey> _queries;


        //init
        public CategoryRolePermissionNoCacheQueries(
            ICategoryRolePermissionQueries<TKey> queries)
        {
            _queries = queries;
        }


        //methods
        public virtual Task InsertMany(
            IEnumerable<CategoryRolePermission<TKey>> categoryRolePermissions)
        {
            return _queries.InsertMany(categoryRolePermissions);
        }

        public virtual Task<List<CategoryRolePermission<TKey>>> SelectMany(
            Expression<Func<CategoryRolePermission<TKey>, bool>> filterConditions)
        {
            return _queries.SelectMany(x => true);
        }

        public virtual Task<long> UpdateMany(IEnumerable<CategoryRolePermission<TKey>> categoryRolePermissions)
        {
            return _queries.UpdateMany(categoryRolePermissions);
        }

        public virtual Task<long> DeleteMany(Expression<Func<CategoryRolePermission<TKey>, bool>> filterConditions)
        {
            return _queries.DeleteMany(filterConditions);
        }


    }
}
