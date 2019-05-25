using Sanatana.Contents.Database;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Resources;
using Sanatana.Contents.Search;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Comments
{
    public class DeleteCommentPipeline<TKey, TCategory, TContent, TComment>
        : CommentPipelineBase<TKey, TCategory, TContent, TComment>, 
        IDeleteCommentPipeline<TKey, TCategory, TContent, TComment> 
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        //init
        public DeleteCommentPipeline(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
            , ICommentQueries<TKey, TContent, TComment> commentQueries, IPermissionSelector<TKey, TCategory> permissionSelector
            , ISearchQueries<TKey> searchQueries)
            : base(exceptionHandler, contentQueries, commentQueries, permissionSelector, searchQueries)
        {
            RegisterModules();
        }


        //bootstrap
        protected virtual void RegisterModules()
        {
            Register(CheckPermission);
            Register(IncrementVersion);
            Register(GetStoredComment);
            Register(DeleteCommentDb, RollBackDeleteCommentDb);
            Register(UpdateTotalCountDb, RollBackUpdateTotalCountDb);
            Register(DeleteCommentSearch, RollBackDeleteCommentSearch);
        }
        

        //modules 
        public virtual async Task<bool> DeleteCommentDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;
            await _commentQueries.DeleteMany(new[] { comment }).ConfigureAwait(false);
            
            return true;
        }

        public virtual async Task<bool> RollBackDeleteCommentDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            await _commentQueries.InsertMany(new[] { _storedComment }).ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> UpdateTotalCountDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;

            await _contentQueries.IncrementCommentsCount(-1,
                x => EqualityComparer<TKey>.Default.Equals(x.ContentId, context.Input.Comment.ContentId))
                .ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> RollBackUpdateTotalCountDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;

            await _contentQueries.IncrementCommentsCount(1,
                x => EqualityComparer<TKey>.Default.Equals(x.ContentId, context.Input.Comment.ContentId))
                .ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> DeleteCommentSearch(
           PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;
            if (comment.IsIndexed == false)
            {
                return true;
            }

            await _searchQueries.Delete(new object[] { comment }.ToList()).ConfigureAwait(false);
            comment.IsIndexed = false;

            return true;
        }

        public virtual async Task<bool> RollBackDeleteCommentSearch(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = _storedComment;
            if (comment.NeverIndex == true)
            {
                return true;
            }

            await _searchQueries.Insert(new object[] { comment }.ToList()).ConfigureAwait(false);
            comment.IsIndexed = true;

            return true;
        }
        
    }
}
