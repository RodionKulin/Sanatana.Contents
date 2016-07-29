using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class CategoryManager<TKey> : ICategoryManager<TKey>
        where TKey : struct
    {

        //свойства
        public ICategoryQueries<TKey> CategoryQueries { get; set; }
        public bool ExcludeParents { get; set; }



        //инициализация
        public CategoryManager(ICategoryQueries<TKey> categoryQueries, bool excludeParents = true)
        {
            CategoryQueries = categoryQueries;
            ExcludeParents = excludeParents;
        }


        //методы        
        public virtual async Task<QueryResult<List<Category<TKey>>>> SelectSorted()
        {
            List<Category<TKey>> result = new List<Category<TKey>>();
            QueryResult<List<Category<TKey>>> categoryResult = await CategoryQueries.Select();

            //sort by SortOrder
            IEnumerable<IGrouping<TKey?, Category<TKey>>> groups = categoryResult.Result
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.AddTimeUtc)
                .GroupBy(p => p.ParentCategoryID);

            //sort by parent
            IGrouping<TKey?, Category<TKey>> parents = groups
                .FirstOrDefault(p => p.Key == null);

            if (parents != null)
            {
                foreach (Category<TKey> category in parents)
                {
                    result.Add(category);

                    IGrouping<TKey?, Category<TKey>> children = groups.FirstOrDefault(
                        p => p.Key.HasValue 
                        && EqualityComparer<TKey>.Default.Equals(p.Key.Value, category.CategoryID));

                    if (children != null)
                    {
                        result.AddRange(children);
                    }
                }
            }

            //filter ExcludeParents
            if (ExcludeParents)
            {
                List<TKey> childGroupKeys = groups
                    .Where(p => p.Key != null)
                    .Select(p => p.Key.Value)
                    .ToList();

                result.RemoveAll(p => childGroupKeys.Contains(p.CategoryID));
            }

            return new QueryResult<List<Category<TKey>>>(result, categoryResult.HasExceptions);
        }

        public virtual async Task<QueryResult<List<Category<TKey>>>> SelectIncluded(
            int permission, List<string> roles)
        {
            QueryResult<List<Category<TKey>>> categoryResult = await SelectSorted();

            List<Category<TKey>> result = categoryResult.Result.Where(
                p => p.Permissions == null
                || p.Permissions.All(a => a.Key != permission)
                || p.Permissions.Any(a => a.Key == permission && roles.Contains(a.Value))
            ).ToList();
            
            return new QueryResult<List<Category<TKey>>>(result, categoryResult.HasExceptions);
        }
        
        public virtual async Task<QueryResult<List<Category<TKey>>>> SelectExcluded(
            int permission, List<string> roles)
        {
            QueryResult<List<Category<TKey>>> categoryResult = await SelectSorted();

            List<Category<TKey>> result = categoryResult.Result.Where(
                p => p.Permissions != null
                && p.Permissions.Any(a => a.Key == permission)
                && p.Permissions.Where(w => w.Key == permission).All(a => !roles.Contains(a.Value))
            ).ToList();
            
            return new QueryResult<List<Category<TKey>>>(result, categoryResult.HasExceptions);
        }

        public virtual async Task<MessageResult<List<TKey>>> FilterCategoriesPermission(
            List<TKey> ids, int permission, List<string> roles)
        {
            QueryResult<List<Category<TKey>>> categoryResult = await SelectIncluded(permission, roles);
            if (categoryResult.HasExceptions)
            {
                return new MessageResult<List<TKey>>(null, MessageResources.Common_DatabaseException, true);
            }

            List<TKey> filteredIDs = categoryResult.Result
                .Select(p => p.CategoryID).Intersect(ids).ToList();

            return new MessageResult<List<TKey>>(filteredIDs, null, false);
        }

        public virtual async Task<MessageResult<Category<TKey>>> CheckPermission(
            string url, int permission, List<string> roles)
        {
            QueryResult<List<Category<TKey>>> categoryResult = await CategoryQueries.Select();
            if (categoryResult.HasExceptions)
            {
                return new MessageResult<Category<TKey>>(null, MessageResources.Common_DatabaseException, true);
            }

            url = (url ?? string.Empty).ToLowerInvariant();
            Category<TKey> category = categoryResult.Result.FirstOrDefault(p => 
                p.Url.ToLowerInvariant() == url);
            if (category == null)
            {
                return new MessageResult<Category<TKey>>(null, MessageResources.Common_CategoryNotFound, true);
            }
            
            if (category.Permissions != null
                && category.Permissions.Any(a => a.Key == permission)
                && category.Permissions.Where(w => w.Key == permission).All(a => !roles.Contains(a.Value))
                )
            {
                return new MessageResult<Category<TKey>>(null, MessageResources.Common_AuthorizationRequired, true);
            }
            
            return new MessageResult<Category<TKey>>(category, null, false);
        }

        public virtual async Task<MessageResult<Category<TKey>>> CheckPermission(
            TKey id, int permission, List<string> roles)
        {
            QueryResult<List<Category<TKey>>> categoryResult = await CategoryQueries.Select();
            if (categoryResult.HasExceptions)
            {
                return new MessageResult<Category<TKey>>(null, MessageResources.Common_DatabaseException, true);
            }

            Category<TKey> category = categoryResult.Result.FirstOrDefault(p =>
                EqualityComparer<TKey>.Default.Equals(p.CategoryID, id));
            if (category == null)
            {
                return new MessageResult<Category<TKey>>(null, MessageResources.Common_CategoryNotFound, true);
            }

            if (category.Permissions != null    //zero roles means it's public
                && category.Permissions.Any(a => a.Key == permission) //zero roles means it's public
                && category.Permissions.Where(w => w.Key == permission).All(a => !roles.Contains(a.Value)) //permission is not enough
                )
            {
                return new MessageResult<Category<TKey>>(null, MessageResources.Common_AuthorizationRequired, true);
            }
            
            return new MessageResult<Category<TKey>>(category, null, false);
        }

        public virtual bool CheckIsPublic(Category<TKey> category, int permission)
        {
            return category.Permissions == null    //zero roles means it's public
                || category.Permissions.All(a => a.Key != permission);
        }

        public virtual async Task<MessageResult<bool>> CheckIsPublic(
            TKey id, int permission)
        {
            QueryResult<List<Category<TKey>>> categoryResult = await CategoryQueries.Select();
            if (categoryResult.HasExceptions)
            {
                return new MessageResult<bool>(false, MessageResources.Common_DatabaseException, true);
            }

            Category<TKey> category = categoryResult.Result.FirstOrDefault(p =>
                EqualityComparer<TKey>.Default.Equals(p.CategoryID, id));
            if (category == null)
            {
                return new MessageResult<bool>(false, MessageResources.Common_CategoryNotFound, true);
            }

            bool isPublic = CheckIsPublic(category, permission);
            return new MessageResult<bool> (isPublic, null, false);
        }

    }
}
