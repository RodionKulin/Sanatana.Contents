using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Patterns.Pipelines;

namespace Sanatana.Contents.Selectors.Comments
{
    public interface ICommentSelector<TKey, TCategory, TContent, TComment>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        Task<Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>>> AddCategoryFilter(long permission, TKey? userId, Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>> filterConditions, IEnumerable<TKey> categoryIds = null);
        Task<Expression<Func<TComment, bool>>> AddCategoryFilter(long permission, TKey? userId, Expression<Func<TComment, bool>> filterConditions, IEnumerable<TKey> categoryIds = null);
        List<ParentVM<CommentJoinResult<TKey, TComment, TContent>>> GroupComments(List<CommentJoinResult<TKey, TComment, TContent>> comments, CommentsGroupMethod groupMethod);
        List<ParentVM<TComment>> GroupComments(List<TComment> comments, CommentsGroupMethod groupMethod);
        Task<PipelineResult<List<ParentVM<TComment>>>> SelectAllForContent(int page, int pageSize, bool orderDescending, TKey contentId, TKey contentCategoryId, long permission, TKey? userId, CommentsGroupMethod groupMethod, Expression<Func<TComment, bool>> filterConditions);
        Task<List<ParentVM<CommentJoinResult<TKey, TComment, TContent>>>> SelectPageInCategory(int page, int pageSize, bool orderDescending, DataAmount contentAmount, long permission, TKey? userId, CommentsGroupMethod groupMethod, Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>> filterConditions, IEnumerable<TKey> categoryIds = null);
    }
}