using Common.Utility.Pipelines;
using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ContentManagementBackend;

namespace ContentManagementBackend
{
    public class UpdateContentPipeline<TKey> : ContentPipelineBase<TKey>
        where TKey : struct
    {
        //поля
        protected ContentBase<TKey> _existingPost;



        //инициализация
        public UpdateContentPipeline(IContentQueries<TKey> contentQueries
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
            Register(SelectExistingPost);
            Register(ValidateImage);
            Register(SanitizeTitle);
            Register(CreateUrl);
            Register(SanitizeFullContent);
            Register(FixIframeSrc);
            Register(ReplaceContentTempImagesSrc);
            Register(SanitizeShortContent);
            Register(IndexInSearchEngine);
            Register(UpdatePost);
            Register(SavePreviewImage);
        }
      
        public override Task<ContentPipelineResult> Process(ContentPipelineModel<TKey> inputModel)
        {
            ContentPipelineResult output = new ContentPipelineResult()
            {
                Result = ContentUpdateResult.Success
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

        public virtual async Task<bool> SelectExistingPost(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;
            
            QueryResult<ContentBase<TKey>> existingPostResult =
                await _contentQueries.SelectShort(content.ContentID, false);

            if (existingPostResult.HasExceptions)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }
            if (existingPostResult.Result == null)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Content_NotFound, ContentUpdateResult.NotFound);
                return false;
            }

            _existingPost = existingPostResult.Result;
            return true;
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
            else if (contentVM.ImageStatus == ImageStatus.Static)
            {
                QueryResult<bool> imageExistsResult = await _previewImageQueries.StaticExists(_existingPost.Url);

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

            if (!content.HasImage)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Content_PreviewImageNotSet);
                return false;
            }

            return true;
        }

        public virtual async Task<bool> IndexInSearchEngine
            (PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentSubmitVM contentVM = context.Input.ContentVM;
            ContentBase<TKey> content = context.Input.Content;

            MessageResult<bool> isPublicCategoryResult =
               await _categoryManager.CheckIsPublic(content.CategoryID, context.Input.Permission);
            if (isPublicCategoryResult.HasExceptions)
            {
                context.Output = ContentPipelineResult.Fail(isPublicCategoryResult.Message);
                return false;
            }

            bool completed = true;
            bool doIndex = content.IsPublished && isPublicCategoryResult.Result
                && content.PublishTimeUtc <= DateTime.UtcNow;

            if (doIndex && !_existingPost.IsIndexed)
            {
                completed = await _searchQueries.Insert(content);
            }
            else if (!doIndex && _existingPost.IsIndexed)
            {
                completed = await _searchQueries.Delete(content.ContentID);
            }
            else if (doIndex && _existingPost.IsIndexed)
            {
                completed = await _searchQueries.Update(content);
            }

            if (!completed)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Common_SearchException);
                return false;
            }

            content.IsIndexed = doIndex;
            return true;
        }

        public virtual async Task<bool> UpdatePost(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentSubmitVM contentVM = context.Input.ContentVM;
            ContentBase<TKey> content = context.Input.Content;

            content.CreateUpdateNonce();

            ContentUpdateResult updateResult = await _contentQueries.Update(
                content, contentVM.UpdateNonce, contentVM.MatchUpdateNonce);

            if (updateResult != ContentUpdateResult.Success)
            {
                string message = updateResult == ContentUpdateResult.HasException
                        ? MessageResources.Common_DatabaseException
                    : updateResult == ContentUpdateResult.NotFound
                        ? MessageResources.Content_NotFound
                        : null;

                context.Output = ContentPipelineResult.Fail(message, updateResult);
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

            bool completed = true;

            if (contentVM.ImageStatus == ImageStatus.Temp)
            {
                if (_existingPost.HasImage)
                {
                    completed = await _previewImageQueries.DeleteStatic(_existingPost.Url);
                }

                completed &= await _previewImageQueries.MoveTempToStatic(authorID, contentVM.ImageID.Value, content.Url);
            }
            else if (contentVM.ImageStatus == ImageStatus.Static && content.Url != _existingPost.Url)
            {
                completed = await _previewImageQueries.RenameStatic(content.Url, _existingPost.Url);
            }
            else if (contentVM.ImageStatus == ImageStatus.NotSet && _existingPost.HasImage)
            {
                completed = await _previewImageQueries.DeleteStatic(_existingPost.Url);
            }

            if (!completed)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Content_ImageException);
                return false;
            }

            content.HasImage = contentVM.ImageStatus != ImageStatus.NotSet;
            contentVM.ImageStatus = content.HasImage ? ImageStatus.Static : ImageStatus.NotSet;
            context.Output.ImageUrl = _previewImageQueries.GetImageUrl(content, contentVM, true);
            return true;
        }


    }
}
