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
    public class ContentPageVM<TKey, TCategory, TContent>
        where TKey : struct
        where TCategory : Category<TKey>
        where TContent : Content<TKey>
    {

        //properties
        public OperationStatus Status { get; set; }
        public List<string> Errors { get; set; }
        public long Page { get; set; }
        public long PageSize { get; set; }
        public long TotalItems { get; set; }
        public string LastPublishTimeUtcIso8601 { get; set; }
        public string ContentNumberMessage { get; set; }
        public List<TContent> Contents { get; set; }
        public List<TCategory> AllCategories { get; set; }
        public List<TCategory> SelectedCategories { get; set; }



        //dependent properties
        public long TotalPages
        {
            get
            {
                return (long)Math.Ceiling((double)TotalItems / PageSize);
            }
        }
        public bool IsPageOutOfRange
        {
            get
            {
                bool isZeroItems = TotalPages == 0 && Page == 1;
                return Page > TotalPages && isZeroItems == false;
            }
        }
        public bool IsLastPage
        {
            get
            {
                return Page >= TotalPages;
            }
        }
        public string SelectedCategoryIdsJSArray
        {
            get
            {
                if (SelectedCategories == null)
                    return null;

                List<string> idStrings = SelectedCategories
                    .Select(p => string.Format("'{0}'", p.CategoryId.ToString()))
                    .ToList();

                return string.Format("[{0}]", string.Join(",", idStrings));
            }
        }


        //init
        public ContentPageVM()
        {
            Contents = new List<TContent>();
            AllCategories = new List<TCategory>();
            SelectedCategories = new List<TCategory>();
        }

        public ContentPageVM(string error)
            : this()
        {
            Errors = new List<string>() { error };
        }

        public ContentPageVM(List<string> errors)
            : this()
        {
            Errors = errors;
        }

        public static ContentPageVM<TKey, TCategory, TContent> NotFound()
        {
            return new ContentPageVM<TKey, TCategory, TContent>
            {
                Errors = new List<string> { ContentsMessages.Content_NotFound },
                Status = OperationStatus.NotFound
            };
        }

        public static ContentPageVM<TKey, TCategory, TContent> PermissionDenied()
        {
            return new ContentPageVM<TKey, TCategory, TContent>
            {
                Errors = new List<string> { ContentsMessages.Common_AuthorizationRequired },
                Status = OperationStatus.PermissionDenied
            };
        }
    }
}
