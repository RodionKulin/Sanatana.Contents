using Sanatana.Contents.Database;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Files.Services;
using Sanatana.Contents.Html;
using Sanatana.Contents.Html.HtmlNodes;
using Sanatana.Contents.Html.Media;
using Sanatana.Contents.Html.Minifier;
using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.DTOs;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Resources;
using Sanatana.Contents.Search;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Contents.Utilities;
using Sanatana.Patterns.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Contents
{
    public abstract class ContentPipelineBase<TKey, TCategory, TContent>
        : Pipeline<ContentUpdateParams<TKey, TContent>, ContentUpdateResult>, 
        IContentPipelineBase<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        //fields
        protected static Random _random = new Random(DateTime.UtcNow.Millisecond);
        protected IPipelineExceptionHandler _exceptionHandler;
        protected IContentQueries<TKey, TContent> _contentQueries;
        protected ISearchQueries<TKey> _searchQueries;
        protected IPermissionSelector<TKey, TCategory> _permissionSelector;
        protected IUrlEncoder _urlEncoder;
        protected IImageFileService _imageFileService;
        protected IHtmlMediaExtractor _htmlMediaExtractor;
        protected HtmlElement _fullContentDocument;
        protected HtmlElement _shortContentDocument;



        //init
        public ContentPipelineBase(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
            , IPermissionSelector<TKey, TCategory> permissionSelector, ISearchQueries<TKey> searchQueries
            , IImageFileService imageFileService, IHtmlMediaExtractor htmlMediaExtractor, IUrlEncoder urlEncoder)
        {
            _exceptionHandler = exceptionHandler;
            _contentQueries = contentQueries;
            _permissionSelector = permissionSelector;
            _searchQueries = searchQueries;
            _imageFileService = imageFileService;
            _htmlMediaExtractor = htmlMediaExtractor;
            _urlEncoder = urlEncoder;
        }


        //execute
        public override Task<ContentUpdateResult> Execute(
            ContentUpdateParams<TKey, TContent> inputModel, ContentUpdateResult outputModel)
        {
            outputModel = outputModel ?? ContentUpdateResult.Success();
            return base.Execute(inputModel, outputModel);
        }

        public override async Task RollBack(
            PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
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



        //methods
        public virtual async Task<bool> CheckPermission(
            PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            bool hasPermission = await _permissionSelector
                .CheckIsAllowed(content.CategoryId, context.Input.Permission, context.Input.UserId)
                .ConfigureAwait(false);

            if (hasPermission == false)
            {
                context.Output = ContentUpdateResult.PermissionDenied();
                return false;
            }

            return true;
        }

        public virtual Task<bool> SanitizeTitle(
          PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            content.Title = HtmlTagRemover.StripHtml(content.Title);

            if (content.Title.Length > context.Input.MaxTitleLength)
                content.Title = content.Title.Substring(0, context.Input.MaxTitleLength);

            return Task.FromResult(true);
        }

        public virtual Task<bool> CreateUrl(
           PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            content.Url = _urlEncoder.Encode(content.Title)
                .ToLowerInvariant();        

            return Task.FromResult(true);
        }

        public virtual Task<bool> SanitizeFullContent(
            PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            HtmlSanitizer sanitizer = new HtmlSanitizer()
            {
                AllowedIFrameUrls = context.Input.AllowedIFrameUrls
            };

            if (_fullContentDocument == null)
                _fullContentDocument = HtmlParser.Parse(content.FullText);
            sanitizer.Sanitize(_fullContentDocument);

            content.FullText = _fullContentDocument.ToString();
            return Task.FromResult(true);
        }

        public virtual Task<bool> FixIframeSrc(
            PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            if (_fullContentDocument == null)
                _fullContentDocument = HtmlParser.Parse(content.FullText);
            if (_shortContentDocument == null)
                _shortContentDocument = HtmlParser.Parse(content.ShortText ?? string.Empty);
            List<HtmlElement> contentElements = new List<HtmlElement>()
            {
                _fullContentDocument,
                _shortContentDocument
            };

            foreach (HtmlElement document in contentElements)
            {
                List<HtmlElement> iframes =
                    _htmlMediaExtractor.FindElementsOfType(document, "iframe");

                foreach (HtmlElement item in iframes)
                {
                    HtmlAttribute srcAttr = item.Attributes.FirstOrDefault(p => p.Name == "src");
                    if (srcAttr == null || srcAttr.Value == null)
                        continue;

                    srcAttr.Value = srcAttr.Value.Replace("/embed/", "/v/");
                }
            }

            content.FullText = _fullContentDocument.ToString();
            content.ShortText = _shortContentDocument.ToString();
            return Task.FromResult(true);
        }

        public virtual async Task<bool> ReplaceContentTempImagesSrc(
            PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            if(context.Input.ContentImagesPathProviderId == null)
            {
                throw new KeyNotFoundException($"No {nameof(context.Input.ContentImagesPathProviderId)} specified. Content images can not be processed.");
            }

            TContent content = context.Input.Content;
            string contentId = content.ContentId.ToString();
            
            if (_fullContentDocument == null)
                _fullContentDocument = HtmlParser.Parse(content.FullText);
            if (_shortContentDocument == null)
                _shortContentDocument = HtmlParser.Parse(content.ShortText ?? string.Empty);            
            List<HtmlElement> contentElements = new List<HtmlElement>()
            {
                _fullContentDocument,
                _shortContentDocument
            };

            int pathProviderId = context.Input.ContentImagesPathProviderId.Value;
            await _imageFileService.UpdateContentImages(pathProviderId, contentElements, contentId).ConfigureAwait(false);
          
            content.FullText = _fullContentDocument.ToString();
            content.ShortText = _shortContentDocument.ToString();
            return true;
        }
        
        public virtual Task<bool> CreateShortContent(
            PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            if (_fullContentDocument == null)
                _fullContentDocument = HtmlParser.Parse(content.FullText);

            content.ShortText = HtmlTagRemover.StripHtml(_fullContentDocument);

            content.ShortText = ContentMinifier.Minify(content.ShortText,
               context.Input.MaxShortTextLength, ContentMinifyMode.ToClosestDotAtRight);

            return Task.FromResult(true);
        }

        public virtual Task<bool> SanitizeShortContent(
            PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            HtmlSanitizer sanitizer = new HtmlSanitizer()
            {
                AllowedIFrameUrls = context.Input.AllowedIFrameUrls
            };

            if (_shortContentDocument == null)
                _shortContentDocument = HtmlParser.Parse(content.ShortText ?? string.Empty);
            
            sanitizer.Sanitize(_shortContentDocument);

            content.ShortText = _shortContentDocument.ToString();
            return Task.FromResult(true);
        }

        public virtual async Task<bool> InsertContentDb(
            PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            //make sure PublishTime is unique for continuation queries.
            content.PublishedTimeUtc = content.PublishedTimeUtc
                .AddMilliseconds(-content.PublishedTimeUtc.Millisecond)
                .AddMilliseconds(_random.Next(1000));

            ContentInsertResult insertResult = await _contentQueries
                .InsertOne(content)
                .ConfigureAwait(false);

            if (insertResult == ContentInsertResult.UrlIsNotUnique)
            {
                context.Output = ContentUpdateResult.Error(ContentsMessages.Content_UrlIsNotUnique);
                return false;
            }
            else if (insertResult == ContentInsertResult.PublishTimeUtcIsNotUnique)
            {
                context.Output = ContentUpdateResult.Error(ContentsMessages.Content_PublishTimeIsNotUnique);
                return false;
            }

            return true;
        }

        public virtual async Task<bool> UpdateContentSearch(
           PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;
            bool doIndex = ShouldIndex(context);

            if (doIndex == true)
            {
                await _searchQueries.Insert(new List<object> { content }).ConfigureAwait(false);
            }
            else if (doIndex == false && content.IsIndexed == true)
            {
                await _searchQueries.Delete(new List<object> { content }).ConfigureAwait(false);
            }

            content.IsIndexed = doIndex;
            return true;
        }

        protected bool ShouldIndex(PipelineContext<ContentUpdateParams<TKey, TContent>, ContentUpdateResult> context)
        {
            TContent content = context.Input.Content;

            bool doIndex = content.NeverIndex == false
               && content.PublishedTimeUtc <= DateTime.UtcNow;

            return doIndex;
        }
    }
}
