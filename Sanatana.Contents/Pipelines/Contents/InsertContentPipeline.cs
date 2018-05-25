﻿using Sanatana.Contents.Resources;
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
using Sanatana.Contents.Html;
using Sanatana.Contents.Utilities;

namespace Sanatana.Contents.Pipelines.Contents
{
    public class InsertContentPipeline<TKey, TCategory, TContent> 
        : ContentPipelineBase<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        //init
        public InsertContentPipeline(IPipelineExceptionHandler exceptionHandler, IContentQueries<TKey, TContent> contentQueries
            , IPermissionSelector<TKey, TCategory> permissionSelector, ISearchQueries<TKey> searchQueries
            , IImageFileQueries imageFileQueries, IHtmlMediaExtractor htmlMediaExtractor, IUrlEncoder urlEncoder)
            : base(exceptionHandler, contentQueries, permissionSelector
                  , searchQueries, imageFileQueries, htmlMediaExtractor, urlEncoder)
        {
            RegisterModules();
        }



        //bootstrap
        protected virtual void RegisterModules()
        {
            Register(CheckPermission);
            Register(ValidateInput);
            Register(SanitizeTitle);
            Register(CreateUrl);
            Register(SanitizeFullContent);
            Register(FixIframeSrc);
            Register(ReplaceContentTempImagesSrc);
            Register(SanitizeShortContent);
            Register(UpdateContentSearch);
            Register(InsertContentDb);
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


    }
}
