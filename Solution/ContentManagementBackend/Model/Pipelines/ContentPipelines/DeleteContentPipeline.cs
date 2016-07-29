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
    public class DeleteContentPipeline<TKey> : Pipeline<ContentDeletePipelineModel<TKey>, PipelineResult>
        where TKey : struct
    {
        //поля
        protected PreviewImageQueries _previewQueries;
        protected ContentImageQueries _contentImageQueries;
        protected IContentQueries<TKey> _contentQueries;
        protected ICommentQueries<TKey> _commentQueries;
        protected ISearchQueries<TKey> _searchQueries;
        protected ICategoryManager<TKey> _categoryManager;
        protected ContentBase<TKey> _content;



        //инициализация
        public DeleteContentPipeline(IContentQueries<TKey> contentQueries, ICommentQueries<TKey> commentQueries
            , PreviewImageQueries previewImageQueries, ContentImageQueries contentImageQueries
            , ICategoryManager<TKey> categoryManager, ISearchQueries<TKey> searchQueries)
        {
            _contentQueries = contentQueries;
            _commentQueries = commentQueries;
            _searchQueries = searchQueries;
            _previewQueries = previewImageQueries;
            _contentImageQueries = contentImageQueries;
            _categoryManager = categoryManager;

            RegisterModules();
        }



        //методы
        protected virtual void RegisterModules()
        {
            Register(SelectExisting);
            Register(CheckPermission);
            Register(DeleteContent);
            Register(DeleteContentImages);
            Register(DeleteFromSearchIndex);
        }

        public override async Task<PipelineResult> Process(ContentDeletePipelineModel<TKey> inputModel)
        {
            PipelineResult outputModel = new PipelineResult();
            return await Process(inputModel, outputModel);
        }



        //этапы
        public virtual async Task<bool> SelectExisting(
            PipelineContext<ContentDeletePipelineModel<TKey>, PipelineResult> context)
        {
            TKey contentID = context.Input.ContentID;
            QueryResult<ContentBase<TKey>> contentResult = await _contentQueries.SelectShort(contentID, false);

            if (contentResult.HasExceptions)
            {
                context.Output = PipelineResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }
            else if (contentResult.Result == null)
            {
                context.Output = PipelineResult.Fail(MessageResources.Content_NotFound);
                return false;
            }

            _content = contentResult.Result;
            return true;
        }

        public virtual async Task<bool> CheckPermission(
            PipelineContext<ContentDeletePipelineModel<TKey>, PipelineResult> context)
        {            
            MessageResult<Category<TKey>> permissinoResult = 
                await _categoryManager.CheckPermission(_content.CategoryID, context.Input.Permission, context.Input.UserRoles);

            if(permissinoResult.HasExceptions)
            {
                context.Output = PipelineResult.Fail(permissinoResult.Message);
                return false;
            }
            
            return true;
        }

        public virtual async Task<bool> DeleteContent(
            PipelineContext<ContentDeletePipelineModel<TKey>, PipelineResult> context)
        {
            TKey contentID = context.Input.ContentID;

            Task<bool> contentTask = _contentQueries.Delete(contentID);
            Task<bool> commentsTask = _commentQueries.Delete(contentID);

            bool contentDeleted = await contentTask;
            bool commentsDeleted = await commentsTask;

            if (!contentDeleted || !commentsDeleted)
            {
                context.Output = PipelineResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }

            return true;
        }

        public virtual async Task<bool> DeleteContentImages(
            PipelineContext<ContentDeletePipelineModel<TKey>, PipelineResult> context)
        {
            string contentID = context.Input.ContentID.ToString();

            Task<bool> previewDeleteTask = _previewQueries.DeleteStatic(_content.Url);
            Task<bool> contentDeleteTask = _contentImageQueries.DeleteStaticDirectory(contentID);

            bool previewCompleted = await previewDeleteTask;
            bool contentCompleted = await contentDeleteTask;

            if (!previewCompleted || !contentCompleted)
            {
                context.Output = PipelineResult.Fail(MessageResources.Content_ImageException);
                return false;
            }

            return true;
        }
        
        public virtual async Task<bool> DeleteFromSearchIndex(
            PipelineContext<ContentDeletePipelineModel<TKey>, PipelineResult> context)
        {
            TKey contentID = context.Input.ContentID;            
            if(!_content.IsIndexed)
            {
                return true;
            }

            bool completed = await _searchQueries.Delete(contentID);
            if (!completed)
            {
                context.Output = PipelineResult.Fail(MessageResources.Common_SearchException);
                return false;
            }

            return true;
        }
    }
}
