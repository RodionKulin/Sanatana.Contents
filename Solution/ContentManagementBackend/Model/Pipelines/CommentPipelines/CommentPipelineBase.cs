using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utility.Pipelines;

namespace ContentManagementBackend
{
    public abstract class CommentPipelineBase<TKey> : Pipeline<CommentPipelineModel<TKey>>
        where TKey : struct
    {
        //поля
        protected IContentQueries<TKey> _contentQueries;
        protected ICommentQueries<TKey> _commentQueries;
        protected ICategoryManager<TKey> _categoryManager;
        protected ICaptchaProvider _captchaProvider;
        protected ICommentStateProvider _commentStateProvider;
        protected ContentBase<TKey> _content;


        //инициализация
        public CommentPipelineBase(IContentQueries<TKey> contentQueries
            , ICommentQueries<TKey> commentQueries, ICategoryManager<TKey> categoryManager
            , ICaptchaProvider captchaProvider, ICommentStateProvider commentStateProvider)
        {
            _contentQueries = contentQueries;
            _commentQueries = commentQueries;
            _categoryManager = categoryManager;
            _captchaProvider = captchaProvider;
            _commentStateProvider = commentStateProvider;
        }



        //методы
        public virtual async Task<bool> CheckPermission(
            PipelineContext<CommentPipelineModel<TKey>, PipelineResult> context)
        {
            Comment<TKey> comment = context.Input.Comment;

            if (_content == null)
            {
                QueryResult<ContentBase<TKey>> contentResult =
                    await _contentQueries.SelectShort(comment.ContentID, false);
                if (contentResult.HasExceptions)
                {
                    context.Output = PipelineResult.Fail(MessageResources.Common_DatabaseException);
                    return false;
                }

                _content = contentResult.Result;
            }

            MessageResult<Category<TKey>> permissionResult =
                await _categoryManager.CheckPermission(_content.CategoryID,
                context.Input.Permission, context.Input.UserRoles);
            if (permissionResult.HasExceptions)
            {
                context.Output = PipelineResult.Fail(permissionResult.Message);
                return false;
            }

            return true;
        }

        public virtual Task<bool> ValidateComment(
            PipelineContext<CommentPipelineModel<TKey>, PipelineResult> context)
        {
            if (string.IsNullOrEmpty(context.Input.Comment.Content))
            {
                context.Output = PipelineResult.Fail(MessageResources.Comment_ContentEmpty);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public virtual Task<bool> ValidateCaptcha(
            PipelineContext<CommentPipelineModel<TKey>, PipelineResult> context)
        {
            bool isValid = _captchaProvider.Validate(context.Input.Challenge, context.Input.Response);

            if (!isValid)
            {
                context.Output = PipelineResult.Fail(MessageResources.Comment_InvalidCaptcha);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public virtual Task<bool> Sanitize(
            PipelineContext<CommentPipelineModel<TKey>, PipelineResult> context)
        {
            Comment<TKey> comment = context.Input.Comment;

            HtmlSanitizer sanitizer = new HtmlSanitizer()
            {
                AllowedIFrameUrls = context.Input.AllowedIFrameUrls
            };

            comment.Content = sanitizer.Sanitize(comment.Content);
            comment.Content = HtmlTagRemover.RemoveEmptyTags(comment.Content);

            return Task.FromResult(true);
        }
    }
}
