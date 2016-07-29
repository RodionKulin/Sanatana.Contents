using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContentManagementBackend
{
    public class CommentStateProvider : ICommentStateProvider
    {


        //методы
        public CommentState Inserted(bool isPublic)
        {
            return isPublic ? CommentState.Inserted : CommentState.AuthorizationRequired;
        }
        public List<CommentState> List(bool isPublic)
        {
            return isPublic
                ? new List<CommentState>() { CommentState.Inserted, CommentState.Moderated }
                : new List<CommentState>() { CommentState.AuthorizationRequired, CommentState.AuthorizationRequiredModerated };
        }
        public CommentState Updated(bool isPublic)
        {
            return isPublic ? CommentState.Inserted : CommentState.AuthorizationRequired;
        }
        public CommentState Deleted(bool isPublic)
        {
            return CommentState.Deleted;
        }
    }
}