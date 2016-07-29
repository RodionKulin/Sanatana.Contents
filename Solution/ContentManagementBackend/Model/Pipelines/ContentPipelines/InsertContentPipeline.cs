using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ContentManagementBackend;
using System.Web;
using Common.Utility.Pipelines;

namespace ContentManagementBackend
{
    public class InsertContentPipeline<TKey> : ContentPipelineBase<TKey>
        where TKey : struct
    {
        //инициализация
        public InsertContentPipeline(IContentQueries<TKey> contentQueries
            , PreviewImageQueries previewImageQueries, ContentImageQueries contentImageQueries
            , ICategoryManager<TKey> categoryManager, ISearchQueries<TKey> searchQueries)
            : base(contentQueries, previewImageQueries, contentImageQueries, categoryManager, searchQueries)
        {
            RegisterModules();
        }



        //методы
        protected virtual void RegisterModules()
        {
            Register(SetPreviewImageUrl);
            Register(CheckPermission);
            Register(ValidateInput);
            Register(ValidateImage);
            Register(SanitizeTitle);
            Register(CreateUrl);
            Register(SanitizeFullContent);
            Register(FixIframeSrc);
            Register(ReplaceContentTempImagesSrc);
            Register(SanitizeShortContent);
            Register(IndexInSearchEngine);
            Register(InsertContent);
            Register(SavePreviewImage);
        }

        public override Task<ContentPipelineResult> Process(ContentPipelineModel<TKey> inputModel)
        {
            ContentPipelineResult output = new ContentPipelineResult()
            {
                Result = ContentUpdateResult.Success,
                Message = null
            };
            
            return base.Process(inputModel, output);
        }


        //этапы
        public virtual Task<bool> ValidateInput(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            if (string.IsNullOrEmpty(content.Title))
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Content_TitleEmpty);
                return Task.FromResult(false);
            }
            if (string.IsNullOrEmpty(content.FullContent))
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Content_FullContentEmpty);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public virtual async Task<bool> ValidateImage(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentSubmitVM contentVM = context.Input.ContentVM;
            ContentBase<TKey> content = context.Input.Content;
            string authorID = content.AuthorID.ToString();

            if (contentVM.ImageStatus == ImageStatus.Temp
                && contentVM.ImageID != null)
            {
                QueryResult<bool> imageExistsResult = 
                    await _previewImageQueries.TempExists(authorID, contentVM.ImageID.Value);

                if (imageExistsResult.HasExceptions)
                {
                    context.Output = ContentPipelineResult.Fail(MessageResources.Content_ImageException);
                    return false;
                }

                content.HasImage = imageExistsResult.Result;
            }
            else
            {
                content.HasImage = false;
            }

            if(!content.HasImage)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Content_PreviewImageNotSet);
                return false;
            }

            return true;
        }
        
        public virtual async Task<bool> SavePreviewImage(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentSubmitVM contentVM = context.Input.ContentVM;
            ContentBase<TKey> content = context.Input.Content;

            string authorID = content.AuthorID.ToString();
            content.HasImage = contentVM.ImageStatus != ImageStatus.NotSet;

            if (content.HasImage)
            {
                bool completed = await _previewImageQueries.MoveTempToStatic(authorID
                    , context.Input.ContentVM.ImageID.Value, content.Url);

                if (!completed)
                {
                    content.HasImage = false;
                    context.Output = ContentPipelineResult.Fail(MessageResources.Content_ImageException);
                    return false;
                }
            }

            contentVM.ImageStatus = content.HasImage ? ImageStatus.Static : ImageStatus.NotSet;
            context.Output.ImageUrl = _previewImageQueries.GetImageUrl(content, contentVM, true);
            return true;
        }
        
    }
}
