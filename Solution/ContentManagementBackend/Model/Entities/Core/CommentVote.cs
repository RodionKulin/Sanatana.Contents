using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public class CommentVote<TKey>
        where TKey : struct
    {
        public TKey CommentVoteID { get; set; }
        public TKey CommentID { get; set; }
        public TKey UserID { get; set; }
        public bool IsUp { get; set; }
    }
}
