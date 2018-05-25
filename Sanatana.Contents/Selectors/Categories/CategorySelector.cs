using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Patterns.Pipelines;
using System.Linq.Expressions;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Contents.Database;
using Sanatana.Contents.Selectors.Comments;
using Sanatana.Contents.Objects.Entities;

namespace Sanatana.Contents.Selectors.Categories
{
    public class CategorySelector<TKey, TCategory> : ICategorySelector<TKey, TCategory>
        where TKey : struct
        where TCategory : Category<TKey>
    {
        //fields
        protected ICategoryQueries<TKey, TCategory> _categoryQueries;
        protected IPermissionSelector<TKey, TCategory> _permissionSelector;


        //init
        public CategorySelector(ICategoryQueries<TKey, TCategory> categoryQueries
            , IPermissionSelector<TKey, TCategory> permissionSelector)
        {
            _categoryQueries = categoryQueries;
            _permissionSelector = permissionSelector;
        }


        //methods        
        public virtual List<ParentVM<TCategory>> Sort(List<TCategory> categories)
        {
            Dictionary<TKey?, List<TCategory>> categoryGroups = categories
                .GroupBy(p => p.ParentCategoryId)
                .ToDictionary(x => x.Key, x => x.ToList());

            if (categoryGroups.ContainsKey(null) == false)
            {
                return new List<ParentVM<TCategory>>();
            }

            List<TCategory> rootGroup = categoryGroups[null];
            List<ParentVM<TCategory>> categoryHierarchy =
                SetupCategoryHierarchy(rootGroup, categoryGroups);

            return categoryHierarchy;
        }

        protected virtual List<ParentVM<TCategory>> SetupCategoryHierarchy(
            List<TCategory> categories, Dictionary<TKey?, List<TCategory>> categoryGroups)
        {
            List<ParentVM<TCategory>> vmList = new List<ParentVM<TCategory>>();

            foreach (TCategory category in categories)
            {
                ParentVM<TCategory> categoryVm = new ParentVM<TCategory>
                {
                    Item = category
                };
                vmList.Add(categoryVm);

                if (categoryGroups.ContainsKey(category.CategoryId) == false)
                {
                    continue;
                }

                List<TCategory> childItems = categoryGroups[category.CategoryId];
                categoryVm.Children = SetupCategoryHierarchy(childItems, categoryGroups);
                categoryVm.HasChildren = true;
            }

            return vmList
                .OrderByDescending(x => x.Item.SortOrder)
                .ThenBy(p => p.Item.AddTimeUtc)
                .ToList();
        }

        public virtual List<ParentVM<TCategory>> SortFlat(
            List<TCategory> categories, bool onlyApexCategories)
        {
            List<ParentVM<TCategory>> categoryHierarchy = Sort(categories);
            categoryHierarchy = FlattenChildren(categoryHierarchy);

            //exclude parents with 1 or more children
            if (onlyApexCategories)
            {
                IEnumerable<TKey?> parentCategoryIds = categories
                    .Select(x => x.ParentCategoryId)
                    .Where(x => x != null)
                    .Distinct();

                categoryHierarchy.RemoveAll(x => parentCategoryIds.Contains(x.Item.CategoryId));
            }

            return categoryHierarchy;
        }

        protected virtual List<ParentVM<TCategory>> FlattenChildren(List<ParentVM<TCategory>> categories)
        {
            var flatResult = new List<ParentVM<TCategory>>();

            foreach (ParentVM<TCategory> category in categories)
            {
                flatResult.Add(category);

                if (category.Children != null)
                {
                    List<ParentVM<TCategory>> childrenFlat = FlattenChildren(category.Children);
                    flatResult.AddRange(childrenFlat);
                }
                category.Children = null;
            }

            return flatResult;
        }

        public virtual async Task<bool> CheckPermission(string url, long permission, TKey? userId)
        {
            url = (url ?? string.Empty).ToLowerInvariant();

            List<TCategory> matchedList = await _categoryQueries
                .SelectMany(x => x.Url == url)
                .ConfigureAwait(false);

            if (matchedList.Count == 0)
            {
                return false;
            }
            if(matchedList.Count > 1)
            {
                throw new NotSupportedException($"More than one {nameof(TCategory)} found with same url {url}");
            }

            bool allowed = await _permissionSelector
                .CheckIsAllowed(matchedList[0].CategoryId, permission, userId)
                .ConfigureAwait(false);

            return allowed;
        }

        public virtual async Task<List<TCategory>> SelectAllowed(long permission, TKey? userId)
        {
            List<TKey> allowedCategoryIds = await _permissionSelector
                .SelectAllowed(permission, userId)
                .ConfigureAwait(false);

            List<TCategory> allowedCategories = await _categoryQueries
                .SelectMany(x => allowedCategoryIds.Contains(x.CategoryId))
                .ConfigureAwait(false);

            return allowedCategories;
        }

        public virtual async Task<List<TCategory>> SelectForbidden(long permission, TKey? userId)
        {
            List<TKey> allowedCategoryIds = await _permissionSelector
                .SelectAllowed(permission, userId)
                .ConfigureAwait(false);

            List<TCategory> forbiddenCategories = await _categoryQueries
                .SelectMany(x => allowedCategoryIds.Contains(x.CategoryId) == false)
                .ConfigureAwait(false);

            return forbiddenCategories;
        }

        public virtual async Task<List<TKey>> SelectForbiddenIds(long permission, TKey? userId)
        {
            List<TCategory> forbiddenCategories = 
                await SelectForbidden(permission, userId)
                .ConfigureAwait(false);

            return forbiddenCategories.Select(x => x.CategoryId).ToList();
        }

    }
}
