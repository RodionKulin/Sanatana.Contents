using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class CommentVM<TKey>
        where TKey : struct
    {
        public TKey? CommentID { get; set; }
        public TKey ContentID { get; set; }
        public string Content { get; set; }
        public TKey? ParentCommentID { get; set; }
        public TKey? BranchCommentID { get; set; }
    }
}
