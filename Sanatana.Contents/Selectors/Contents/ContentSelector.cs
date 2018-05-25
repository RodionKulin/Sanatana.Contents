using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sanatana.Patterns.Pipelines;
using System.Linq.Expressions;
using LinqKit;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Database;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Contents.Selectors.Categories;

namespace Sanatana.Contents.Selectors.Contents
{
    public class ContentSelector<TKey, TCategory, TContent> 
        : IContentSelector<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        //fields
        protected ICategoryQueries<TKey, TCategory> _categoryQueries;
        protected IContentQueries<TKey, TContent> _contentQueries;
        protected IPermissionSelector<TKey, TCategory> _permissionSelector;
        protected ICategorySelector<TKey, TCategory> _categorySelector;



        //init
        public ContentSelector(ICategoryQueries<TKey, TCategory> categoryQueries, IContentQueries<TKey, TContent> contentQueries
            , ICategorySelector<TKey, TCategory> categorySelector, IPermissionSelector<TKey, TCategory> permissionSelector)
        {
            _categoryQueries = categoryQueries;
            _contentQueries = contentQueries;
            _categorySelector = categorySelector;
            _permissionSelector = permissionSelector;
        }


        //Select
        public virtual async Task<ContentRelatedVM<TKey, TCategory, TContent>> SelectToRead(
            string contentUrl, long permission, TKey? userId, bool incrementViewCount
            , Expression<Func<TContent, bool>> filterConditions)
        {
            //validation
            if (string.IsNullOrEmpty(contentUrl))
            {
                return ContentRelatedVM<TKey, TCategory, TContent>.NotFound();
            }
            contentUrl = contentUrl.ToLowerInvariant();
            filterConditions = filterConditions.And(x => x.Url == contentUrl);

            //queries
            Task<TContent> contentTask = _contentQueries.SelectOne(
                incrementViewCount, DataAmount.FullContent, filterConditions);
            Task<List<TCategory>> allowedCategoriesTask = _categorySelector
                .SelectAllowed(permission, userId);

            TContent content = await contentTask.ConfigureAwait(false);
            List<TCategory> allowedCategories = await allowedCategoriesTask.ConfigureAwait(false);

            if (content == null)
            {
                return ContentRelatedVM<TKey, TCategory, TContent>.NotFound();
            }

            //permission
            TCategory contentCategory = allowedCategories
               .FirstOrDefault(p => EqualityComparer<TKey>.Default.Equals(p.CategoryId, content.CategoryId));
            if (contentCategory == null)
            {
                //content category was not found among allowed categories
                return ContentRelatedVM<TKey, TCategory, TContent>.PermissionDenied();
            }

            return ContentRelatedVM<TKey, TCategory, TContent>.Success(content, contentCategory, allowedCategories);
        }

        public virtual async Task<ContentRelatedVM<TKey, TCategory, TContent>> SelectToRead(
            string contentUrl, long permission, TKey? userId, bool incrementViewCount, List<ContentRelatedQuery> queries
            , Expression<Func<TContent, bool>> filterConditions
            , int? queryPageSize = null, TimeSpan? topViewsSelectPeriod = null)
        {
            //validation
            if ((queries.Contains(ContentRelatedQuery.SameCategoryTopViewContents)
                || queries.Contains(ContentRelatedQuery.SameCategoryTopViewContents))
                && queryPageSize == null)
            {
                throw new KeyNotFoundException($"Parameter {nameof(queryPageSize)} was null but is expected to have a value.");
            }
            if (queries.Contains(ContentRelatedQuery.SameCategoryTopViewContents)
                && topViewsSelectPeriod == null)
            {
                throw new KeyNotFoundException($"Parameter {nameof(topViewsSelectPeriod)} was null but is expected to have a value.");
            }

            //query content by url
            ContentRelatedVM<TKey, TCategory, TContent> vm = await 
                SelectToRead(contentUrl, permission, userId, incrementViewCount, filterConditions)
                .ConfigureAwait(false);
            if (vm.Status != OperationStatus.Success)
            {
                return vm;
            }

            //filter
            List<TKey> categoryIds = new List<TKey>() { vm.Content.CategoryId };
            filterConditions = await AddCategoryFilter(permission, userId, filterConditions, categoryIds)
                .ConfigureAwait(false);

            DateTime publishDateUtc = vm.Content.PublishTimeUtc;
            DateTime topViewMinTimeUtc = DateTime.UtcNow - topViewsSelectPeriod.Value;
            Expression<Func<TContent, bool>> topViewsFilter = filterConditions
                .And(x => x.PublishTimeUtc >= topViewMinTimeUtc);


            //related queries
            Task<List<TContent>> nextTask = queries.Contains(ContentRelatedQuery.NextContent)
                ? SelectNextPage(publishDateUtc, 1, DataAmount.FullContent, filterConditions)
                : Task.FromResult(new List<TContent>());

            Task<List<TContent>> previousTask = queries.Contains(ContentRelatedQuery.PreviousContent)
                ? SelectPreviousPage(publishDateUtc, 1, DataAmount.FullContent, filterConditions)
                : Task.FromResult(new List<TContent>());

            Task<List<TContent>> sameCategoryLatestTask = queries.Contains(ContentRelatedQuery.SameCategoryLatestContents)
                ? _contentQueries.SelectMany(1, queryPageSize.Value + 1, DataAmount.ShortContent, true, filterConditions)
                : Task.FromResult<List<TContent>>(null);

            Task<List<TContent>> topViewsTask = queries.Contains(ContentRelatedQuery.SameCategoryTopViewContents)
                ? _contentQueries.SelectTopViews(queryPageSize.Value, DataAmount.ShortContent, topViewsFilter)
                : Task.FromResult(new List<TContent>());

            vm.NextContent = (await nextTask.ConfigureAwait(false)).FirstOrDefault();
            vm.PreviousContent = (await previousTask.ConfigureAwait(false)).FirstOrDefault();
            vm.CategoryTopViewsContent = await topViewsTask.ConfigureAwait(false);
            List<TContent> sameCategoryContent = await sameCategoryLatestTask.ConfigureAwait(false);

            //exclude content from list of same category content
            if (sameCategoryContent != null)
            {
                contentUrl = contentUrl.ToLowerInvariant();
                sameCategoryContent.RemoveAll(p => p.Url == contentUrl);
                vm.CategoryLatestContent = sameCategoryContent.Take(queryPageSize.Value).ToList();
            }

            return vm;
        }

