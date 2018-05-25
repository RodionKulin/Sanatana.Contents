using Sanatana.Contents.Objects.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.Contents.Objects.DTOs
{
    public class CommentJoinResult<TKey, TComment, TContent>
        where TKey : struct
        where TComment : Comment<TKey>
        where TContent : Content<TKey>
    {
        public TComment Comment { get; set; }
        public TContent Content { get; set; }
    }
}
