using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public interface IContentQueries<TKey>
        where TKey : struct
    {
        Task<QueryResult<bool>> Insert(ContentBase<TKey> post);

        Task<QueryResult<ContentBase<TKey>>> SelectFull(TKey contentID, bool incrementView, bool onlyPublished);

        Task<QueryResult<ContentBase<TKey>>> SelectFull(string url, bool incrementView, bool onlyPublished);

        Task<QueryResult<ContentBase<TKey>>> SelectShort(TKey contentID, bool includeShortContent);

        Task<QueryResult<List<ContentBase<TKey>>>> SelectShortPopular(
            TimeSpan period, int count, List<TKey> excludeCategoryIDs, bool useAllPostsToMatchCount);

        Task<QueryResult<ContentBase<TKey>>> SelectShortPrevious(
            ContentBase<TKey> post, bool onlyPublished, bool includeShortContent
            , List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null);

        Task<QueryResult<ContentBase<TKey>>> SelectShortNext(
            ContentBase<TKey> post, bool onlyPublished, bool includeShortContent
            , List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null);

        Task<QueryResult<List<ContentBase<TKey>>>> SelectShortList(
            List<TKey> contentIDs, bool onlyPublished, bool includeShortContent);

        Task<QueryResult<List<ContentBase<TKey>>>> SelectShortList(
            int page, int pageSize, bool onlyPublished, bool includeShortContent
            , List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null);

        Task<QueryResult<List<ContentBase<TKey>>>> SelectShortListСontinuation(
            DateTime? lastPublishTimeUtc, int pageSize, bool onlyPublished, bool includeShortContent
            , List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null);

        Task<QueryResult<int>> Count(bool onlyPublished, List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null);

        Task<ContentUpdateResult> Update(ContentBase<TKey> post, string updateNonce, bool matchNonce);

        Task<bool> IncrementCommentCount(TKey contentID, int increment);

        Task<bool> Delete(TKey contentID);

        Task<bool> SetIndexed(List<TKey> contentIDs, bool value);

        Task<QueryResult<List<ContentBase<TKey>>>> SelectFullNotIndexed(
            DateTime fromDateUtc, List<TKey> excludeCategoryIDs, int count);

        Task<QueryResult<List<ContentCategoryGroup<TKey>>>> SelectLatestFromEachCategory(
            int count, List<TKey> categoryIDs = null, List<TKey> excludeCategoryIDs = null);
    }
}