        public virtual async Task<ContentEditVM<TKey, TCategory, TContent>> SelectToEdit(
            TKey contentId, long permission, TKey? userId)
        {
            Task<TContent> contentQuery = _contentQueries.SelectOne(false, DataAmount.FullContent,
                x => EqualityComparer<TKey>.Default.Equals(x.ContentId, contentId));
            Task<List<TCategory>> categoriesQuery = _categorySelector.SelectAllowed(permission, userId);

            TContent content = await contentQuery.ConfigureAwait(false);
            List<TCategory> allowedCategories = await categoriesQuery.ConfigureAwait(false);

            if (content == null)
            {
                return ContentEditVM<TKey, TCategory, TContent>.NotFound(allowedCategories, true);
            }

            IEnumerable<TKey> allowedCategoryIds = allowedCategories.Select(x => x.CategoryId);
            bool hasPermission = allowedCategoryIds.Contains(content.CategoryId);
            if (hasPermission == false)
            {
                return ContentEditVM<TKey, TCategory, TContent>.PermissionDenied(allowedCategories, true);
            }

            return ContentEditVM<TKey, TCategory, TContent>.Success(content, allowedCategories, true);
        }

        public virtual async Task<ContentPageVM<TKey, TCategory, TContent>> SelectPage(
            string categoryUrl, int page, int pageSize, DataAmount dataAmount, bool orderDescending
            , long permission, TKey? userId, bool countContent
            , Expression<Func<TContent, bool>> filterConditions)
        {
            //validation
            if (string.IsNullOrEmpty(categoryUrl))
            {
                return ContentPageVM<TKey, TCategory, TContent>.NotFound();
            }

            //permissions
            bool hasPermission = await _categorySelector.CheckPermission(categoryUrl, permission, userId)
                .ConfigureAwait(false);
            if (hasPermission == false)
            {
                return ContentPageVM<TKey, TCategory, TContent>.PermissionDenied();
            }

            categoryUrl = categoryUrl.ToLowerInvariant();
            List<TCategory> categories = await _categoryQueries
                .SelectMany(p => p.Url == categoryUrl)
                .ConfigureAwait(false);
            TCategory category = categories.FirstOrDefault();
            if (category == null)
            {
                return ContentPageVM<TKey, TCategory, TContent>.NotFound();
            }

            return await SelectPage(page, pageSize, dataAmount, orderDescending, permission, userId
                , countContent, filterConditions, new List<TKey>() { category.CategoryId })
                .ConfigureAwait(false);
        }

