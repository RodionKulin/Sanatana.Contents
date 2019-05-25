using Sanatana.Contents.Objects;
using Sanatana.Contents.Objects.Entities;
using Sanatana.Contents.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Selectors.Contents
{
    public class ContentEditVM<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        //properties
        public OperationStatus UpdateResult { get; set; }
        public List<string> Errors { get; set; }
        public TContent Content { get; set; }
        public List<TCategory> Categories { get; set; }
        public bool IsEditPage { get; set; }



        //init
        public ContentEditVM()
        {
        }

        public static ContentEditVM<TKey, TCategory, TContent> Success(TContent content
            , List<TCategory> categories, bool isEditPage)
        {
            return new ContentEditVM<TKey, TCategory, TContent>()
            {
                Content = content,
                Categories = categories,
                IsEditPage = isEditPage
            };
        }

        public static ContentEditVM<TKey, TCategory, TContent> NotFound(
            List<TCategory> categories, bool isEditPage)
        {
            return new ContentEditVM<TKey, TCategory, TContent>()
            {
                UpdateResult = OperationStatus.NotFound,
                Errors = new List<string> { ContentsMessages.Content_NotFound },
                IsEditPage = isEditPage,
                Categories = categories
            };
        }
        public static ContentEditVM<TKey, TCategory, TContent> PermissionDenied(
            List<TCategory> categories, bool isEditPage)
        {
            return new ContentEditVM<TKey, TCategory, TContent>
            {
                UpdateResult = OperationStatus.PermissionDenied,
                Errors = new List<string> { ContentsMessages.Common_AuthorizationRequired },
                IsEditPage = isEditPage,
                Categories = categories
            };
        }

    }
}
