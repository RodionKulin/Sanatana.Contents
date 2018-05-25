using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents
{
    public static class ContentsConstants
    {
        //cache
        public static readonly TimeSpan DEFAULT_CACHE_EXPIRY_PERIOD = TimeSpan.FromHours(2);


        //content
        public const int DEFAULT_MAX_CONTENT_SHORT_CONTENT_LENGTH = 200;
        public const int DEFAULT_MAX_CONTENT_TITLE_LENGTH = 80;
        public const int DEFAULT_MAX_CATEGORY_NAME_LENGTH = 80;
        

        //regular jobs
        public const int DEFAULT_INDEX_FUTURE_CONTENTS_SELECT_COUNT = 100;
    }
}
