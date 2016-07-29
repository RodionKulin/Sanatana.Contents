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
    public class InsertCommentPipeline<TKey> : CommentPipelineBase<TKey>
        where TKey : struct
    {
        //инициализация
        public InsertCommentPipeline(IContentQueries<TKey> contentQueries
            , ICommentQueries<TKey> commentQueries, ICategoryManager<TKey> categoryManager
            , ICaptchaProvider captchaProvider, ICommentStateProvider commentStateProvider)
            : base(contentQueries, commentQueries, categoryManager, captchaProvider, commentStateProvider)
        {
            RegisterModules();
        }



        //методы
        protected virtual void RegisterModules()
        {
            Register(CheckPermission);
            Register(ValidateComment);
            Register(ValidateCaptcha);
            Register(Sanitize);
            Register(SetState);
            Register(SaveComment);
            Register(UpdateTotalCount);
        }

        public override async Task<PipelineResult> Process(CommentPipelineModel<TKey> inputModel)
        {
            PipelineResult outputModel = new PipelineResult();
            return await Process(inputModel, outputModel);
        }



        //этапы        
        public virtual async Task<bool> SetState(
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
            
            MessageResult<bool> isPublicResult = await _categoryManager.CheckIsPublic(
                _content.CategoryID, context.Input.ViewPermission);
            if (isPublicResult.HasExceptions)
            {
                context.Output = PipelineResult.Fail(isPublicResult.Message);
                return false;
            }

            comment.State = _commentStateProvider.Inserted(isPublicResult.Result);
            return true;
        }

        public virtual async Task<bool> SaveComment(
            PipelineContext<CommentPipelineModel<TKey>, PipelineResult> context)
        {
            Task<bool> insertTask = _commentQueries.Insert(context.Input.Comment);
            bool added = await insertTask;
            
            if (!added)
            {
                context.Output = PipelineResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }
            
            return true;
        }

        public virtual async Task<bool> UpdateTotalCount(
            PipelineContext<CommentPipelineModel<TKey>, PipelineResult> context)
        {
            bool updated = await _contentQueries.IncrementCommentCount(
                context.Input.Comment.ContentID, 1);

            return true;
        }
    }
}
