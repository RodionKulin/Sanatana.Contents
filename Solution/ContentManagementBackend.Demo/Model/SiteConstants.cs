using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContentManagementBackend.Demo
{
    public class SiteConstants
    {
        //brand
        public const string SITE_BRAND = "ContentManagementBackend"; 
        public const int SITE_FOUNDATION_YEAR = 2016;

        //content pages
        public const int POSTS_LIST_PAGE_SIZE = 10;
        public const int POSTS_AJAX_LIST_PAGE_SIZE = 10;
        public const int POSTS_FULL_CATEGORY_LIST_COUNT = 4;
        public const int POSTS_MODERATION_PAGE_SIZE = 9;
        public const int LATEST_COMMENTS_COUNT = 5;
        public const int SEARCH_LIST_PAGE_SIZE = 10;
        public static readonly TimeSpan POSTS_POPULAR_PERIOD = TimeSpan.FromDays(7);
        public const int POSTS_POPULAR_COUNT = 10;
        public const bool USE_ALL_POSTS_TO_MATCH_POPULAR_COUNT = true;

        //urls
        public const string URL_BASE_FULL_POST = "/post/";
        public const string URL_BASE_EDIT_POST = "/posts/edit/";
        public const string URL_BASE_CATEGORY = "/category/";
        public const string URL_ULOGIN_CALLBACK_FORMAT = "{0}/Account/ULoginCallback";

        //external urls
        public const string URL_ULOGIN_REQUEST = "http://ulogin.ru/token.php?token={0}&host={1}";

        //claims
        public const string CLAIM_NAME_USER_AVATAR = "U:Avatar";
        public static readonly List<IdentityUserRole> STUFF_ROLES = 
            new List<IdentityUserRole> { IdentityUserRole.Author, IdentityUserRole.Moderator, IdentityUserRole.Admin };


    }
}