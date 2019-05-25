using Sanatana.Contents.Database;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Files.Services;
using Sanatana.Contents.Objects;
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

namespace Sanatana.Contents.Pipelines.Contents
{
    public class DeleteContentPipeline<TKey, TCategory, TContent, TComment>
        : Pipeline<ContentDeleteParams<TKey>, ContentUpdateResult>, 
        IDeleteContentPipeline<TKey, TCategory, TContent, TComment>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>, new()
        where TComment : Comment<TKey>

    {
        //fields
        protected IPipelineExceptionHandler _exceptionHandler;
        protected IContentQueries<TKey, TContent> _contentQueries;
        protected ICommentQueries<TKey, TContent, TComment> _commentQueries;
        protected ISearchQueries<TKey> _searchQueries;
        protected IPermissionSelector<TKey, TCategory> _permissionSelector;
        protected TContent _storedContent;
        protected IFileService _fileService;



        //init
        public DeleteContentPipeline(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
            , ICommentQueries<TKey, TContent, TComment> commentQueries, IPermissionSelector<TKey, TCategory> permissionSelector
            , ISearchQueries<TKey> searchQueries, IFileService fileService)
        {
            _exceptionHandler = exceptionHandler;
            _contentQueries = contentQueries;
            _commentQueries = commentQueries;
            _searchQueries = searchQueries;
            _fileService = fileService;
            _permissionSelector = permissionSelector;

            RegisterModules();
        }



        //bootstrap
        protected virtual void RegisterModules()
        {
            Register(SelectExisting);
            Register(CheckPermission);
            Register(CheckVersion);
            Register(IncrementVersion);
            Register(DeleteContentDb);
            Register(DeleteCommentsDb);
            Register(DeleteContentImages);
            Register(DeleteContentSearch);
        }

        public override Task<ContentUpdateResult> Execute(
            ContentDeleteParams<TKey> inputModel, ContentUpdateResult outputModel)
        {
            outputModel = outputModel ?? ContentUpdateResult.Success();
            return base.Execute(inputModel, outputModel);
        }

        public override async Task RollBack(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            await base.RollBack(context);

            if (context.Exceptions != null)
            {
                _exceptionHandler.Handle(context);
            }
            if (context.Output == null)
            {
                context.Output = ContentUpdateResult.Error(ContentsMessages.Common_ProcessingError);
            }

            return;
        }


        //modules
        public virtual async Task<bool> SelectExisting(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            TKey contentId = context.Input.ContentId;
            TContent contentResult = await _contentQueries.SelectOne(
                false, DataAmount.DescriptionOnly, 
                x => EqualityComparer<TKey>.Default.Equals(x.ContentId, contentId))
                .ConfigureAwait(false);

            if (contentResult == null)
            {
                context.Output = ContentUpdateResult.ContentNotFound();
                return false;
            }

            _storedContent = contentResult;
            return true;
        }

        public virtual async Task<bool> CheckPermission(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            bool hasPermission = await _permissionSelector.CheckIsAllowed(
                _storedContent.CategoryId, context.Input.Permission, context.Input.UserId)
                .ConfigureAwait(false);

            if (hasPermission == false)
            {
                context.Output = ContentUpdateResult.PermissionDenied();
                return false;
            }

            return true;
        }

        public virtual Task<bool> CheckVersion(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            bool versionChangedSinceDataFetched = context.Input.Version < _storedContent.Version;

            if (versionChangedSinceDataFetched && context.Input.CheckVersion)
            {
                context.Output = ContentUpdateResult.VersionChanged();
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
        
        public virtual Task<bool> IncrementVersion(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            context.Input.Version++;
            return Task.FromResult(true);
        }
        
        public virtual async Task<bool> DeleteCommentsDb(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            long commentsDeleted = await _commentQueries
                .DeleteMany(x => EqualityComparer<TKey>.Default.Equals(x.ContentId, context.Input.ContentId))
                .ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> DeleteContentDb(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            List<TKey> contentIds = new List<TKey> { context.Input.ContentId };

            Task<long> contentTask = _contentQueries.DeleteMany(x => contentIds.Contains(x.ContentId));
            await contentTask.ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> DeleteContentImages(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            if (context.Input.ContentImagesPathProviderId == null)
            {
                throw new KeyNotFoundException($"No {nameof(context.Input.ContentImagesPathProviderId)} specified. Content images can not be processed.");
            }

            string contentId = context.Input.ContentId.ToString();

            int pathProviderId = context.Input.ContentImagesPathProviderId.Value;
            await _fileService.DeleteDirectory(pathProviderId, contentId).ConfigureAwait(false);

            return true;
        }

        public virtual async Task<bool> DeleteContentSearch(
            PipelineContext<ContentDeleteParams<TKey>, ContentUpdateResult> context)
        {
            TKey contentId = context.Input.ContentId;
            if (_storedContent.IsIndexed == false)
            {
                return true;
            }

            var content = new TContent
            {
                ContentId = contentId,
                Version = context.Input.Version
            };
            await _searchQueries.Delete(new object[] { content }.ToList()).ConfigureAwait(false);
            
            return true;
        }
    }
}
