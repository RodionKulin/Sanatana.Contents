using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.Contents;

namespace Sanatana.Contents.Objects.Entities
{
    public class Comment<TKey>
        where TKey : struct
    {
        //properties
        public TKey CommentId { get; set; }
        public TKey ContentId { get; set; }
        public TKey? AuthorId { get; set; }
        public string Text { get; set; }
        public int State { get; set; }
        public TKey? BranchCommentId { get; set; }
        public TKey? ParentCommentId { get; set; }
        public DateTime CreatedTimeUtc { get; set; }
        public DateTime UpdatedTimeUtc { get; set; }
        public long Version { get; set; }
        public bool NeverIndex { get; set; }
        public bool IsIndexed { get; set; }


        //foreign entities
        public virtual Content<TKey> Content { get; set; }

                
    }
}
