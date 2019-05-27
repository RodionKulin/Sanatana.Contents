using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.Contents.Database;
using Sanatana.Contents.Objects.Entities;

namespace Sanatana.Contents.Selectors.Permissions
{
    public class PermissionSelector<TKey, TCategory> : IPermissionSelector<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        //fields
        protected ICategoryRolePermissionQueries<TKey> _catRolePermissionQueries;
        protected IUserRolesQueries<TKey> _userRolesQueries;
        protected IEntitiesDatabaseNameMapping _entitiesDatabaseNameMapping;


        //init
        public PermissionSelector(ICategoryRolePermissionQueries<TKey> catRolePermissionQueries
            , IUserRolesQueries<TKey> userRolesQueries, IEntitiesDatabaseNameMapping entitiesDatabaseNameMapping)
        {
            _catRolePermissionQueries = catRolePermissionQueries;
            _userRolesQueries = userRolesQueries;
            _entitiesDatabaseNameMapping = entitiesDatabaseNameMapping;
        }


        //method
        public virtual async Task<List<TKey>> SelectAllowed(long permission, TKey? userId)
        {
            List<TKey> userRoles = await _userRolesQueries.SelectUserRoles(userId);
            string categoryEntityName = _entitiesDatabaseNameMapping.GetEntityName<TCategory>();

            List<CategoryRolePermission<TKey>> allowedCategories = await _catRolePermissionQueries.SelectMany(
                p => userRoles.Contains(p.RoleId)
                && p.CategoryType == categoryEntityName
                && ((p.PermissionFlags & permission) == permission))
                .ConfigureAwait(false);

            return allowedCategories.Select(x => x.CategoryId).ToList();
        }
        
        public virtual async Task<List<TKey>> FilterAllowedCategories(
            IEnumerable<TKey> categoryIds, long permission, TKey? userId)
        {
            List<TKey> allowedCategoryIds = await SelectAllowed(permission, userId)
                .ConfigureAwait(false);

            List<TKey> filteredIds = allowedCategoryIds
                .Intersect(categoryIds)
                .ToList();

            return filteredIds;
        }
        
        public virtual async Task<bool> CheckIsAllowed(TKey categoryId, long permission, TKey? userId)
        {
            List<TKey> userRoles = await _userRolesQueries.SelectUserRoles(userId);
            string categoryEntityName = _entitiesDatabaseNameMapping.GetEntityName<TCategory>();

            List<CategoryRolePermission<TKey>> matchedPermission = await _catRolePermissionQueries.SelectMany(
                x => EqualityComparer<TKey>.Default.Equals(x.CategoryId, categoryId)
                && x.CategoryType == categoryEntityName
                && userRoles.Contains(x.RoleId)
                && ((x.PermissionFlags & permission) == permission))
                .ConfigureAwait(false);

            bool hasPermission = matchedPermission.Any();
            return hasPermission;
        }
    }
}
