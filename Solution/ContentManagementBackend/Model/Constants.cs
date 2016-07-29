using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public static class Constants
    {
        //cache
        public const string CACHE_CATEGORIES_KEY = "PostCategories";
        public const string CACHE_POPULAR_POSTS_KEY = "PopularPosts";
        public static readonly TimeSpan CACHE_POPULAR_POSTS_EXPIRY_PERIOD = TimeSpan.FromHours(2);
        public const string CACHE_LATEST_POSTS_EACH_CATEGORY_KEY = "LatestFromEachCategory";
        public static readonly TimeSpan CACHE_LATEST_POSTS_EACH_CATEGORY_EXPIRY_PERIOD = TimeSpan.FromMinutes(30);
        public const string CACHE_LATEST_COMMENTS_KEY = "LatestComments";



        //content
        public const int MAX_SHORT_CONTENT_LENGTH = 200;
        public const int MAX_TITLE_LENGTH = 80;
        public const int MAX_TWIT_LENGTH = 140;


        //file storage
        public const string IMAGE_TEMP_FOLDER_PREVIEW = "art-preview-temp";
        public const string IMAGE_STATIC_FOLDER_PREVIEW = "art-preview";
        public const string IMAGE_TEMP_FOLDER_CONTENT = "art-content-temp";
        public const string IMAGE_STATIC_FOLDER_CONTENT = "art-content";
        public const string IMAGE_TEMP_FOLDER_AVATAR = "user-temp";
        public const string IMAGE_STATIC_FOLDER_AVATAR = "user";
        public const string IMAGE_TEMP_FOLDER_COMMENT = "comment-temp";
        public const string IMAGE_STATIC_FOLDER_COMMENT = "comment";
        

        //image settings
        public const string IMAGE_SETTINGS_NAME_CONTENT = "content";
        public const string IMAGE_SETTINGS_NAME_PREVIEW = "preview";
        public const string IMAGE_SETTINGS_NAME_COMMENT = "comment";
    }
}