        public virtual async Task<ContentPageVM<TKey, TCategory, TContent>> SelectPage(
            int page, int pageSize, DataAmount dataAmount, bool orderDescending
            , long permission, TKey? userId, bool countContent
            , Expression<Func<TContent, bool>> filterConditions
            , IEnumerable<TKey> categoryIds = null)
        {
            page = page < 1 ? 1 : page;
            categoryIds = categoryIds ?? new List<TKey>();

            //filter categories
            filterConditions = await AddCategoryFilter(permission, userId, filterConditions, categoryIds)
                .ConfigureAwait(false);

            //queries
            Task<List<TContent>> contentsTask = _contentQueries.SelectMany(
                page, pageSize, dataAmount, orderDescending, filterConditions);

            Task<long> countTask = countContent
                ? _contentQueries.Count(filterConditions)
                : Task.FromResult<long>(0);

            Task<List<TCategory>> categoriesTask =
                _categorySelector.SelectAllowed(permission, userId);

            List<TContent> contentInCategory = await contentsTask.ConfigureAwait(false);
            long contentCountSelectedCategories = await countTask.ConfigureAwait(false);
            List<TCategory> allAllowedCategories = await categoriesTask.ConfigureAwait(false);

            //assemble
            return new ContentPageVM<TKey, TCategory, TContent>()
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = countContent ? contentCountSelectedCategories : 0,
                AllCategories = allAllowedCategories,
                SelectedCategories = allAllowedCategories.Where(p => categoryIds.Contains(p.CategoryId)).ToList(),
                Contents = contentInCategory,
                LastPublishTimeUtcIso8601 = contentInCategory.Count == 0
                    ? null
                    : contentInCategory.Last().PublishTimeUtc.ToIso8601(),
                ContentNumberMessage = string.Format(ContentsMessages.Content_Shown
                    , contentInCategory.Count, contentCountSelectedCategories)
            };
        }

        public virtual Task<List<TContent>> SelectPreviousPage(
          DateTime selectBeforeTimeUtc, int pageSize, DataAmount dataAmount
           , Expression<Func<TContent, bool>> filterConditions)
        {
            filterConditions = filterConditions.And(x => x.PublishTimeUtc < selectBeforeTimeUtc);
            return _contentQueries.SelectMany(1, pageSize, dataAmount, true, filterConditions);
        }

        public virtual Task<List<TContent>> SelectNextPage(
          DateTime selectAfterTimeUtc, int pageSize, DataAmount dataAmount
           , Expression<Func<TContent, bool>> filterConditions)
        {
            filterConditions = filterConditions.And(x => x.PublishTimeUtc > selectAfterTimeUtc);
            return _contentQueries.SelectMany(1, pageSize, dataAmount, false, filterConditions);
        }

        public virtual Task<ContinuationContentPageVM<TKey, TContent>> SelectNextPage(
            string lastPublishTimeUtcIso8601, int pageSize
            , long permission, TKey? userId
            , Expression<Func<TContent, bool>> filterConditions
            , IEnumerable<TKey> categoryIds = null)
        {
            //validate inputs
            DateTime? lastPublishTimeUtc;
            bool validDatetime = lastPublishTimeUtcIso8601.TryParseIso8601(out lastPublishTimeUtc);
            if (validDatetime == false)
            {
                string message = string.Format(ContentsMessages.Common_DateParseException, nameof(lastPublishTimeUtcIso8601));
                throw new FormatException(message);
            }

            return SelectNextPage(lastPublishTimeUtc.Value, pageSize
                , permission, userId, filterConditions, categoryIds);
        }

        public virtual async Task<ContinuationContentPageVM<TKey, TContent>> SelectNextPage(
            DateTime lastPublishTimeUtc, int pageSize
            , long permission, TKey? userId, Expression<Func<TContent, bool>> filterConditions
            , IEnumerable<TKey> categoryIds = null)
        {
            int itemsLimit = pageSize + 1;  //extra item to check if list is continueable

            //filter categories
            filterConditions = await AddCategoryFilter(permission, userId, filterConditions, categoryIds)
                .ConfigureAwait(false);

            //query
            List<TContent> contents = await
                SelectPreviousPage(lastPublishTimeUtc, itemsLimit
                , DataAmount.ShortContent, filterConditions)
                .ConfigureAwait(false);

            //assemble
            List<TContent> items = contents.Take(pageSize).ToList();
            return new ContinuationContentPageVM<TKey, TContent>()
            {
                CanContinue = contents.Count > pageSize,
                Contents = items,
                LastPublishTimeUtcIso8601 = items.Count == 0
                    ? null
                    : items.Last().PublishTimeUtc.ToIso8601()
            };
        }

        public virtual async Task<Expression<Func<TContent, bool>>> AddCategoryFilter(
            long permission, TKey? userId
            , Expression<Func<TContent, bool>> filterConditions
            , IEnumerable<TKey> categoryIds = null)
        {
            categoryIds = categoryIds ?? new List<TKey>();

            if (categoryIds.Count() > 0)
            {
                //Filter and include only alowed categories.
                List<TKey> allowedCategories = await _permissionSelector
                    .FilterAllowedCategories(categoryIds, permission, userId)
                    .ConfigureAwait(false);

                filterConditions.And(x => allowedCategories.Contains(x.CategoryId));
            }
            else
            {
                //Pick all categories except not allowed
                List<TKey> excludedCategoryIds = await _categorySelector
                    .SelectForbiddenIds(permission, userId)
                    .ConfigureAwait(false);

                filterConditions.And(x => excludedCategoryIds.Contains(x.CategoryId) == false);
            }
            
            return filterConditions;
        }

    }
}
