using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Utility;
using Common.Utility.Pipelines;

namespace ContentManagementBackend
{
    public interface IContentManager<TKey> where TKey : struct
    {
        AvatarImageQueries AvatarImageQueries { get; set; }
        ICacheProvider CacheProvider { get; set; }
        ICategoryManager<TKey> CategoryManager { get; set; }
        ICategoryQueries<TKey> CategoryQueries { get; set; }
        CommentImageQueries CommentImageQueries { get; set; }
        ICommentManager<TKey> CommentManager { get; set; }
        ICommentQueries<TKey> CommentQueries { get; set; }
        ContentImageQueries ContentImageQueries { get; set; }
        IContentQueries<TKey> ContentQueries { get; set; }
        DeleteContentPipeline<TKey> DeleteContentPipeline { get; set; }
        InsertContentPipeline<TKey> InsertContentPipeline { get; set; }
        PreviewImageQueries PreviewImageQueries { get; set; }
        ISearchQueries<TKey> SearchQueries { get; set; }
        UpdateContentPipeline<TKey> UpdateContentPipeline { get; set; }

        Task<PipelineResult> Delete(TKey contentID, int permission, List<string> userRoles);
        Task<ContentEditVM<TKey>> Insert(ContentSubmitVM contentVM, ContentBase<TKey> content, int permission, List<string> userRoles);
        Task<SearchResultVM<TKey>> Search(SearchInputVM<TKey> vm, int pageSize, int permission, List<string> userRoles);
        Task<SelectContentVM<TKey>> SelectPage(int page, int pageSize, bool onlyPublished, string categoryUrl, int permission, List<string> userRoles, bool countPosts);
        Task<SelectContentVM<TKey>> SelectPage(int page, int pageSize, bool onlyPublished, List<TKey> categoryIDs, int permission, List<string> userRoles, bool countPosts);
        Task<QueryResult<List<ContentRenderVM<TKey>>>> SelectPopular(TimeSpan period, int count, int permission, bool useAllPostsToMatchCount);
        Task<ContentEditVM<TKey>> SelectToEdit(TKey contentID, int permission, List<string> userRoles);
        Task<ContentFullVM<TKey>> SelectToRead(string url, int permission, List<string> userRoles);
        Task<ContentFullVM<TKey>> SelectToRead(string url, int permission, List<string> userRoles, int categoryPostsCount, List<ContentFullQuery> queries);
        Task<SelectNextContentVM<TKey>> SelectСontinuation(string lastID, int pageSize, bool onlyPublished, List<TKey> categoryIDs, int permission, List<string> userRoles);
        Task<ContentEditVM<TKey>> Update(ContentSubmitVM contentVM, ContentBase<TKey> content, int permission, List<string> userRoles);
    }
}