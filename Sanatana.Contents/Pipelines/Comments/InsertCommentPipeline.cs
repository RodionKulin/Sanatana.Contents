using Sanatana.Contents.Database;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Pipelines.Images;
using Sanatana.Contents.Resources;
using Sanatana.Contents.Search;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Comments
{
    public class InsertCommentPipeline<TKey, TCategory, TContent, TComment> : CommentPipelineBase<TKey, TCategory, TContent, TComment>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        //init
        public InsertCommentPipeline(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
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
            Register(ValidateComment);
            Register(Sanitize);
            Register(InsertCommentDb, RollBackInsertCommentDb);
            Register(UpdateTotalCountDb, RollBackUpdateTotalCountDb);
            Register(UpdateCommentSearch, RollBackCommentInsertSearch);
        }
        

        //modules 
        public virtual async Task<bool> InsertCommentDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            await _commentQueries.InsertMany(new[] { context.Input.Comment }).ConfigureAwait(false);
            
            return true;
        }

        public virtual async Task<bool> RollBackInsertCommentDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            await _commentQueries.DeleteMany(new[] { context.Input.Comment }).ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> UpdateTotalCountDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            await _contentQueries.IncrementCommentsCount(1
                , x => EqualityComparer<TKey>.Default.Equals(x.ContentId, context.Input.Comment.ContentId))
                .ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> RollBackUpdateTotalCountDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            await _contentQueries.IncrementCommentsCount(-1
                , x => EqualityComparer<TKey>.Default.Equals(x.ContentId, context.Input.Comment.ContentId))
                .ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> RollBackCommentInsertSearch(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment updatedComment = context.Input.Comment;
            updatedComment.Version++;

            if (updatedComment.IsIndexed == true)
            {
                await _searchQueries.Delete(new object[] { _storedComment }.ToList()).ConfigureAwait(false);
                updatedComment.IsIndexed = false;
            }

            return true;
        }
    }
}
