using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Contents
{
    public class ContentUpdateParams<TKey, TContent>
        where TKey : struct
        where TContent : Content<TKey>
    {
        //properties
        public TContent Content { get; set; }
        public int MaxTitleLength { get; set; } = ContentsConstants.DEFAULT_MAX_CONTENT_TITLE_LENGTH;
        public int MaxShortTextLength { get; set; } = ContentsConstants.DEFAULT_MAX_CONTENT_SHORT_CONTENT_LENGTH;
        public List<string> AllowedIFrameUrls { get; set; }
        public int Permission { get; set; }
        public TKey? UserId { get; set; }
        public int? ContentImagesPathProviderId { get; set; }
        public bool CheckVersion { get; set; }



        //init
        public ContentUpdateParams()
        {
            AllowedIFrameUrls = new List<string>()
            {
                @"youtube.\w*"
            };
        }
    }
}
