using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Patterns.Pipelines;
using System.Linq.Expressions;
using LinqKit;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Database;
using Sanatana.Contents.Selectors.Categories;
using Sanatana.Contents.Selectors.Permissions;

namespace Sanatana.Contents.Selectors.Comments
{
    public class CommentSelector<TKey, TCategory, TContent, TComment> 
        : ICommentSelector<TKey, TCategory, TContent, TComment>
        where TKey : struct
        where TCategory : Category<TKey>
        where TComment : Comment<TKey>
        where TContent : Content<TKey>
    {
        //fields
        protected ICommentQueries<TKey, TContent, TComment> _commentQueries;
        protected ICategorySelector<TKey, TCategory> _categorySelector;
        protected IPermissionSelector<TKey, TCategory> _permissionSelector;


        //init
        public CommentSelector(ICommentQueries<TKey, TContent, TComment> commentQueries
            , ICategorySelector<TKey, TCategory> categorySelector
            , IPermissionSelector<TKey, TCategory> permissionSelector)
        {
            _commentQueries = commentQueries;
            _categorySelector = categorySelector;
            _permissionSelector = permissionSelector;
        }


        //SelectAllForContent
        public virtual async Task<PipelineResult<List<ParentVM<TComment>>>> SelectAllForContent(
            int page, int pageSize, bool orderDescending, TKey contentId, TKey contentCategoryId, long permission, TKey? userId
            , CommentsGroupMethod groupMethod, Expression<Func<TComment, bool>> filterConditions)
        {
            bool hasPermission = await _permissionSelector
                .CheckIsAllowed(contentCategoryId, permission, userId)
                .ConfigureAwait(false);
            if (hasPermission == false)
            {
                return PipelineResult<List<ParentVM<TComment>>>.Error(ContentsMessages.Common_AuthorizationRequired);
            }

            filterConditions.And(x => EqualityComparer<TKey>.Default.Equals(x.ContentId, contentId));

            List<TComment> comments = await _commentQueries.SelectMany(
                page, pageSize, orderDescending, filterConditions)
                .ConfigureAwait(false);

            List<ParentVM<TComment>> commentGroups = GroupComments(comments, groupMethod);
            return PipelineResult<List<ParentVM<TComment>>>.Success(commentGroups);
        }

        public virtual List<ParentVM<TComment>> GroupComments(
            List<TComment> comments, CommentsGroupMethod groupMethod)
        {
            Dictionary<TKey?, List<TComment>> commentGroups;

            if (groupMethod == CommentsGroupMethod.BranchId)
            {
                commentGroups = comments.GroupBy(p => p.BranchCommentId)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }
            else if (groupMethod == CommentsGroupMethod.ParentId)
            {
                commentGroups = comments.GroupBy(p => p.ParentCommentId)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }
            else
            {
                return comments
                    .OrderByDescending(x => x.CreatedTimeUtc)
                    .Select(x => new ParentVM<TComment>(x))
                    .ToList();
            }

            if (commentGroups.ContainsKey(null) == false)
            {
                return new List<ParentVM<TComment>>();
            }

            List<TComment> rootGroup = commentGroups[null];
            return SetupCommentsHierarchy(rootGroup, commentGroups);
        }

        protected virtual List<ParentVM<TComment>> SetupCommentsHierarchy(
            List<TComment> comments, Dictionary<TKey?, List<TComment>> commentGroups)
        {
            List<ParentVM<TComment>> vmList = new List<ParentVM<TComment>>();

            foreach (TComment comment in comments)
            {
                ParentVM<TComment> commentVm = new ParentVM<TComment>
                {
                    Item = comment
                };
                vmList.Add(commentVm);

                if (commentGroups.ContainsKey(comment.CommentId) == false)
                {
                    continue;
                }

                List<TComment> childItems = commentGroups[comment.CommentId];
                commentVm.Children = SetupCommentsHierarchy(childItems, commentGroups);
                commentVm.HasChildren = true;
            }

            return vmList.OrderByDescending(x => x.Item.CreatedTimeUtc).ToList();
        }

        public virtual async Task<Expression<Func<TComment, bool>>> AddCategoryFilter(
            long permission, TKey? userId, Expression<Func<TComment, bool>> filterConditions
            , IEnumerable<TKey> categoryIds = null)
        {
            categoryIds = categoryIds ?? new List<TKey>();

            if (categoryIds.Count() > 0)
            {
                //Pick only allowed categories.
                List<TKey> allowedCategories = await _permissionSelector
                    .FilterAllowedCategories(categoryIds, permission, userId)
                    .ConfigureAwait(false);

                filterConditions.And(x => allowedCategories.Contains(x.Content.CategoryId));
            }
            else
            {
                //Pick all categories except not allowed.
                List<TKey> excludedCategoryIds = await _categorySelector
                    .SelectForbiddenIds(permission, userId)
                     .ConfigureAwait(false);

                filterConditions.And(x => excludedCategoryIds.Contains(x.Content.CategoryId) == false);
            }

            return filterConditions;
        }


        //SelectPageInCategory
        public virtual async Task<List<ParentVM<CommentJoinResult<TKey, TComment, TContent>>>> SelectPageInCategory(
            int page, int pageSize, bool orderDescending, DataAmount contentAmount, long permission
            , TKey? userId, CommentsGroupMethod groupMethod
            , Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>> filterConditions
            , IEnumerable<TKey> categoryIds = null)
        {
            filterConditions = await AddCategoryFilter(permission, userId, filterConditions, categoryIds);

            List<CommentJoinResult<TKey, TComment, TContent>> comments = await _commentQueries
               .SelectManyJoinedContent(page, pageSize, orderDescending, contentAmount, filterConditions)
               .ConfigureAwait(false);

            return GroupComments(comments, groupMethod);
        }

        public virtual List<ParentVM<CommentJoinResult<TKey, TComment, TContent>>> GroupComments(
            List<CommentJoinResult<TKey, TComment, TContent>> comments, CommentsGroupMethod groupMethod)
        {
            Dictionary<TKey?, List<CommentJoinResult<TKey, TComment, TContent>>> commentGroups;

            if (groupMethod == CommentsGroupMethod.BranchId)
            {
                commentGroups = comments.GroupBy(p => p.Comment.BranchCommentId)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }
            else if (groupMethod == CommentsGroupMethod.ParentId)
            {
                commentGroups = comments.GroupBy(p => p.Comment.ParentCommentId)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }
            else
            {
                return comments
                    .OrderByDescending(x => x.Comment.CreatedTimeUtc)
                    .Select(x => new ParentVM<CommentJoinResult<TKey, TComment, TContent>>(x))
                    .ToList();
            }

            if (commentGroups.ContainsKey(null) == false)
            {
                return new List<ParentVM<CommentJoinResult<TKey, TComment, TContent>>>();
            }

            List<CommentJoinResult<TKey, TComment, TContent>> rootGroup = commentGroups[null];
            return SetupCommentsHierarchy(rootGroup, commentGroups);
        }

        protected virtual List<ParentVM<CommentJoinResult<TKey, TComment, TContent>>> SetupCommentsHierarchy(
            List<CommentJoinResult<TKey, TComment, TContent>> comments, Dictionary<TKey?, List<CommentJoinResult<TKey, TComment, TContent>>> commentGroups)
        {
            List<ParentVM<CommentJoinResult<TKey, TComment, TContent>>> vmList = new List<ParentVM<CommentJoinResult<TKey, TComment, TContent>>>();

            foreach (CommentJoinResult<TKey, TComment, TContent> comment in comments)
            {
                ParentVM<CommentJoinResult<TKey, TComment, TContent>> commentVm = new ParentVM<CommentJoinResult<TKey, TComment, TContent>>
                {
                    Item = comment
                };
                vmList.Add(commentVm);

                if (commentGroups.ContainsKey(comment.Comment.CommentId) == false)
                {
                    continue;
                }

                List<CommentJoinResult<TKey, TComment, TContent>> childItems = commentGroups[comment.Comment.CommentId];
                commentVm.Children = SetupCommentsHierarchy(childItems, commentGroups);
                commentVm.HasChildren = true;
            }

            return vmList.OrderByDescending(x => x.Item.Comment.CreatedTimeUtc).ToList();
        }

        public virtual async Task<Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>>> AddCategoryFilter(
            long permission, TKey? userId, Expression<Func<CommentJoinResult<TKey, TComment, TContent>, bool>> filterConditions
            , IEnumerable<TKey> categoryIds = null)
        {
            categoryIds = categoryIds ?? new List<TKey>();

            if (categoryIds.Count() > 0)
            {
                //Pick only allowed categories.
                List<TKey> allowedCategories = await _permissionSelector
                    .FilterAllowedCategories(categoryIds, permission, userId)
                    .ConfigureAwait(false);

                filterConditions.And(x => allowedCategories.Contains(x.Content.CategoryId));
            }
            else
            {
                //Pick all categories except not allowed.
                List<TKey> excludedCategoryIds = await _categorySelector
                    .SelectForbiddenIds(permission, userId)
                     .ConfigureAwait(false);

                filterConditions.And(x => excludedCategoryIds.Contains(x.Content.CategoryId) == false);
            }

            return filterConditions;
        }
    }
}
