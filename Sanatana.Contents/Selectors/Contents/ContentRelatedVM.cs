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
    public class ContentRelatedVM<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {
        //properties
        public List<string> Errors { get; set; } = new List<string>();
        public OperationStatus Status { get; set; }
        public TContent Content { get; set; }
        public TContent NextContent { get; set; }
        public TContent PreviousContent { get; set; }
        public List<TContent> CategoryLatestContent { get; set; }
        public List<TContent> CategoryTopViewsContent { get; set; }
        public TCategory ContentCategory { get; set; }
        public List<TCategory> AllCategories { get; set; }



        //init
        public ContentRelatedVM()
        {
        }

        public static ContentRelatedVM<TKey, TCategory, TContent> Success(TContent content, TCategory contentCategory, List<TCategory> allAllowedCategories)
        {
            return new ContentRelatedVM<TKey, TCategory, TContent>
            {
                Status = OperationStatus.Success,
                Content = content,
                ContentCategory = contentCategory,
                AllCategories = allAllowedCategories
            };
        }

        public static ContentRelatedVM<TKey, TCategory, TContent> Error(string error)
        {
            return new ContentRelatedVM<TKey, TCategory, TContent>
            {
                Errors = new List<string>() { error },
                Status = OperationStatus.Error
            };
        }

        public static ContentRelatedVM<TKey, TCategory, TContent> NotFound()
        {
            return new ContentRelatedVM<TKey, TCategory, TContent>
            {
                Errors = new List<string> { ContentsMessages.Content_NotFound },
                Status = OperationStatus.NotFound
            };
        }

        public static ContentRelatedVM<TKey, TCategory, TContent> PermissionDenied()
        {
            return new ContentRelatedVM<TKey, TCategory, TContent>
            {
                Errors = new List<string> { ContentsMessages.Common_AuthorizationRequired },
                Status = OperationStatus.PermissionDenied
            };
        }
    }
}
