using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sanatana.Contents.Selectors.Contents
{
    public interface IContentSelector<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        Task<Expression<Func<TContent, bool>>> AddCategoryFilter(long permission, TKey? userId, Expression<Func<TContent, bool>> filterConditions, IEnumerable<TKey> categoryIds = null);
        Task<ContentPageVM<TKey, TCategory, TContent>> SelectPage(int page, int pageSize, DataAmount dataAmount, bool orderDescending, long permission, TKey? userId, bool countContent, Expression<Func<TContent, bool>> filterConditions, IEnumerable<TKey> categoryIds = null);
        Task<ContentPageVM<TKey, TCategory, TContent>> SelectPage(string categoryUrl, int page, int pageSize, DataAmount dataAmount, bool orderDescending, long permission, TKey? userId, bool countContent, Expression<Func<TContent, bool>> filterConditions);
        Task<ContentEditVM<TKey, TCategory, TContent>> SelectToEdit(TKey contentId, long permission, TKey? userId);
        Task<ContentRelatedVM<TKey, TCategory, TContent>> SelectToRead(string contentUrl, long permission, TKey? userId, bool incrementViewCount, List<ContentRelatedQuery> queries, Expression<Func<TContent, bool>> filterConditions, int? queryPageSize = null, TimeSpan? topViewsSelectPeriod = null);
        Task<ContentRelatedVM<TKey, TCategory, TContent>> SelectToRead(string contentUrl, long permission, TKey? userId, bool incrementViewCount, Expression<Func<TContent, bool>> filterConditions);
        Task<List<TContent>> SelectPreviousPage(DateTime selectBeforeTimeUtc, int pageSize, DataAmount dataAmount, Expression<Func<TContent, bool>> filterConditions);
        Task<List<TContent>> SelectNextPage(DateTime selectAfterTimeUtc, int pageSize, DataAmount dataAmount, Expression<Func<TContent, bool>> filterConditions);
        Task<ContinuationContentPageVM<TKey, TContent>> SelectNextPage(DateTime lastPublishTimeUtc, int pageSize, long permission, TKey? userId, Expression<Func<TContent, bool>> filterConditions, IEnumerable<TKey> categoryIds = null);
        Task<ContinuationContentPageVM<TKey, TContent>> SelectNextPage(string lastPublishTimeUtcIso8601, int pageSize, long permission, TKey? userId, Expression<Func<TContent, bool>> filterConditions, IEnumerable<TKey> categoryIds = null);
    }
}