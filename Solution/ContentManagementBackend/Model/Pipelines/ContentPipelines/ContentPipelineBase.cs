using ContentManagementBackend.Resources;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Utility.Pipelines;

namespace ContentManagementBackend
{
    public abstract class ContentPipelineBase<TKey> : Pipeline<ContentPipelineModel<TKey>, ContentPipelineResult>
        where TKey : struct
    {
        //поля
        protected IContentQueries<TKey> _contentQueries;
        protected PreviewImageQueries _previewImageQueries;
        protected ContentImageQueries _contentImageQueries;
        protected ICategoryManager<TKey> _categoryManager;
        protected ISearchQueries<TKey> _searchQueries;


        //свойства
        public HtmlElement FullContentDocument { get; set; }
        public HtmlElement ShortContentDocument { get; set; }



        //инициализация
        public ContentPipelineBase(IContentQueries<TKey> contentQueries, PreviewImageQueries previewImageQueries
            , ContentImageQueries contentImageQueries, ICategoryManager<TKey> categoryManager
            , ISearchQueries<TKey> searchQueries)
        {
            _contentQueries = contentQueries;
            _previewImageQueries = previewImageQueries;
            _contentImageQueries = contentImageQueries;
            _categoryManager = categoryManager;
            _searchQueries = searchQueries;
        }



        //методы
        public virtual Task<bool> SetPreviewImageUrl(
          PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentSubmitVM contentVM = context.Input.ContentVM;
            ContentBase<TKey> content = context.Input.Content;

            content.Url = Translit.RussianToTranslitUrl(content.Title);
            context.Output.ImageUrl = _previewImageQueries.GetImageUrl(content, contentVM, false);

            return Task.FromResult(true);
        }

        public virtual async Task<bool> CheckPermission(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentSubmitVM contentVM = context.Input.ContentVM;
            ContentBase<TKey> content = context.Input.Content;

            MessageResult<Category<TKey>> permissinoResult =
                await _categoryManager.CheckPermission(content.CategoryID
                , context.Input.Permission, context.Input.UserRoles);

            if (permissinoResult.HasExceptions)
            {
                context.Output = ContentPipelineResult.Fail(permissinoResult.Message);
                return false;
            }

            return true;
        }

        public virtual Task<bool> SanitizeTitle(
          PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            content.Title = HtmlTagRemover.StripHtml(content.Title);

            if (content.Title.Length > context.Input.MaxTitleLength)
                content.Title = content.Title.Substring(0, context.Input.MaxTitleLength);

            return Task.FromResult(true);
        }

        public virtual Task<bool> CreateUrl(
           PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            content.Url = Translit.RussianToTranslitUrl(content.Title);

            return Task.FromResult(true);
        }

        public virtual Task<bool> SanitizeFullContent(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            HtmlSanitizer sanitizer = new HtmlSanitizer()
            {
                AllowedIFrameUrls = context.Input.AllowedIFrameUrls
            };

            if (FullContentDocument == null)
                FullContentDocument = HtmlParser.Parse(content.FullContent);
            sanitizer.Sanitize(FullContentDocument);

            content.FullContent = FullContentDocument.ToString();
            return Task.FromResult(true);
        }

        public virtual Task<bool> FixIframeSrc(
         PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            if (FullContentDocument == null)
                FullContentDocument = HtmlParser.Parse(content.FullContent);

            if (ShortContentDocument == null)
                ShortContentDocument = HtmlParser.Parse(content.ShortContent ?? string.Empty);

            List<HtmlElement> contentElements = new List<HtmlElement>()
            {
                FullContentDocument,
                ShortContentDocument
            };

            foreach (HtmlElement document in contentElements)
            {
                List<HtmlElement> iframes =
                    HtmlMediaExtractor.FindElementsOfType(document, "iframe");

                foreach (HtmlElement item in iframes)
                {
                    HtmlAttribute srcAttr = item.Attributes.FirstOrDefault(p => p.Name == "src");
                    if (srcAttr == null || srcAttr.Value == null)
                        continue;

                    srcAttr.Value = srcAttr.Value.Replace("/embed/", "/v/");
                }
            }

            content.FullContent = FullContentDocument.ToString();
            content.ShortContent = ShortContentDocument.ToString();
            return Task.FromResult(true);
        }

        public virtual async Task<bool> ReplaceContentTempImagesSrc(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;
            string contentID = content.ContentID.ToString();
            
            if (FullContentDocument == null)
                FullContentDocument = HtmlParser.Parse(content.FullContent);

            if (ShortContentDocument == null)
                ShortContentDocument = HtmlParser.Parse(content.ShortContent ?? string.Empty);
            
            List<HtmlElement> contentElements = new List<HtmlElement>()
            {
                FullContentDocument,
                ShortContentDocument
            };
            
            bool srcUpdated = await _contentImageQueries.MoveTempToStatic(contentElements, contentID);
            if (!srcUpdated)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Content_ImageException);
                return false;
            }
                        
            content.FullContent = FullContentDocument.ToString();
            content.ShortContent = ShortContentDocument.ToString();
            return true;
        }
        
        public virtual Task<bool> CreateShortContent(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            if (FullContentDocument == null)
                FullContentDocument = HtmlParser.Parse(content.FullContent);

            content.ShortContent = HtmlTagRemover.StripHtml(FullContentDocument);

            content.ShortContent = ContentMinifier.Minify(content.ShortContent,
               context.Input.MaxShortContentLength, ContentMinifyMode.ToClosestDotAtRight);

            return Task.FromResult(true);
        }

        public virtual Task<bool> SanitizeShortContent(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            HtmlSanitizer sanitizer = new HtmlSanitizer()
            {
                AllowedIFrameUrls = context.Input.AllowedIFrameUrls
            };

            if (ShortContentDocument == null)
                ShortContentDocument = HtmlParser.Parse(content.ShortContent ?? string.Empty);
            
            sanitizer.Sanitize(ShortContentDocument);

            content.ShortContent = ShortContentDocument.ToString();
            return Task.FromResult(true);
        }
        
        public virtual async Task<bool> InsertContent(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            content.CreateUpdateNonce();
            content.AddTimeUtc = DateTime.UtcNow;

            QueryResult<bool> isUnique = await _contentQueries.Insert(content);

            if (isUnique.HasExceptions)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Common_DatabaseException);
                return false;
            }
            else if (!isUnique.Result)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.ContentUrlIsUsed);
                return false;
            }

            return true;
        }

        public virtual async Task<bool> IndexInSearchEngine(
           PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            ContentBase<TKey> content = context.Input.Content;

            MessageResult<bool> isPublicCategoryResult =
               await _categoryManager.CheckIsPublic(content.CategoryID, context.Input.Permission);
            if (isPublicCategoryResult.HasExceptions)
            {
                context.Output = ContentPipelineResult.Fail(isPublicCategoryResult.Message);
                return false;
            }

            bool doIndex = content.IsPublished && isPublicCategoryResult.Result
                && content.PublishTimeUtc <= DateTime.UtcNow;
            if (!doIndex)
            {
                return true;
            }

            bool completed = await _searchQueries.Insert(content);
            if (!completed)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.Common_SearchException);
                return false;
            }

            content.IsIndexed = doIndex;
            return true;
        }
    }
}
