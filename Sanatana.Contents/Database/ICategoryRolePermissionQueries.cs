using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Database
{
    public interface ICategoryRolePermissionQueries<TKey>
        where TKey : struct
    {
        Task InsertMany(IEnumerable<CategoryRolePermission<TKey>> categoryRolePermissions);
        Task<List<CategoryRolePermission<TKey>>> SelectMany(Expression<Func<CategoryRolePermission<TKey>, bool>> filterConditions);
        Task<long> UpdateMany(IEnumerable<CategoryRolePermission<TKey>> categoryRolePermissions);
        Task<long> DeleteMany(Expression<Func<CategoryRolePermission<TKey>, bool>> filterConditions);
    }
}
