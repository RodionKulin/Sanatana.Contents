using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentManagementBackend
{
    public interface ICommentStateProvider
    {
        CommentState Inserted(bool isPublic);
        List<CommentState> List(bool isPublic);
        CommentState Updated(bool isPublic);
        CommentState Deleted(bool isPublic);
    }
}