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
    public class ImportYoutubeContentPipeline<TKey> : ContentPipelineBase<TKey>
        where TKey : struct
    {
        //поля
        protected string _youtubeID;



        //инициализация
        public ImportYoutubeContentPipeline(IContentQueries<TKey> contentQueries
            , PreviewImageQueries previewImageQueries, ContentImageQueries contentImageQueries
            , ICategoryManager<TKey> categoryManager, ISearchQueries<TKey> searchQueries)
            : base(contentQueries, previewImageQueries, contentImageQueries, categoryManager, searchQueries)
        {
            RegisterModules();
        }



        //методы
        protected virtual void RegisterModules()
        {
            Register(ValidateType);
            Register(ParseYoutubeID);
            Register(SanitizeTitle);
            Register(CreateUrl);
            Register(CreateHtmlContent);
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
        public virtual Task<bool> ValidateType(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            bool isYoutubePost = context.Input.Content is YoutubePost<TKey>;
            if (!isYoutubePost)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.YoutubePost_WrongType);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public virtual Task<bool> ParseYoutubeID(
           PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            YoutubePost<TKey> youtubePost = context.Input.Content as YoutubePost<TKey>;

            Regex youtubeIDRegex = new Regex(@"/watch\?v=([A-Za-z0-9\-]+)");
            Match idMatch = youtubeIDRegex.Match(youtubePost.YoutubeUrl);
            if (idMatch.Groups.Count < 2)
            {
                context.Output = ContentPipelineResult.Fail(MessageResources.YoutubePost_GetPreviewImageException);
                return Task.FromResult(false);
            }

            _youtubeID = idMatch.Groups[1].Value;
            return Task.FromResult(true);
        }

        public virtual Task<bool> CreateHtmlContent(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            YoutubePost<TKey> youtubePost = context.Input.Content as YoutubePost<TKey>;

            youtubePost.FullContent = $"<p style=\"text-align: center;\"><iframe allowfullscreen=\"\" frameborder=\"0\" height=\"360\" scrolling=\"no\" src=\"//www.youtube.com/v/{_youtubeID}\" width=\"640\"></iframe></p>";
            youtubePost.ShortContent = youtubePost.FullContent;

            return Task.FromResult(true);
        }

        public virtual async Task<bool> SavePreviewImage(
            PipelineContext<ContentPipelineModel<TKey>, ContentPipelineResult> context)
        {
            YoutubePost<TKey> youtubePost = context.Input.Content as YoutubePost<TKey>;
            string previewImageUrl = $"img.youtube.com/vi/{_youtubeID}/0.jpg";

            PipelineResult<List<ImagePipelineResult>> imageResult =
               await _previewImageQueries.CreateStaticImage(previewImageUrl, youtubePost.Url);
            if (!imageResult.Result)
            {
                context.Output = ContentPipelineResult.Fail(imageResult.Message);
                return false;
            }

            return true;
        }
        


    }
}
