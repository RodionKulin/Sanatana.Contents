﻿using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sanatana.Contents;
using Sanatana.Patterns.Pipelines;
using Sanatana.Contents.YouTube.Resources;
using Sanatana.Contents.Pipelines.Contents;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Selectors.Permissions;
using Sanatana.Contents.Pipelines;
using Sanatana.Contents.Search;
using Sanatana.Contents.Html.Media;
using Sanatana.Contents.Files.Queries;
using Sanatana.Contents.Database;
using Sanatana.Contents.Utilities;
using Sanatana.Contents.Files.Services;

namespace Sanatana.Contents
{
    public class ImportYoutubeContentPipeline<TKey, TCategory, TContent>
        : ContentPipelineBase<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        //fields
        protected string _youtubeId;



        //init
        public ImportYoutubeContentPipeline(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
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
            Register(ValidateType);
            Register(ParseYoutubeId);
            Register(SanitizeTitle);
            Register(CreateUrl);
            Register(CreateHtmlContent);
            Register(UpdateContentSearch);
            Register(InsertContentDb);
        }

        public override Task<ContentEditResult> Execute(
            ContentEditParams<TKey, TContent> inputModel, ContentEditResult outputModel)
        {
            outputModel = outputModel ?? ContentEditResult.Success();
            return base.Execute(inputModel, outputModel);
        }

        public override async Task RollBack(
            PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            await base.RollBack(context);

            if (context.Exceptions != null)
            {
                _exceptionHandler.Handle(context);
            }
            if (context.Output == null)
            {
                context.Output = ContentEditResult.Error(ContentsMessages.Common_ProcessingError);
            }

            return;
        }


        //steps
        public virtual Task<bool> ValidateType(
            PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            bool isYoutubePost = context.Input.Content is YoutubePost<TKey>;
            if (!isYoutubePost)
            {
                throw new ArgumentException($"Unexpected Content type {context.Input.Content.GetType()} instead of {nameof(YoutubePost<TKey>)}.");
            }

            return Task.FromResult(true);
        }

        public virtual Task<bool> ParseYoutubeId(
           PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            YoutubePost<TKey> youtubePost = context.Input.Content as YoutubePost<TKey>;

            Regex youtubeIdRegex = new Regex(@"/watch\?v=([A-Za-z0-9\-]+)");
            Match idMatch = youtubeIdRegex.Match(youtubePost.YoutubeUrl);
            if (idMatch.Groups.Count < 2)
            {
                context.Output = ContentEditResult.Error(YouTubeMessages.GetPreviewImageException);
                return Task.FromResult(false);
            }

            _youtubeId = idMatch.Groups[1].Value;
            return Task.FromResult(true);
        }

        public virtual Task<bool> CreateHtmlContent(
            PipelineContext<ContentEditParams<TKey, TContent>, ContentEditResult> context)
        {
            YoutubePost<TKey> youtubePost = context.Input.Content as YoutubePost<TKey>;

            youtubePost.FullText = $"<p style=\"text-align: center;\"><iframe allowfullscreen=\"\" frameborder=\"0\" height=\"360\" scrolling=\"no\" src=\"//www.youtube.com/v/{_youtubeId}\" width=\"640\"></iframe></p>";
            youtubePost.ShortText = youtubePost.FullText;

            return Task.FromResult(true);
        }
        

    }
}
