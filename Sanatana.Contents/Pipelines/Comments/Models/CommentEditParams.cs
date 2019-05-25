using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Contents.Pipelines.Comments
{
    public class CommentEditParams<TKey, TComment>
        where TKey : struct
        where TComment : Comment<TKey>
    {
        //properties
        public TComment Comment { get; set; }
        public List<string> AllowedIFrameUrls { get; set; }
        public TKey? UserId { get; set; }
        public int Permission { get; set; }



        //init
        public CommentEditParams()
        {
            AllowedIFrameUrls = new List<string>()
            {
                @"youtube.\w*"
            };
        }
    }
}
