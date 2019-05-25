using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Sanatana.Patterns.Pipelines;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Database;
using Sanatana.Contents.Search;
using Sanatana.Contents.Selectors.Permissions;

namespace Sanatana.Contents.Pipelines.Comments
{
    public class UpdateCommentPipeline<TKey, TCategory, TContent, TComment>
        : CommentPipelineBase<TKey, TCategory, TContent, TComment>, 
        IUpdateCommentPipeline<TKey, TCategory, TContent, TComment>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {

        //init
        public UpdateCommentPipeline(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
            , ICommentQueries<TKey, TContent, TComment> commentQueries, IPermissionSelector<TKey, TCategory> permissionSelector
            , ISearchQueries<TKey> searchQueries)
            : base(exceptionHandler, contentQueries, commentQueries
                  , permissionSelector, searchQueries)
        {
            RegisterModules();
        }



        //bootstrap
        protected virtual void RegisterModules()
        {
            Register(CheckPermission);
            Register(ValidateComment);
            Register(Sanitize);
            Register(GetStoredComment);
            Register(IncrementVersion);
            Register(UpdateCommentDb, RollBackUpdateCommentDb);
            Register(UpdateCommentSearch, RollBackUpdateCommentSearch);
        }



        //modules
        public virtual async Task<bool> UpdateCommentDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;
            
            var updateProps = new Expression<Func<TComment , object>>[]
            {
                p => p.Text,
                p => p.UpdatedTimeUtc,
                p => p.State
            };

            await _commentQueries.UpdateMany(new[] { comment }, updateProps).ConfigureAwait(false);
            
            return true;
        }

        public virtual async Task<bool> RollBackUpdateCommentDb(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = _storedComment;

            var updateProps = new Expression<Func<TComment , object>>[]
            {
                p => p.Text,
                p => p.UpdatedTimeUtc,
                p => p.State
            };

            await _commentQueries.UpdateMany(new[] { comment }, updateProps).ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> RollBackUpdateCommentSearch(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment updatedComment = context.Input.Comment;
            _storedComment.Version = updatedComment.Version + 1;

            if (_storedComment.NeverIndex == false)
            {
                await _searchQueries.Insert(new object[] { _storedComment }.ToList()).ConfigureAwait(false);
                updatedComment.IsIndexed = true;
            }
            else if (_storedComment.NeverIndex == true && updatedComment.IsIndexed == true)
            {
                await _searchQueries.Delete(new object[] { _storedComment }.ToList()).ConfigureAwait(false);
                updatedComment.IsIndexed = false;
            }
            
            return true;
        }
    }
}
