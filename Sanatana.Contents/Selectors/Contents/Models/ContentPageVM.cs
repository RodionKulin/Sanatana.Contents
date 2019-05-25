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
        /// <summary>
        /// Status of database queries and permission validation completion.
        /// </summary>
        public OperationStatus Status { get; set; }
        /// <summary>
        /// Errors describing Status if it is different from OperationStatus.Success.
        /// </summary>
        public List<string> Errors { get; set; }
        /// <summary>
        /// Selected page number.
        /// </summary>
        public long Page { get; set; }
        /// <summary>
        /// Number of items on the page.
        /// </summary>
        public long PageSize { get; set; }
        /// <summary>
        /// Total number of Content items in selected categories with respect to provided filter parameter.
        /// </summary>
        public long TotalItems { get; set; }
        /// <summary>
        /// PublishTimeUtc of latest Content returned. Can be used to select next Content after that PublishDataUtc on next continuation request.
        /// </summary>
        public string LastPublishTimeUtcIso8601 { get; set; }
        /// <summary>
        /// Message about current and total number of pages.
        /// </summary>
        public string ContentNumberMessage { get; set; }
        public List<TContent> Contents { get; set; }
        /// <summary>
        /// All categories that provided user has permission to access.
        /// </summary>
        public List<TCategory> AllCategories { get; set; }
        /// <summary>
        /// Selected categories that provided user has permission to access.
        /// </summary>
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
