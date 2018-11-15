using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sanatana.Contents;
using Sanatana.Patterns.Pipelines;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Html.Media;
using Sanatana.Contents.Search;
using Sanatana.Contents.Database;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Utilities;
using Sanatana.Contents.Files.Services;

namespace Sanatana.Contents.Pipelines.Contents
{
    public class UpdateContentPipeline<TKey, TCategory, TContent>
        : ContentPipelineBase<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        //fields
        protected TContent _existingContent;

        
        //init
        public UpdateContentPipeline(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
            , IPermissionSelector<TKey, TCategory> permissionSelector, ISearchQueries<TKey> searchQueries
            , IImageFileService imageFileService, IHtmlMediaExtractor htmlMediaExtractor, IUrlEncoder urlEncoder)
            : base(exceptionHandler, contentQueries, permissionSelector
                  , searchQueries, imageFileService, htmlMediaExtractor, urlEncoder)
        {
            RegisterModules();
        }


        //bootstrap
        protected virtual void RegisterModules()
        {
            Register(CheckPermission);
            Register(ValidateInput);
            Register(SelectExistingContent);
            Register(SanitizeTitle);
            Register(CreateUrl);
            Register(SanitizeFullContent);
            Register(FixIframeSrc);
            Register(ReplaceContentTempImagesSrc);
            Register(SanitizeShortContent);
            Register(IncrementVersion);
            Register(UpdateContentSearch);
            Register(UpdateContentDb);
        }
      


        //modules
        public virtual Task<bool> ValidateInput(
            PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            TContent content = context.Input.Content;

            if (string.IsNullOrEmpty(content.Title))
            {
                context.Output = ContentEditResult.Error(ContentsMessages.Content_TitleEmpty);
                return Task.FromResult(false);
            }
            if (string.IsNullOrEmpty(content.FullText))
            {
                context.Output = ContentEditResult.Error(ContentsMessages.Content_FullContentEmpty);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public virtual async Task<bool> SelectExistingContent(
            PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            TContent content = context.Input.Content;
            
            _existingContent = await _contentQueries.SelectOne(
                false, DataAmount.DescriptionOnly, 
                x => EqualityComparer<TKey>.Default.Equals(content.ContentId, x.ContentId))
                .ConfigureAwait(false);
            if (_existingContent == null)
            {
                context.Output = ContentEditResult.ContentNotFound();
                return false;
            }

            return true;
        }

        public virtual Task<bool> IncrementVersion(
            PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            TContent content = context.Input.Content;
            content.Version++;
            return Task.FromResult(true);
        }


        public virtual async Task<bool> InsertToSearchEngine
            (PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            TContent content = context.Input.Content;
            bool doIndex = ShouldIndex(context);

            if (doIndex && _existingContent.IsIndexed == false)
            {
                await _searchQueries.Insert(new List<object> { content }).ConfigureAwait(false);
            }
            else if (doIndex == false && _existingContent.IsIndexed)
            {
                await _searchQueries.Delete(new List<object> { content }).ConfigureAwait(false);
            }
            else if (doIndex && _existingContent.IsIndexed)
            {
                await _searchQueries.Update(new List<object> { content }).ConfigureAwait(false);
            }
            
            content.IsIndexed = doIndex;
            return true;
        }

        public virtual async Task<bool> UpdateContentDb(
            PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            TContent content = context.Input.Content;

            //make sure PublishTime is unique for continuation queries.
            if(_existingContent.PublishTimeUtc != content.PublishTimeUtc)
            {
                content.PublishTimeUtc = content.PublishTimeUtc
                    .AddMilliseconds(-content.PublishTimeUtc.Millisecond)
                    .AddMilliseconds(_random.Next(1000));
            }

            OperationStatus updateResult = await _contentQueries.UpdateOne(
                content, context.Input.Content.Version, context.Input.CheckVersion)
                .ConfigureAwait(false);

            if (updateResult == OperationStatus.NotFound)
            {
                context.Output = ContentEditResult.ContentNotFound();
                return false;
            }
            else if (updateResult == OperationStatus.VersionChanged)
            {
                context.Output = ContentEditResult.VersionChanged();
                return false;
            }
            else if (updateResult != OperationStatus.Success)
            {
                throw new NotImplementedException($"Not supported {nameof(updateResult)} with value of {updateResult} received.");
            }

            return true;
        }
        

    }
}
