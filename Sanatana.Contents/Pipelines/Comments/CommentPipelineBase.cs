using Sanatana.Contents.Database;
using Sanatana.Contents.Html;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Pipelines.Comments;
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
    public abstract class CommentPipelineBase<TKey, TCategory, TContent, TComment> 
        : Pipeline<CommentEditParams<TKey, TComment>>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
        where TComment : Comment<TKey>
    {
        //fields
        protected IPipelineExceptionHandler _exceptionHandler;
        protected IContentQueries<TKey, TContent> _contentQueries;
        protected ICommentQueries<TKey, TContent, TComment> _commentQueries;
        protected IPermissionSelector<TKey, TCategory> _permissionSelector;
        protected TContent _content;
        protected TComment _storedComment;
        protected ISearchQueries<TKey> _searchQueries;


        //init
        public CommentPipelineBase(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
            , ICommentQueries<TKey, TContent, TComment> commentQueries, IPermissionSelector<TKey, TCategory> permissionSelector
            , ISearchQueries<TKey> searchQueries)
        {
            _exceptionHandler = exceptionHandler;
            _contentQueries = contentQueries;
            _commentQueries = commentQueries;
            _permissionSelector = permissionSelector;
            _searchQueries = searchQueries;
        }


        //execute
        public override Task<PipelineResult> Execute(
            CommentEditParams<TKey, TComment> inputModel, PipelineResult outputModel)
        {
            outputModel = outputModel ?? PipelineResult.Success();
            return base.Execute(inputModel, outputModel);
        }

        public override async Task RollBack(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            await base.RollBack(context);

            if (context.Exceptions != null)
            {
                _exceptionHandler.Handle(context);
            }
            if (context.Output == null)
            {
                context.Output = PipelineResult.Error(ContentsMessages.Common_ProcessingError);
            }

            return;
        }


        //methods
        public virtual async Task<bool> CheckPermission(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;

            if (_content == null)
            {
                _content = await _contentQueries.SelectOne(
                    false, DataAmount.DescriptionOnly,
                    x => EqualityComparer<TKey>.Default.Equals(x.ContentId, comment.ContentId))
                    .ConfigureAwait(false);
            }

            bool hasPermission = await _permissionSelector.CheckIsAllowed(_content.CategoryId,
                context.Input.Permission, context.Input.UserId)
                .ConfigureAwait(false);
            if (hasPermission == false)
            {
                context.Output = PipelineResult.Error(ContentsMessages.Common_AuthorizationRequired);
                return false;
            }

            return true;
        }

        public virtual Task<bool> ValidateComment(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            if (string.IsNullOrEmpty(context.Input.Comment.Text))
            {
                context.Output = PipelineResult.Error(ContentsMessages.Comment_ContentEmpty);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
        
        public virtual Task<bool> Sanitize(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;

            HtmlSanitizer sanitizer = new HtmlSanitizer()
            {
                AllowedIFrameUrls = context.Input.AllowedIFrameUrls
            };

            comment.Text = sanitizer.Sanitize(comment.Text);
            comment.Text = HtmlTagRemover.RemoveEmptyTags(comment.Text);

            return Task.FromResult(true);
        }

        public virtual Task<bool> IncrementVersion(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;
            comment.Version++;
            return Task.FromResult(true);
        }

        public virtual async Task<bool> GetStoredComment(
            PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;
            _storedComment = await _commentQueries
                .SelectOne(x => EqualityComparer<TKey>.Default.Equals(x.CommentId, comment.CommentId))
                .ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> UpdateCommentSearch(
           PipelineContext<CommentEditParams<TKey, TComment>, PipelineResult> context)
        {
            TComment comment = context.Input.Comment;
            
            if (comment.NeverIndex == false)
            {
                await _searchQueries.Insert(new object[] { comment }.ToList()).ConfigureAwait(false);
                comment.IsIndexed = true;
            }
            else if (comment.NeverIndex == true && comment.IsIndexed == true)
            {
                await _searchQueries.Delete(new object[] { comment }.ToList()).ConfigureAwait(false);
                comment.IsIndexed = false;
            }
            
            return true;
        }

    }
}
